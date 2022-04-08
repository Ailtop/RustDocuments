#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class SprayCan : HeldEntity
{
	private enum SprayFailReason
	{
		None = 0,
		MountedBlocked = 1,
		IOConnection = 2,
		LineOfSight = 3,
		SkinNotOwned = 4,
		InvalidItem = 5
	}

	private struct ContainerSet
	{
		public int ContainerIndex;

		public uint PrefabId;
	}

	private struct ChildPreserveInfo
	{
		public BaseEntity TargetEntity;

		public uint TargetBone;

		public Vector3 LocalPosition;

		public Quaternion LocalRotation;
	}

	public SoundDefinition SpraySound;

	public GameObjectRef SkinSelectPanel;

	public float SprayCooldown = 2f;

	public float ConditionLossPerSpray = 10f;

	public float ConditionLossPerReskin = 10f;

	public GameObjectRef LinePrefab;

	public Color[] SprayColours = new Color[0];

	public float[] SprayWidths = new float[3] { 0.1f, 0.2f, 0.3f };

	public ParticleSystem FreehandWorldSpray;

	public ParticleSystem OneShotWorldSpray;

	public GameObjectRef ReskinEffect;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCan.OnRpcMessage"))
		{
			if (rpc == 151738090 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ChangeItemSkin "));
				}
				using (TimeWarning.New("ChangeItemSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(151738090u, "ChangeItemSkin", this, player))
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
							ChangeItemSkin(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ChangeItemSkin");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void ChangeItemSkin(RPCMessage msg)
	{
		_003C_003Ec__DisplayClass14_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass14_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		if (IsBusy())
		{
			return;
		}
		uint uid = msg.read.UInt32();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		CS_0024_003C_003E8__locals0.targetSkin = msg.read.Int32();
		if (msg.player == null || !msg.player.CanBuild())
		{
			return;
		}
		bool flag = false;
		if (CS_0024_003C_003E8__locals0.targetSkin != 0 && !flag && !msg.player.blueprints.CheckSkinOwnership(CS_0024_003C_003E8__locals0.targetSkin, msg.player.userID))
		{
			CS_0024_003C_003E8__locals0._003CChangeItemSkin_003Eg__SprayFailResponse_007C2(SprayFailReason.SkinNotOwned);
			return;
		}
		BaseEntity baseEntity;
		if (baseNetworkable != null && (object)(baseEntity = baseNetworkable as BaseEntity) != null)
		{
			Vector3 position = baseEntity.WorldSpaceBounds().ClosestPoint(msg.player.eyes.position);
			if (!msg.player.IsVisible(position, 3f))
			{
				CS_0024_003C_003E8__locals0._003CChangeItemSkin_003Eg__SprayFailResponse_007C2(SprayFailReason.LineOfSight);
				return;
			}
			Door door;
			if ((object)(door = baseNetworkable as Door) != null)
			{
				if (!door.GetPlayerLockPermission(msg.player))
				{
					msg.player.ChatMessage("Door must be openable");
					return;
				}
				if (door.IsOpen())
				{
					msg.player.ChatMessage("Door must be closed");
					return;
				}
			}
			ItemDefinition def;
			if (!GetItemDefinitionForEntity(baseEntity, out def))
			{
				CS_0024_003C_003E8__locals0._003CChangeItemSkin_003Eg__SprayFailResponse_007C2(SprayFailReason.InvalidItem);
				return;
			}
			ItemDefinition itemDefinition = null;
			ulong num = ItemDefinition.FindSkin(def.itemid, CS_0024_003C_003E8__locals0.targetSkin);
			ItemSkinDirectory.Skin skin = def.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => x.id == CS_0024_003C_003E8__locals0.targetSkin);
			if (Interface.CallHook("OnEntityReskin", baseEntity, skin, msg.player) != null)
			{
				return;
			}
			ItemSkin itemSkin;
			if (skin.invItem != null && (object)(itemSkin = skin.invItem as ItemSkin) != null)
			{
				if (itemSkin.Redirect != null)
				{
					itemDefinition = itemSkin.Redirect;
				}
				else if (GetItemDefinitionForEntity(baseEntity, out def, false) && def.isRedirectOf != null)
				{
					itemDefinition = def.isRedirectOf;
				}
			}
			else if (def.isRedirectOf != null || (GetItemDefinitionForEntity(baseEntity, out def, false) && def.isRedirectOf != null))
			{
				itemDefinition = def.isRedirectOf;
			}
			if (itemDefinition == null)
			{
				baseEntity.skinID = num;
				baseEntity.SendNetworkUpdate();
				Facepunch.Rust.Analytics.Server.SkinUsed(def.shortname, CS_0024_003C_003E8__locals0.targetSkin);
			}
			else
			{
				SprayFailReason reason;
				if (!CanEntityBeRespawned(baseEntity, out reason))
				{
					CS_0024_003C_003E8__locals0._003CChangeItemSkin_003Eg__SprayFailResponse_007C2(reason);
					return;
				}
				string resourcePath;
				if (!GetEntityPrefabPath(itemDefinition, out resourcePath))
				{
					Debug.LogWarning("Cannot find resource path of redirect entity to spawn! " + itemDefinition.gameObject.name);
					CS_0024_003C_003E8__locals0._003CChangeItemSkin_003Eg__SprayFailResponse_007C2(SprayFailReason.InvalidItem);
					return;
				}
				Vector3 position2 = baseEntity.transform.position;
				Quaternion rotation = baseEntity.transform.rotation;
				BaseEntity entity = baseEntity.GetParentEntity();
				float health = baseEntity.Health();
				EntityRef[] slots = baseEntity.GetSlots();
				BaseCombatEntity baseCombatEntity;
				float lastAttackedTime = (((object)(baseCombatEntity = baseEntity as BaseCombatEntity) != null) ? baseCombatEntity.lastAttackedTime : 0f);
				bool flag2 = baseEntity is Door;
				Dictionary<ContainerSet, List<Item>> dictionary = new Dictionary<ContainerSet, List<Item>>();
				_003CChangeItemSkin_003Eg__SaveEntityStorage_007C14_0(baseEntity, dictionary, 0);
				List<ChildPreserveInfo> obj = Facepunch.Pool.GetList<ChildPreserveInfo>();
				if (flag2)
				{
					foreach (BaseEntity child in baseEntity.children)
					{
						obj.Add(new ChildPreserveInfo
						{
							TargetEntity = child,
							TargetBone = child.parentBone,
							LocalPosition = child.transform.localPosition,
							LocalRotation = child.transform.localRotation
						});
					}
					foreach (ChildPreserveInfo item in obj)
					{
						item.TargetEntity.SetParent(null, true);
					}
				}
				else
				{
					for (int i = 0; i < baseEntity.children.Count; i++)
					{
						_003CChangeItemSkin_003Eg__SaveEntityStorage_007C14_0(baseEntity.children[i], dictionary, -1);
					}
				}
				baseEntity.Kill();
				baseEntity = GameManager.server.CreateEntity(resourcePath, position2, rotation);
				baseEntity.SetParent(entity);
				ItemDefinition def2;
				if (GetItemDefinitionForEntity(baseEntity, out def2, false) && def2.isRedirectOf != null)
				{
					baseEntity.skinID = 0uL;
				}
				else
				{
					baseEntity.skinID = num;
				}
				DecayEntity decayEntity;
				if ((object)(decayEntity = baseEntity as DecayEntity) != null)
				{
					decayEntity.AttachToBuilding(null);
				}
				baseEntity.Spawn();
				BaseCombatEntity baseCombatEntity2;
				if ((object)(baseCombatEntity2 = baseEntity as BaseCombatEntity) != null)
				{
					baseCombatEntity2.SetHealth(health);
					baseCombatEntity2.lastAttackedTime = lastAttackedTime;
				}
				if (dictionary.Count > 0)
				{
					_003CChangeItemSkin_003Eg__RestoreEntityStorage_007C14_1(baseEntity, 0, dictionary);
					if (!flag2)
					{
						for (int j = 0; j < baseEntity.children.Count; j++)
						{
							_003CChangeItemSkin_003Eg__RestoreEntityStorage_007C14_1(baseEntity.children[j], -1, dictionary);
						}
					}
					foreach (KeyValuePair<ContainerSet, List<Item>> item2 in dictionary)
					{
						foreach (Item item3 in item2.Value)
						{
							Debug.Log($"Deleting {item3} as it has no new container");
							item3.Remove();
						}
					}
					Facepunch.Rust.Analytics.Server.SkinUsed(def.shortname, CS_0024_003C_003E8__locals0.targetSkin);
				}
				if (flag2)
				{
					foreach (ChildPreserveInfo item4 in obj)
					{
						item4.TargetEntity.SetParent(baseEntity, item4.TargetBone, true);
						item4.TargetEntity.transform.localPosition = item4.LocalPosition;
						item4.TargetEntity.transform.localRotation = item4.LocalRotation;
						item4.TargetEntity.SendNetworkUpdate();
					}
					baseEntity.SetSlots(slots);
				}
				Facepunch.Pool.FreeList(ref obj);
			}
			Interface.CallHook("OnEntityReskinned", baseEntity, skin, msg.player);
			ClientRPC(null, "Client_ReskinResult", 1, baseEntity.net.ID);
		}
		LoseCondition(ConditionLossPerReskin);
		SetFlag(Flags.Busy, true);
		Invoke(ClearBusy, SprayCooldown);
	}

	private bool GetEntityPrefabPath(ItemDefinition def, out string resourcePath)
	{
		resourcePath = string.Empty;
		ItemModDeployable component;
		if (def.TryGetComponent<ItemModDeployable>(out component))
		{
			resourcePath = component.entityPrefab.resourcePath;
			return true;
		}
		ItemModEntity component2;
		if (def.TryGetComponent<ItemModEntity>(out component2))
		{
			resourcePath = component2.entityPrefab.resourcePath;
			return true;
		}
		ItemModEntityReference component3;
		if (def.TryGetComponent<ItemModEntityReference>(out component3))
		{
			resourcePath = component3.entityPrefab.resourcePath;
			return true;
		}
		return false;
	}

	private void LoseCondition(float amount)
	{
		GetOwnerItem()?.LoseCondition(amount);
	}

	public void ClearBusy()
	{
		SetFlag(Flags.Busy, false);
	}

	private bool CanEntityBeRespawned(BaseEntity targetEntity, out SprayFailReason reason)
	{
		BaseMountable baseMountable;
		if ((object)(baseMountable = targetEntity as BaseMountable) != null && baseMountable.IsMounted())
		{
			reason = SprayFailReason.MountedBlocked;
			return false;
		}
		BaseVehicle baseVehicle;
		if (targetEntity.isServer && (object)(baseVehicle = targetEntity as BaseVehicle) != null && (baseVehicle.HasDriver() || baseVehicle.AnyMounted()))
		{
			reason = SprayFailReason.MountedBlocked;
			return false;
		}
		IOEntity iOEntity;
		if ((object)(iOEntity = targetEntity as IOEntity) != null && (iOEntity.GetConnectedInputCount() != 0 || iOEntity.GetConnectedOutputCount() != 0))
		{
			reason = SprayFailReason.IOConnection;
			return false;
		}
		reason = SprayFailReason.None;
		return true;
	}

	public static bool GetItemDefinitionForEntity(BaseEntity be, out ItemDefinition def, bool useRedirect = true)
	{
		def = null;
		BaseCombatEntity baseCombatEntity;
		if ((object)(baseCombatEntity = be as BaseCombatEntity) != null)
		{
			if (baseCombatEntity.pickup.enabled && baseCombatEntity.pickup.itemTarget != null)
			{
				def = baseCombatEntity.pickup.itemTarget;
			}
			else if (baseCombatEntity.repair.enabled && baseCombatEntity.repair.itemTarget != null)
			{
				def = baseCombatEntity.repair.itemTarget;
			}
		}
		if (useRedirect && def != null && def.isRedirectOf != null)
		{
			def = def.isRedirectOf;
		}
		return def != null;
	}
}
