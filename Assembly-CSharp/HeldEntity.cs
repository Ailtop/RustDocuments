#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class HeldEntity : BaseEntity
{
	[Serializable]
	public class HolsterInfo
	{
		public enum HolsterSlot
		{
			BACK = 0,
			RIGHT_THIGH = 1,
			LEFT_THIGH = 2
		}

		public HolsterSlot slot;

		public bool displayWhenHolstered;

		public string holsterBone = "spine3";

		public Vector3 holsterOffset;

		public Vector3 holsterRotationOffset;
	}

	public static class HeldEntityFlags
	{
		public const Flags Deployed = Flags.Reserved4;

		public const Flags LightsOn = Flags.Reserved5;
	}

	public enum heldEntityVisState
	{
		UNSET = 0,
		Invis = 1,
		Hand = 2,
		Holster = 3,
		GenericVis = 4
	}

	public Animator worldModelAnimator;

	public SoundDefinition thirdPersonDeploySound;

	public SoundDefinition thirdPersonAimSound;

	public SoundDefinition thirdPersonAimEndSound;

	public const Flags Flag_ForceVisible = Flags.Reserved8;

	[Header("Held Entity")]
	public string handBone = "r_prop";

	public AnimatorOverrideController HoldAnimationOverride;

	public bool isBuildingTool;

	[Header("Hostility")]
	public float hostileScore;

	public HolsterInfo holsterInfo;

	[Header("Camera")]
	public BasePlayer.CameraMode HeldCameraMode;

	public Vector3 FirstPersonArmOffset;

	public Vector3 FirstPersonArmRotation;

	[Range(0f, 1f)]
	public float FirstPersonRotationStrength = 1f;

	public bool holsterVisible;

	public bool genericVisible;

	private heldEntityVisState currentVisState;

	private TimeSince lastHeldEvent;

	public ItemId ownerItemUID;

	private Item cachedItem;

	public bool hostile => hostileScore > 0f;

	public virtual Transform MuzzleTransform => null;

	public virtual bool IsUsableByTurret => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HeldEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool LightsOn()
	{
		return HasFlag(Flags.Reserved5);
	}

	public bool IsDeployed()
	{
		return HasFlag(Flags.Reserved4);
	}

	public BasePlayer GetOwnerPlayer()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (!BaseNetworkableEx.IsValid(baseEntity))
		{
			return null;
		}
		BasePlayer basePlayer = baseEntity.ToPlayer();
		if (basePlayer == null)
		{
			return null;
		}
		if (basePlayer.IsDead())
		{
			return null;
		}
		return basePlayer;
	}

	public Connection GetOwnerConnection()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return null;
		}
		if (ownerPlayer.net == null)
		{
			return null;
		}
		return ownerPlayer.net.connection;
	}

	public virtual void SetOwnerPlayer(BasePlayer player)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		Assert.IsTrue(player.isServer, "Player should be serverside!");
		TransformEx.Identity(base.gameObject);
		SetParent(player, handBone);
		SetHeld(bHeld: false);
	}

	public virtual void ClearOwnerPlayer()
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		SetParent(null);
		SetHeld(bHeld: false);
	}

	public virtual void SetVisibleWhileHolstered(bool visible)
	{
		if (holsterInfo.displayWhenHolstered)
		{
			holsterVisible = visible;
			genericVisible = false;
			UpdateHeldItemVisibility();
		}
	}

	public virtual void SetGenericVisible(bool wantsVis)
	{
		genericVisible = wantsVis;
		SetFlag(Flags.Reserved8, wantsVis);
		UpdateHeldItemVisibility();
	}

	public uint GetBone(string bone)
	{
		return StringPool.Get(bone);
	}

	public virtual void SetLightsOn(bool isOn)
	{
		SetFlag(Flags.Reserved5, isOn);
	}

	public void UpdateHeldItemVisibility()
	{
		bool flag = false;
		if (!genericVisible && (bool)GetOwnerPlayer())
		{
			bool flag2 = GetOwnerPlayer().GetHeldEntity() == this;
			flag = ((!ConVar.Server.showHolsteredItems && !flag2) ? UpdateVisiblity_Invis() : (flag2 ? UpdateVisibility_Hand() : ((!holsterVisible) ? UpdateVisiblity_Invis() : UpdateVisiblity_Holster())));
		}
		else if (genericVisible)
		{
			flag = UpdateVisibility_GenericVis();
		}
		else if (!genericVisible)
		{
			flag = UpdateVisiblity_Invis();
		}
		if (flag)
		{
			SendNetworkUpdate();
		}
	}

	public bool UpdateVisibility_Hand()
	{
		if (currentVisState == heldEntityVisState.Hand)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Hand;
		base.limitNetworking = false;
		SetFlag(Flags.Disabled, b: false);
		SetParent(GetOwnerPlayer(), GetBone(handBone));
		return true;
	}

	public bool UpdateVisibility_GenericVis()
	{
		if (currentVisState == heldEntityVisState.GenericVis)
		{
			return false;
		}
		currentVisState = heldEntityVisState.GenericVis;
		base.limitNetworking = false;
		SetFlag(Flags.Disabled, b: false);
		return true;
	}

	public bool UpdateVisiblity_Holster()
	{
		if (currentVisState == heldEntityVisState.Holster)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Holster;
		base.limitNetworking = false;
		SetFlag(Flags.Disabled, b: false);
		SetParent(GetOwnerPlayer(), GetBone(holsterInfo.holsterBone));
		return true;
	}

	public bool UpdateVisiblity_Invis()
	{
		if (currentVisState == heldEntityVisState.Invis)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Invis;
		SetParent(GetOwnerPlayer(), GetBone(handBone));
		base.limitNetworking = true;
		SetFlag(Flags.Disabled, b: true);
		return true;
	}

	public virtual void SetHeld(bool bHeld)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		SetFlag(Flags.Reserved4, bHeld);
		if (!bHeld)
		{
			UpdateVisiblity_Invis();
		}
		base.limitNetworking = !bHeld;
		SetFlag(Flags.Disabled, !bHeld);
		SendNetworkUpdate();
		if (bHeld && (float)lastHeldEvent > 1f && Analytics.Server.Enabled && !GetOwnerPlayer().IsNpc)
		{
			Analytics.Server.HeldItemDeployed(GetItem().info);
			lastHeldEvent = 0f;
		}
		OnHeldChanged();
	}

	public virtual void OnHeldChanged()
	{
	}

	public virtual bool CanBeUsedInWater()
	{
		return false;
	}

	public virtual bool BlocksGestures()
	{
		return false;
	}

	protected Item GetOwnerItem()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null || ownerPlayer.inventory == null)
		{
			return null;
		}
		return ownerPlayer.inventory.FindItemByUID(ownerItemUID);
	}

	public override Item GetItem()
	{
		return GetOwnerItem();
	}

	public ItemDefinition GetOwnerItemDefinition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			Debug.LogWarning("GetOwnerItem - null!", this);
			return null;
		}
		return ownerItem.info;
	}

	public virtual void CollectedForCrafting(Item item, BasePlayer crafter)
	{
	}

	public virtual void ReturnedFromCancelledCraft(Item item, BasePlayer crafter)
	{
	}

	public virtual void ServerCommand(Item item, string command, BasePlayer player)
	{
	}

	public virtual void SetupHeldEntity(Item item)
	{
		ownerItemUID = item.uid;
		InitOwnerPlayer();
	}

	public Item GetCachedItem()
	{
		return cachedItem;
	}

	public void OnItemChanged(Item item)
	{
		cachedItem = item;
	}

	public override void PostServerLoad()
	{
		InitOwnerPlayer();
	}

	private void InitOwnerPlayer()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null)
		{
			SetOwnerPlayer(ownerPlayer);
		}
		else
		{
			ClearOwnerPlayer();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.heldEntity = Facepunch.Pool.Get<ProtoBuf.HeldEntity>();
		info.msg.heldEntity.itemUID = ownerItemUID;
	}

	public void DestroyThis()
	{
		GetOwnerItem()?.Remove();
	}

	protected bool HasItemAmount()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			return ownerItem.amount > 0;
		}
		return false;
	}

	protected bool UseItemAmount(int iAmount)
	{
		if (iAmount <= 0)
		{
			return true;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			DestroyThis();
			return true;
		}
		ownerItem.amount -= iAmount;
		ownerItem.MarkDirty();
		if (ownerItem.amount <= 0)
		{
			DestroyThis();
			return true;
		}
		return false;
	}

	public virtual void ServerUse()
	{
	}

	public virtual void ServerUse(float damageModifier, Transform originOverride = null)
	{
		ServerUse();
	}

	public virtual bool IsInstrument()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.heldEntity != null)
		{
			ownerItemUID = info.msg.heldEntity.itemUID;
		}
	}

	public void SendPunch(Vector3 amount, float duration)
	{
		ClientRPCPlayer(null, GetOwnerPlayer(), "CL_Punch", amount, duration);
	}
}
