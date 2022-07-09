#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Rust;
using UnityEngine;

public class ResourceDispenser : EntityComponent<BaseEntity>, IServerComponent
{
	public enum GatherType
	{
		Tree = 0,
		Ore = 1,
		Flesh = 2,
		UNSET = 3,
		LAST = 4
	}

	[Serializable]
	public class GatherPropertyEntry
	{
		public float gatherDamage;

		public float destroyFraction;

		public float conditionLost;
	}

	[Serializable]
	public class GatherProperties
	{
		public GatherPropertyEntry Tree;

		public GatherPropertyEntry Ore;

		public GatherPropertyEntry Flesh;

		public float GetProficiency()
		{
			float num = 0f;
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex(i);
				float num2 = fromIndex.gatherDamage * fromIndex.destroyFraction;
				if (num2 > 0f)
				{
					num += fromIndex.gatherDamage / num2;
				}
			}
			return num;
		}

		public bool Any()
		{
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex(i);
				if (fromIndex.gatherDamage > 0f || fromIndex.conditionLost > 0f)
				{
					return true;
				}
			}
			return false;
		}

		public GatherPropertyEntry GetFromIndex(int index)
		{
			return GetFromIndex((GatherType)index);
		}

		public GatherPropertyEntry GetFromIndex(GatherType index)
		{
			return index switch
			{
				GatherType.Tree => Tree, 
				GatherType.Ore => Ore, 
				GatherType.Flesh => Flesh, 
				_ => null, 
			};
		}
	}

	public GatherType gatherType = GatherType.UNSET;

	public List<ItemAmount> containedItems;

	public float maxDestroyFractionForFinishBonus = 0.2f;

	public List<ItemAmount> finishBonus;

	public float fractionRemaining = 1f;

	private float categoriesRemaining;

	private float startingItemCounts;

	private static Dictionary<GatherType, HashSet<int>> cachedResourceItemTypes;

	public void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		CacheResourceTypeItems();
		UpdateFraction();
		UpdateRemainingCategories();
		CountAllItems();
	}

	private void CacheResourceTypeItems()
	{
		if (cachedResourceItemTypes == null)
		{
			cachedResourceItemTypes = new Dictionary<GatherType, HashSet<int>>();
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(ItemManager.FindItemDefinition("wood").itemid);
			cachedResourceItemTypes.Add(GatherType.Tree, hashSet);
			HashSet<int> hashSet2 = new HashSet<int>();
			hashSet2.Add(ItemManager.FindItemDefinition("stones").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("sulfur.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("metal.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("hq.metal.ore").itemid);
			cachedResourceItemTypes.Add(GatherType.Ore, hashSet2);
		}
	}

	public void DoGather(HitInfo info)
	{
		if (!base.baseEntity.isServer || !info.CanGather || info.DidGather)
		{
			return;
		}
		if (gatherType == GatherType.UNSET)
		{
			Debug.LogWarning("Object :" + base.gameObject.name + ": has unset gathertype!");
			return;
		}
		float num = 0f;
		float num2 = 0f;
		BaseMelee baseMelee = ((info.Weapon == null) ? null : info.Weapon.GetComponent<BaseMelee>());
		if (baseMelee != null)
		{
			GatherPropertyEntry gatherInfoFromIndex = baseMelee.GetGatherInfoFromIndex(gatherType);
			num = gatherInfoFromIndex.gatherDamage * info.gatherScale;
			num2 = gatherInfoFromIndex.destroyFraction;
			if (num == 0f)
			{
				return;
			}
			baseMelee.SendPunch(new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(-0.25f, -0.5f), 0f) * -30f * (gatherInfoFromIndex.conditionLost / 6f), 0.05f);
			baseMelee.LoseCondition(gatherInfoFromIndex.conditionLost);
			if (!BaseNetworkableEx.IsValid(baseMelee) || baseMelee.IsBroken())
			{
				return;
			}
			info.DidGather = true;
		}
		else
		{
			num = info.damageTypes.Total();
			num2 = 0.5f;
		}
		float num3 = fractionRemaining;
		GiveResources(info.Initiator, num, num2, info.Weapon);
		UpdateFraction();
		float num4 = 0f;
		if (fractionRemaining <= 0f)
		{
			num4 = base.baseEntity.MaxHealth();
			if (info.DidGather && num2 < maxDestroyFractionForFinishBonus)
			{
				AssignFinishBonus(info.InitiatorPlayer, 1f - num2);
			}
		}
		else
		{
			num4 = (num3 - fractionRemaining) * base.baseEntity.MaxHealth();
		}
		HitInfo hitInfo = new HitInfo(info.Initiator, base.baseEntity, DamageType.Generic, num4, base.transform.position);
		hitInfo.gatherScale = 0f;
		hitInfo.PointStart = info.PointStart;
		hitInfo.PointEnd = info.PointEnd;
		hitInfo.WeaponPrefab = info.WeaponPrefab;
		hitInfo.Weapon = info.Weapon;
		base.baseEntity.OnAttacked(hitInfo);
	}

	public void AssignFinishBonus(BasePlayer player, float fraction)
	{
		SendMessage("FinishBonusAssigned", SendMessageOptions.DontRequireReceiver);
		if (fraction <= 0f || finishBonus == null)
		{
			return;
		}
		foreach (ItemAmount finishBonu in finishBonus)
		{
			int num = Mathf.CeilToInt((float)(int)finishBonu.amount * Mathf.Clamp01(fraction));
			int num2 = CalculateGatherBonus(player, finishBonu, num);
			Item item = ItemManager.Create(finishBonu.itemDef, num + num2, 0uL);
			if (item != null)
			{
				object obj = Interface.CallHook("OnDispenserBonus", this, player, item);
				if (obj is Item)
				{
					item = (Item)obj;
				}
				player.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested);
			}
		}
	}

	public void OnAttacked(HitInfo info)
	{
		DoGather(info);
	}

	private void GiveResources(BaseEntity entity, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (!BaseNetworkableEx.IsValid(entity) || gatherDamage <= 0f)
		{
			return;
		}
		ItemAmount itemAmount = null;
		int num = containedItems.Count;
		int num2 = UnityEngine.Random.Range(0, containedItems.Count);
		while (num > 0)
		{
			if (num2 >= containedItems.Count)
			{
				num2 = 0;
			}
			if (containedItems[num2].amount > 0f)
			{
				itemAmount = containedItems[num2];
				break;
			}
			num2++;
			num--;
		}
		if (itemAmount == null)
		{
			return;
		}
		GiveResourceFromItem(entity, itemAmount, gatherDamage, destroyFraction, attackWeapon);
		UpdateVars();
		BasePlayer basePlayer = entity.ToPlayer();
		if ((bool)basePlayer)
		{
			Debug.Assert(attackWeapon.GetItem() != null, string.Concat("Attack Weapon ", attackWeapon, " has no Item"));
			Debug.Assert(attackWeapon.ownerItemUID != 0, string.Concat("Attack Weapon ", attackWeapon, " ownerItemUID is 0"));
			Debug.Assert(attackWeapon.GetParentEntity() != null, string.Concat("Attack Weapon ", attackWeapon, " GetParentEntity is null"));
			Debug.Assert(BaseNetworkableEx.IsValid(attackWeapon.GetParentEntity()), string.Concat("Attack Weapon ", attackWeapon, " GetParentEntity is not valid"));
			Debug.Assert(attackWeapon.GetParentEntity().ToPlayer() != null, string.Concat("Attack Weapon ", attackWeapon, " GetParentEntity is not a player"));
			Debug.Assert(!attackWeapon.GetParentEntity().ToPlayer().IsDead(), string.Concat("Attack Weapon ", attackWeapon, " GetParentEntity is not valid"));
			BasePlayer ownerPlayer = attackWeapon.GetOwnerPlayer();
			Debug.Assert(ownerPlayer != null, string.Concat("Attack Weapon ", attackWeapon, " ownerPlayer is null"));
			Debug.Assert(ownerPlayer == basePlayer, string.Concat("Attack Weapon ", attackWeapon, " ownerPlayer is not player"));
			if (ownerPlayer != null)
			{
				Debug.Assert(ownerPlayer.inventory != null, string.Concat("Attack Weapon ", attackWeapon, " ownerPlayer inventory is null"));
				Debug.Assert(ownerPlayer.inventory.FindItemUID(attackWeapon.ownerItemUID) != null, string.Concat("Attack Weapon ", attackWeapon, " FindItemUID is null"));
			}
		}
	}

	public void DestroyFraction(float fraction)
	{
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				containedItem.amount -= fraction / categoriesRemaining;
			}
		}
		UpdateVars();
	}

	private void GiveResourceFromItem(BaseEntity entity, ItemAmount itemAmt, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (itemAmt.amount == 0f)
		{
			return;
		}
		float num = Mathf.Min(gatherDamage, base.baseEntity.Health()) / base.baseEntity.MaxHealth();
		float num2 = itemAmt.startAmount / startingItemCounts;
		float f = Mathf.Clamp(itemAmt.startAmount * num / num2, 0f, itemAmt.amount);
		f = Mathf.Round(f);
		float num3 = f * destroyFraction * 2f;
		if (itemAmt.amount <= f + num3)
		{
			float num4 = (f + num3) / itemAmt.amount;
			f /= num4;
			num3 /= num4;
		}
		itemAmt.amount -= Mathf.Floor(f);
		itemAmt.amount -= Mathf.Floor(num3);
		if (f < 1f)
		{
			f = ((UnityEngine.Random.Range(0f, 1f) <= f) ? 1f : 0f);
			itemAmt.amount = 0f;
		}
		if (itemAmt.amount < 0f)
		{
			itemAmt.amount = 0f;
		}
		if (f >= 1f)
		{
			int num5 = CalculateGatherBonus(entity, itemAmt, f);
			int iAmount = Mathf.FloorToInt(f) + num5;
			Item item = ItemManager.CreateByItemID(itemAmt.itemid, iAmount, 0uL);
			if (Interface.CallHook("OnDispenserGather", this, entity, item) == null && item != null)
			{
				OverrideOwnership(item, attackWeapon);
				entity.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested);
			}
		}
	}

	private int CalculateGatherBonus(BaseEntity entity, ItemAmount item, float amountToGive)
	{
		if (entity == null)
		{
			return 0;
		}
		BasePlayer basePlayer = entity.ToPlayer();
		if (basePlayer == null)
		{
			return 0;
		}
		if (basePlayer.modifiers == null)
		{
			return 0;
		}
		amountToGive = Mathf.FloorToInt(amountToGive);
		float num = 1f;
		Modifier.ModifierType type;
		switch (gatherType)
		{
		case GatherType.Tree:
			type = Modifier.ModifierType.Wood_Yield;
			break;
		case GatherType.Ore:
			type = Modifier.ModifierType.Ore_Yield;
			break;
		default:
			return 0;
		}
		if (!IsProducedItemOfGatherType(item))
		{
			return 0;
		}
		num += basePlayer.modifiers.GetValue(type);
		float variableValue = basePlayer.modifiers.GetVariableValue(type, 0f);
		float num2 = ((num > 1f) ? Mathf.Max(amountToGive * num - amountToGive, 0f) : 0f);
		variableValue += num2;
		int num3 = 0;
		if (variableValue >= 1f)
		{
			num3 = (int)variableValue;
			variableValue -= (float)num3;
		}
		basePlayer.modifiers.SetVariableValue(type, variableValue);
		return num3;
	}

	private bool IsProducedItemOfGatherType(ItemAmount item)
	{
		if (gatherType == GatherType.Tree)
		{
			return cachedResourceItemTypes[GatherType.Tree].Contains(item.itemid);
		}
		if (gatherType == GatherType.Ore)
		{
			return cachedResourceItemTypes[GatherType.Ore].Contains(item.itemid);
		}
		return false;
	}

	public virtual bool OverrideOwnership(Item item, AttackEntity weapon)
	{
		return false;
	}

	private void UpdateVars()
	{
		UpdateFraction();
		UpdateRemainingCategories();
	}

	public void UpdateRemainingCategories()
	{
		int num = 0;
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				num++;
			}
		}
		categoriesRemaining = num;
	}

	public void CountAllItems()
	{
		startingItemCounts = containedItems.Sum((ItemAmount x) => x.startAmount);
	}

	private void UpdateFraction()
	{
		float num = containedItems.Sum((ItemAmount x) => x.startAmount);
		float num2 = containedItems.Sum((ItemAmount x) => x.amount);
		if (num == 0f)
		{
			fractionRemaining = 0f;
		}
		else
		{
			fractionRemaining = num2 / num;
		}
	}
}
