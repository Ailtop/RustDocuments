#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class TrainCarUnloadable : TrainCar
{
	public enum WagonType
	{
		Ore = 0,
		Lootboxes = 1,
		Fuel = 2
	}

	[Header("Train Car Unloadable")]
	[SerializeField]
	private GameObjectRef storagePrefab;

	[SerializeField]
	private BoxCollider[] unloadingAreas;

	[SerializeField]
	private TrainCarFuelHatches fuelHatches;

	[SerializeField]
	private Transform orePlaneVisuals;

	[SerializeField]
	private Transform orePlaneColliderDetailed;

	[SerializeField]
	private Transform orePlaneColliderWorld;

	[SerializeField]
	[Range(0f, 1f)]
	public float vacuumStretchPercent = 0.5f;

	[SerializeField]
	private ParticleSystemContainer unloadingFXContainer;

	[SerializeField]
	private ParticleSystem unloadingFX;

	public WagonType wagonType;

	private int lootTypeIndex = -1;

	private List<EntityRef<LootContainer>> lootContainers = new List<EntityRef<LootContainer>>();

	private Vector3 _oreScale = Vector3.one;

	private float animPercent;

	private float prevAnimTime;

	[ServerVar(Help = "How long before an unloadable train car despawns afer being unloaded")]
	public static float decayminutesafterunload = 10f;

	private EntityRef<StorageContainer> storageInstance;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TrainCarUnloadable.OnRpcMessage"))
		{
			if (rpc == 4254195175u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Open "));
				}
				using (TimeWarning.New("RPC_Open"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4254195175u, "RPC_Open", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_Open(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Open");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (old.HasFlag(Flags.Reserved4) != next.HasFlag(Flags.Reserved4) && fuelHatches != null)
		{
			fuelHatches.LinedUpStateChanged(base.LinedUpToUnload);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (IsDead() || base.IsDestroyed)
		{
			return;
		}
		if (child.TryGetComponent<LootContainer>(out var component))
		{
			if (base.isServer)
			{
				component.inventory.SetLocked(!IsEmpty());
			}
			lootContainers.Add(new EntityRef<LootContainer>(component.net.ID));
		}
		if (base.isServer && child.prefabID == storagePrefab.GetEntity().prefabID)
		{
			StorageContainer storageContainer = (StorageContainer)child;
			storageInstance.Set(storageContainer);
			if (!Rust.Application.isLoadingSave)
			{
				FillWithLoot(storageContainer);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseTrain != null)
		{
			lootTypeIndex = info.msg.baseTrain.lootTypeIndex;
			if (base.isServer)
			{
				SetVisualOreLevel(info.msg.baseTrain.lootPercent);
			}
		}
	}

	public bool IsEmpty()
	{
		return GetOrePercent() == 0f;
	}

	public bool TryGetLootType(out TrainWagonLootData.LootOption lootOption)
	{
		return TrainWagonLootData.instance.TryGetLootFromIndex(lootTypeIndex, out lootOption);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		return !IsEmpty();
	}

	public int GetFilledLootAmount()
	{
		if (TryGetLootType(out var lootOption))
		{
			return lootOption.maxLootAmount;
		}
		Debug.LogWarning(GetType().Name + ": Called GetFilledLootAmount without a lootTypeIndex set.");
		return 0;
	}

	public void SetVisualOreLevel(float percent)
	{
		if (!(orePlaneColliderDetailed == null))
		{
			_oreScale.y = Mathf.Clamp01(percent);
			orePlaneColliderDetailed.localScale = _oreScale;
			if (base.isClient)
			{
				orePlaneVisuals.localScale = _oreScale;
				orePlaneVisuals.gameObject.SetActive(percent > 0f);
			}
			if (base.isServer)
			{
				orePlaneColliderWorld.localScale = _oreScale;
			}
		}
	}

	private void AnimateUnload(float startPercent)
	{
		prevAnimTime = UnityEngine.Time.time;
		animPercent = startPercent;
		if (base.isClient && unloadingFXContainer != null)
		{
			unloadingFXContainer.Play();
		}
		InvokeRepeating(UnloadAnimTick, 0f, 0f);
	}

	private void UnloadAnimTick()
	{
		animPercent -= (UnityEngine.Time.time - prevAnimTime) / 40f;
		SetVisualOreLevel(animPercent);
		prevAnimTime = UnityEngine.Time.time;
		if (animPercent <= 0f)
		{
			EndUnloadAnim();
		}
	}

	private void EndUnloadAnim()
	{
		if (base.isClient && unloadingFXContainer != null)
		{
			unloadingFXContainer.Stop();
		}
		CancelInvoke(UnloadAnimTick);
	}

	public float GetOrePercent()
	{
		if (base.isServer)
		{
			return TrainWagonLootData.GetOrePercent(lootTypeIndex, GetStorageContainer());
		}
		return 0f;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseTrain = Facepunch.Pool.Get<BaseTrain>();
		info.msg.baseTrain.lootTypeIndex = lootTypeIndex;
		info.msg.baseTrain.lootPercent = GetOrePercent();
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			foreach (EntityRef<LootContainer> lootContainer2 in lootContainers)
			{
				LootContainer lootContainer = lootContainer2.Get(base.isServer);
				if (lootContainer != null && lootContainer.inventory != null && !lootContainer.inventory.IsLocked())
				{
					lootContainer.DropItems();
				}
			}
		}
		base.DoServerDestroy();
	}

	public bool IsLinedUpToUnload(BoxCollider unloaderBounds)
	{
		BoxCollider[] array = unloadingAreas;
		foreach (BoxCollider boxCollider in array)
		{
			if (unloaderBounds.bounds.Intersects(boxCollider.bounds))
			{
				return true;
			}
		}
		return false;
	}

	public void FillWithLoot(StorageContainer sc)
	{
		sc.inventory.Clear();
		ItemManager.DoRemoves();
		TrainWagonLootData.LootOption lootOption = TrainWagonLootData.instance.GetLootOption(wagonType, out lootTypeIndex);
		int amount = UnityEngine.Random.Range(lootOption.minLootAmount, lootOption.maxLootAmount);
		ItemDefinition itemToCreate = ItemManager.FindItemDefinition(lootOption.lootItem.itemid);
		sc.inventory.AddItem(itemToCreate, amount, 0uL, ItemContainer.LimitStack.All);
		sc.inventory.SetLocked(isLocked: true);
		SetVisualOreLevel(GetOrePercent());
		SendNetworkUpdate();
	}

	public void EmptyOutLoot(StorageContainer sc)
	{
		sc.inventory.Clear();
		ItemManager.DoRemoves();
		SetVisualOreLevel(GetOrePercent());
		SendNetworkUpdate();
	}

	public void BeginUnloadAnimation()
	{
		float orePercent = GetOrePercent();
		AnimateUnload(orePercent);
		ClientRPC(null, "RPC_AnimateUnload", orePercent);
	}

	public void EndEmptyProcess()
	{
		float orePercent = GetOrePercent();
		if (!(orePercent > 0f))
		{
			lootTypeIndex = -1;
			foreach (EntityRef<LootContainer> lootContainer2 in lootContainers)
			{
				LootContainer lootContainer = lootContainer2.Get(base.isServer);
				if (lootContainer != null && lootContainer.inventory != null)
				{
					lootContainer.inventory.SetLocked(isLocked: false);
				}
			}
		}
		SetVisualOreLevel(orePercent);
		ClientRPC(null, "RPC_StopAnimateUnload", orePercent);
		decayingFor = 0f;
	}

	public StorageContainer GetStorageContainer()
	{
		StorageContainer storageContainer = storageInstance.Get(base.isServer);
		if (BaseNetworkableEx.IsValid(storageContainer))
		{
			return storageContainer;
		}
		return null;
	}

	protected override float GetDecayMinutes(bool hasPassengers)
	{
		if ((wagonType == WagonType.Ore || wagonType == WagonType.Fuel) && !hasPassengers && IsEmpty())
		{
			return decayminutesafterunload;
		}
		return base.GetDecayMinutes(hasPassengers);
	}

	protected override bool CanDieFromDecayNow()
	{
		if (IsEmpty())
		{
			return true;
		}
		return base.CanDieFromDecayNow();
	}

	public override bool AdminFixUp(int tier)
	{
		if (!base.AdminFixUp(tier))
		{
			return false;
		}
		StorageContainer storageContainer = GetStorageContainer();
		if (BaseNetworkableEx.IsValid(storageContainer))
		{
			if (tier > 1)
			{
				FillWithLoot(storageContainer);
			}
			else
			{
				EmptyOutLoot(storageContainer);
			}
		}
		return true;
	}

	public float MinDistToUnloadingArea(Vector3 point)
	{
		float num = float.MaxValue;
		point.y = 0f;
		BoxCollider[] array = unloadingAreas;
		foreach (BoxCollider boxCollider in array)
		{
			Vector3 b = boxCollider.transform.position + boxCollider.transform.rotation * boxCollider.center;
			b.y = 0f;
			float num2 = Vector3.Distance(point, b);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_Open(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanBeLooted(player))
		{
			StorageContainer storageContainer = GetStorageContainer();
			if (BaseNetworkableEx.IsValid(storageContainer))
			{
				storageContainer.PlayerOpenLoot(player);
			}
			else
			{
				Debug.LogError(GetType().Name + ": No container component found.");
			}
		}
	}
}
