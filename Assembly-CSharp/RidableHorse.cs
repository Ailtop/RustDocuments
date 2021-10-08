using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class RidableHorse : BaseRidableAnimal
{
	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 2f;

	public string distanceStatName = "";

	public HorseBreed[] breeds;

	public SkinnedMeshRenderer[] bodyRenderers;

	public SkinnedMeshRenderer[] hairRenderers;

	public int currentBreed = -1;

	public ProtectionProperties riderProtection;

	public ProtectionProperties baseHorseProtection;

	public const Flags Flag_HideHair = Flags.Reserved4;

	public const Flags Flag_WoodArmor = Flags.Reserved5;

	public const Flags Flag_RoadsignArmor = Flags.Reserved6;

	public float equipmentSpeedMod;

	public int numStorageSlots;

	private static Material[] breedAssignmentArray = new Material[2];

	private float distanceRecordingSpacing = 5f;

	public HitchTrough currentHitch;

	public float totalDistance;

	public float kmDistance;

	public float tempDistanceTravelled;

	public int numEquipmentSlots = 4;

	public override float RealisticMass => 550f;

	public override float PositionTickRate
	{
		protected get
		{
			return 0.05f;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RidableHorse.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetStorageSlotCount()
	{
		return numStorageSlots;
	}

	public void ApplyBreed(int index)
	{
		if (currentBreed != index)
		{
			if (index >= breeds.Length || index < 0)
			{
				Debug.LogError("ApplyBreed issue! index is " + index + " breed length is : " + breeds.Length);
			}
			else
			{
				ApplyBreedInternal(breeds[index]);
				currentBreed = index;
			}
		}
	}

	protected void ApplyBreedInternal(HorseBreed breed)
	{
		if (base.isServer)
		{
			SetMaxHealth(StartHealth() * breed.maxHealth);
			base.health = MaxHealth();
		}
	}

	public HorseBreed GetBreed()
	{
		if (currentBreed == -1 || currentBreed >= breeds.Length)
		{
			return null;
		}
		return breeds[currentBreed];
	}

	public override float GetTrotSpeed()
	{
		float num = equipmentSpeedMod / (base.GetRunSpeed() * GetBreed().maxSpeed);
		return base.GetTrotSpeed() * GetBreed().maxSpeed * (1f + num);
	}

	public override float GetRunSpeed()
	{
		float num = base.GetRunSpeed();
		HorseBreed breed = GetBreed();
		return num * breed.maxSpeed + equipmentSpeedMod;
	}

	public override void SetupCorpse(BaseCorpse corpse)
	{
		base.SetupCorpse(corpse);
		HorseCorpse component = corpse.GetComponent<HorseCorpse>();
		if ((bool)component)
		{
			component.breedIndex = currentBreed;
		}
		else
		{
			Debug.Log("no horse corpse");
		}
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		riderProtection.Scale(info.damageTypes);
	}

	public override void OnKilled(HitInfo hitInfo = null)
	{
		TryLeaveHitch();
		base.OnKilled(hitInfo);
	}

	public void SetBreed(int index)
	{
		ApplyBreed(index);
		SendNetworkUpdate();
	}

	public override void LeadingChanged()
	{
		if (!IsLeading())
		{
			TryHitch();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetBreed(Random.Range(0, breeds.Length));
		baseHorseProtection = baseProtection;
		riderProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection.Add(baseHorseProtection, 1f);
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		InvokeRepeating(RecordDistance, distanceRecordingSpacing, distanceRecordingSpacing);
		TryLeaveHitch();
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		CancelInvoke(RecordDistance);
		TryHitch();
	}

	public bool IsHitched()
	{
		return currentHitch != null;
	}

	public void SetHitch(HitchTrough Hitch)
	{
		currentHitch = Hitch;
		SetFlag(Flags.Reserved3, currentHitch != null);
	}

	public override float ReplenishRatio()
	{
		return 1f;
	}

	public override void EatNearbyFood()
	{
		if (Time.time < nextEatTime || (StaminaCoreFraction() >= 1f && base.healthFraction >= 1f))
		{
			return;
		}
		if (IsHitched())
		{
			Item foodItem = currentHitch.GetFoodItem();
			if (foodItem != null && foodItem.amount > 0)
			{
				ItemModConsumable component = foodItem.info.GetComponent<ItemModConsumable>();
				if ((bool)component)
				{
					float amount = component.GetIfType(MetabolismAttribute.Type.Calories) * currentHitch.caloriesToDecaySeconds;
					AddDecayDelay(amount);
					ReplenishFromFood(component);
					foodItem.UseItem();
					nextEatTime = Time.time + Random.Range(2f, 3f) + Mathf.InverseLerp(0.5f, 1f, StaminaCoreFraction()) * 4f;
					return;
				}
			}
		}
		base.EatNearbyFood();
	}

	public void TryLeaveHitch()
	{
		if ((bool)currentHitch)
		{
			currentHitch.Unhitch(this);
		}
	}

	public void TryHitch()
	{
		List<HitchTrough> obj = Pool.GetList<HitchTrough>();
		Vis.Entities(base.transform.position, 2.5f, obj, 256, QueryTriggerInteraction.Ignore);
		foreach (HitchTrough item in obj)
		{
			if (!(Vector3.Dot(Vector3Ex.Direction2D(item.transform.position, base.transform.position), base.transform.forward) < 0.4f) && !item.isClient && item.HasSpace() && item.ValidHitchPosition(base.transform.position) && item.AttemptToHitch(this))
			{
				break;
			}
		}
		Pool.FreeList(ref obj);
	}

	public void RecordDistance()
	{
		BasePlayer driver = GetDriver();
		if (driver == null)
		{
			tempDistanceTravelled = 0f;
			return;
		}
		kmDistance += tempDistanceTravelled / 1000f;
		if (kmDistance >= 1f)
		{
			driver.stats.Add(distanceStatName + "_km", 1, (Stats)5);
			kmDistance -= 1f;
		}
		driver.stats.Add(distanceStatName, Mathf.FloorToInt(tempDistanceTravelled));
		driver.stats.Save();
		totalDistance += tempDistanceTravelled;
		tempDistanceTravelled = 0f;
	}

	public override void MarkDistanceTravelled(float amount)
	{
		tempDistanceTravelled += amount;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.horse = Pool.Get<ProtoBuf.Horse>();
		info.msg.horse.staminaSeconds = staminaSeconds;
		info.msg.horse.currentMaxStaminaSeconds = currentMaxStaminaSeconds;
		info.msg.horse.breedIndex = currentBreed;
		info.msg.horse.numStorageSlots = numStorageSlots;
		if (!info.forDisk)
		{
			info.msg.horse.runState = (int)currentRunState;
			info.msg.horse.maxSpeed = GetRunSpeed();
		}
	}

	public override void OnInventoryDirty()
	{
		EquipmentUpdate();
	}

	public override bool CanAnimalAcceptItem(Item item, int targetSlot)
	{
		ItemModAnimalEquipment component = item.info.GetComponent<ItemModAnimalEquipment>();
		if (targetSlot == -1 && !component)
		{
			return true;
		}
		if (targetSlot < numEquipmentSlots)
		{
			if (component == null)
			{
				return false;
			}
			if (component.slot == ItemModAnimalEquipment.SlotType.Basic)
			{
				return true;
			}
			for (int i = 0; i < numEquipmentSlots; i++)
			{
				Item slot = inventory.GetSlot(i);
				if (slot != null)
				{
					ItemModAnimalEquipment component2 = slot.info.GetComponent<ItemModAnimalEquipment>();
					if (!(component2 == null) && component2.slot == component.slot)
					{
						Debug.Log("rejecting because slot same, found : " + (int)component2.slot + " new : " + (int)component.slot);
						return false;
					}
				}
			}
		}
		return true;
	}

	public int GetStorageStartIndex()
	{
		return numEquipmentSlots;
	}

	public void EquipmentUpdate()
	{
		SetFlag(Flags.Reserved4, false, false, false);
		SetFlag(Flags.Reserved5, false, false, false);
		SetFlag(Flags.Reserved6, false, false, false);
		riderProtection.Clear();
		baseProtection.Clear();
		equipmentSpeedMod = 0f;
		numStorageSlots = 0;
		for (int i = 0; i < numEquipmentSlots; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot == null)
			{
				continue;
			}
			ItemModAnimalEquipment component = slot.info.GetComponent<ItemModAnimalEquipment>();
			if ((bool)component)
			{
				SetFlag(component.WearableFlag, true, false, false);
				if (component.hideHair)
				{
					SetFlag(Flags.Reserved4, true);
				}
				if ((bool)component.riderProtection)
				{
					riderProtection.Add(component.riderProtection, 1f);
				}
				if ((bool)component.animalProtection)
				{
					baseProtection.Add(component.animalProtection, 1f);
				}
				equipmentSpeedMod += component.speedModifier;
				numStorageSlots += component.additionalInventorySlots;
			}
		}
		for (int j = GetStorageStartIndex(); j < inventory.capacity; j++)
		{
			if (j >= GetStorageStartIndex() + numStorageSlots)
			{
				Item slot2 = inventory.GetSlot(j);
				if (slot2 != null)
				{
					slot2.RemoveFromContainer();
					slot2.Drop(base.transform.position + Vector3.up + Random.insideUnitSphere * 0.25f, Vector3.zero);
				}
			}
		}
		inventory.capacity = GetStorageStartIndex() + numStorageSlots;
		SendNetworkUpdate();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.horse != null)
		{
			staminaSeconds = info.msg.horse.staminaSeconds;
			currentMaxStaminaSeconds = info.msg.horse.currentMaxStaminaSeconds;
			numStorageSlots = info.msg.horse.numStorageSlots;
			ApplyBreed(info.msg.horse.breedIndex);
		}
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		return false;
	}

	[ServerVar]
	public static void setHorseBreed(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsDeveloper)
		{
			return;
		}
		int @int = arg.GetInt(0);
		List<RidableHorse> obj = Pool.GetList<RidableHorse>();
		Vis.Entities(basePlayer.eyes.position, basePlayer.eyes.position + basePlayer.eyes.HeadForward() * 5f, 0f, obj);
		foreach (RidableHorse item in obj)
		{
			item.SetBreed(@int);
		}
		Pool.FreeList(ref obj);
	}
}
