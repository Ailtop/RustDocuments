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

	public const float MaxFreeSprayDistanceFromStart = 10f;

	public const float MaxFreeSprayStartingDistance = 3f;

	private SprayCanSpray_Freehand paintingLine;

	public const Flags IsFreeSpraying = Flags.Reserved1;

	public SoundDefinition SpraySound;

	public GameObjectRef SkinSelectPanel;

	public float SprayCooldown = 2f;

	public float ConditionLossPerSpray = 10f;

	public float ConditionLossPerReskin = 10f;

	public GameObjectRef LinePrefab;

	public Color[] SprayColours = new Color[0];

	public float[] SprayWidths = new float[3] { 0.1f, 0.2f, 0.3f };

	public ParticleSystem worldSpaceSprayFx;

	public GameObjectRef ReskinEffect;

	public ItemDefinition SprayDecalItem;

	public GameObjectRef SprayDecalEntityRef;

	public SteamInventoryItem FreeSprayUnlockItem;

	public ParticleSystem.MinMaxGradient DecalSprayGradient;

	public SoundDefinition SprayLoopDef;

	public const string ENEMY_BASE_STAT = "sprayed_enemy_base";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCan.OnRpcMessage"))
		{
			if (rpc == 3490735573u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - BeginFreehandSpray "));
				}
				using (TimeWarning.New("BeginFreehandSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3490735573u, "BeginFreehandSpray", this, player))
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
							BeginFreehandSpray(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BeginFreehandSpray");
					}
				}
				return true;
			}
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
							RPCMessage msg3 = rPCMessage;
							ChangeItemSkin(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ChangeItemSkin");
					}
				}
				return true;
			}
			if (rpc == 396000799 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - CreateSpray "));
				}
				using (TimeWarning.New("CreateSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(396000799u, "CreateSpray", this, player))
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
							RPCMessage msg4 = rPCMessage;
							CreateSpray(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in CreateSpray");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void BeginFreehandSpray(RPCMessage msg)
	{
		if (!IsBusy() && CanSprayFreehand(msg.player))
		{
			Vector3 vector = msg.read.Vector3();
			Vector3 atNormal = msg.read.Vector3();
			int num = msg.read.Int32();
			int num2 = msg.read.Int32();
			if (num >= 0 && num < SprayColours.Length && num2 >= 0 && num2 < SprayWidths.Length && !(Vector3.Distance(vector, GetOwnerPlayer().transform.position) > 3f))
			{
				SprayCanSpray_Freehand sprayCanSpray_Freehand = GameManager.server.CreateEntity(LinePrefab.resourcePath, vector, Quaternion.identity) as SprayCanSpray_Freehand;
				sprayCanSpray_Freehand.AddInitialPoint(atNormal);
				sprayCanSpray_Freehand.SetColour(SprayColours[num]);
				sprayCanSpray_Freehand.SetWidth(SprayWidths[num2]);
				sprayCanSpray_Freehand.EnableChanges(msg.player);
				sprayCanSpray_Freehand.Spawn();
				paintingLine = sprayCanSpray_Freehand;
				ClientRPC(null, "Client_ChangeSprayColour", num);
				SetFlag(Flags.Busy, b: true);
				SetFlag(Flags.Reserved1, b: true);
				CheckAchievementPosition(vector);
			}
		}
	}

	public void ClearPaintingLine(bool allowNewSprayImmediately)
	{
		paintingLine = null;
		LoseCondition(ConditionLossPerSpray);
		if (allowNewSprayImmediately)
		{
			ClearBusy();
		}
		else
		{
			Invoke(ClearBusy, 0.1f);
		}
	}

	public bool CanSprayFreehand(BasePlayer player)
	{
		if (FreeSprayUnlockItem != null)
		{
			if (!player.blueprints.steamInventory.HasItem(FreeSprayUnlockItem.id))
			{
				return FreeSprayUnlockItem.HasUnlocked(player.userID);
			}
			return true;
		}
		return false;
	}

	private bool IsSprayBlockedByTrigger(Vector3 pos)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return true;
		}
		TriggerNoSpray triggerNoSpray = ownerPlayer.FindTrigger<TriggerNoSpray>();
		if (triggerNoSpray == null)
		{
			return false;
		}
		return !triggerNoSpray.IsPositionValid(pos);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void ChangeItemSkin(RPCMessage msg)
	{
		if (IsBusy())
		{
			return;
		}
		uint uid = msg.read.UInt32();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		int targetSkin = msg.read.Int32();
		if (msg.player == null || !msg.player.CanBuild())
		{
			return;
		}
		bool flag = false;
		if (targetSkin != 0 && !flag && !msg.player.blueprints.CheckSkinOwnership(targetSkin, msg.player.userID))
		{
			SprayFailResponse(SprayFailReason.SkinNotOwned);
			return;
		}
		if (baseNetworkable != null && baseNetworkable is BaseEntity baseEntity2)
		{
			Vector3 position = baseEntity2.WorldSpaceBounds().ClosestPoint(msg.player.eyes.position);
			if (!msg.player.IsVisible(position, 3f))
			{
				SprayFailResponse(SprayFailReason.LineOfSight);
				return;
			}
			if (baseNetworkable is Door door)
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
			if (!GetItemDefinitionForEntity(baseEntity2, out var def))
			{
				SprayFailResponse(SprayFailReason.InvalidItem);
				return;
			}
			ItemDefinition itemDefinition = null;
			ulong num = ItemDefinition.FindSkin(def.itemid, targetSkin);
			ItemSkinDirectory.Skin skin = def.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => x.id == targetSkin);
			if (Interface.CallHook("OnEntityReskin", baseEntity2, skin, msg.player) != null)
			{
				return;
			}
			if (skin.invItem != null && skin.invItem is ItemSkin itemSkin)
			{
				if (itemSkin.Redirect != null)
				{
					itemDefinition = itemSkin.Redirect;
				}
				else if (GetItemDefinitionForEntity(baseEntity2, out def, useRedirect: false) && def.isRedirectOf != null)
				{
					itemDefinition = def.isRedirectOf;
				}
			}
			else if (def.isRedirectOf != null || (GetItemDefinitionForEntity(baseEntity2, out def, useRedirect: false) && def.isRedirectOf != null))
			{
				itemDefinition = def.isRedirectOf;
			}
			if (itemDefinition == null)
			{
				baseEntity2.skinID = num;
				baseEntity2.SendNetworkUpdate();
				Facepunch.Rust.Analytics.Server.SkinUsed(def.shortname, targetSkin);
			}
			else
			{
				if (!CanEntityBeRespawned(baseEntity2, out var reason2))
				{
					SprayFailResponse(reason2);
					return;
				}
				if (!GetEntityPrefabPath(itemDefinition, out var resourcePath))
				{
					Debug.LogWarning("Cannot find resource path of redirect entity to spawn! " + itemDefinition.gameObject.name);
					SprayFailResponse(SprayFailReason.InvalidItem);
					return;
				}
				Vector3 position2 = baseEntity2.transform.position;
				Quaternion rotation = baseEntity2.transform.rotation;
				BaseEntity entity = baseEntity2.GetParentEntity();
				float health = baseEntity2.Health();
				EntityRef[] slots = baseEntity2.GetSlots();
				float lastAttackedTime = ((baseEntity2 is BaseCombatEntity baseCombatEntity) ? baseCombatEntity.lastAttackedTime : 0f);
				bool flag2 = baseEntity2 is Door;
				Dictionary<ContainerSet, List<Item>> dictionary2 = new Dictionary<ContainerSet, List<Item>>();
				SaveEntityStorage(baseEntity2, dictionary2, 0);
				List<ChildPreserveInfo> obj = Facepunch.Pool.GetList<ChildPreserveInfo>();
				if (flag2)
				{
					foreach (BaseEntity child in baseEntity2.children)
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
						item.TargetEntity.SetParent(null, worldPositionStays: true);
					}
				}
				else
				{
					for (int i = 0; i < baseEntity2.children.Count; i++)
					{
						SaveEntityStorage(baseEntity2.children[i], dictionary2, -1);
					}
				}
				baseEntity2.Kill();
				baseEntity2 = GameManager.server.CreateEntity(resourcePath, position2, rotation);
				baseEntity2.SetParent(entity);
				if (GetItemDefinitionForEntity(baseEntity2, out var def2, useRedirect: false) && def2.isRedirectOf != null)
				{
					baseEntity2.skinID = 0uL;
				}
				else
				{
					baseEntity2.skinID = num;
				}
				if (baseEntity2 is DecayEntity decayEntity)
				{
					decayEntity.AttachToBuilding(null);
				}
				baseEntity2.Spawn();
				if (baseEntity2 is BaseCombatEntity baseCombatEntity2)
				{
					baseCombatEntity2.SetHealth(health);
					baseCombatEntity2.lastAttackedTime = lastAttackedTime;
				}
				if (dictionary2.Count > 0)
				{
					RestoreEntityStorage(baseEntity2, 0, dictionary2);
					if (!flag2)
					{
						for (int j = 0; j < baseEntity2.children.Count; j++)
						{
							RestoreEntityStorage(baseEntity2.children[j], -1, dictionary2);
						}
					}
					foreach (KeyValuePair<ContainerSet, List<Item>> item2 in dictionary2)
					{
						foreach (Item item3 in item2.Value)
						{
							Debug.Log($"Deleting {item3} as it has no new container");
							item3.Remove();
						}
					}
					Facepunch.Rust.Analytics.Server.SkinUsed(def.shortname, targetSkin);
				}
				if (flag2)
				{
					foreach (ChildPreserveInfo item4 in obj)
					{
						item4.TargetEntity.SetParent(baseEntity2, item4.TargetBone, worldPositionStays: true);
						item4.TargetEntity.transform.localPosition = item4.LocalPosition;
						item4.TargetEntity.transform.localRotation = item4.LocalRotation;
						item4.TargetEntity.SendNetworkUpdate();
					}
					baseEntity2.SetSlots(slots);
				}
				Facepunch.Pool.FreeList(ref obj);
			}
			Interface.CallHook("OnEntityReskinned", baseEntity2, skin, msg.player);
			ClientRPC(null, "Client_ReskinResult", 1, baseEntity2.net.ID);
		}
		LoseCondition(ConditionLossPerReskin);
		ClientRPC(null, "Client_ChangeSprayColour", -1);
		SetFlag(Flags.Busy, b: true);
		Invoke(ClearBusy, SprayCooldown);
		static void RestoreEntityStorage(BaseEntity baseEntity, int index, Dictionary<ContainerSet, List<Item>> copy)
		{
			if (baseEntity is IItemContainerEntity itemContainerEntity)
			{
				ContainerSet containerSet = default(ContainerSet);
				containerSet.ContainerIndex = index;
				containerSet.PrefabId = ((index != 0) ? baseEntity.prefabID : 0u);
				ContainerSet key = containerSet;
				if (copy.ContainsKey(key))
				{
					foreach (Item item5 in copy[key])
					{
						item5.MoveToContainer(itemContainerEntity.inventory);
					}
					copy.Remove(key);
				}
			}
		}
		static void SaveEntityStorage(BaseEntity baseEntity, Dictionary<ContainerSet, List<Item>> dictionary, int index)
		{
			if (baseEntity is IItemContainerEntity itemContainerEntity2)
			{
				ContainerSet containerSet2 = default(ContainerSet);
				containerSet2.ContainerIndex = index;
				containerSet2.PrefabId = ((index != 0) ? baseEntity.prefabID : 0u);
				ContainerSet key2 = containerSet2;
				if (!dictionary.ContainsKey(key2))
				{
					dictionary.Add(key2, new List<Item>());
					foreach (Item item6 in itemContainerEntity2.inventory.itemList)
					{
						dictionary[key2].Add(item6);
					}
					{
						foreach (Item item7 in dictionary[key2])
						{
							item7.RemoveFromContainer();
						}
						return;
					}
				}
				Debug.Log("Multiple containers with the same prefab id being added during vehicle reskin");
			}
		}
		void SprayFailResponse(SprayFailReason reason)
		{
			ClientRPC(null, "Client_ReskinResult", 0, (int)reason);
		}
	}

	private bool GetEntityPrefabPath(ItemDefinition def, out string resourcePath)
	{
		resourcePath = string.Empty;
		if (def.TryGetComponent<ItemModDeployable>(out var component))
		{
			resourcePath = component.entityPrefab.resourcePath;
			return true;
		}
		if (def.TryGetComponent<ItemModEntity>(out var component2))
		{
			resourcePath = component2.entityPrefab.resourcePath;
			return true;
		}
		if (def.TryGetComponent<ItemModEntityReference>(out var component3))
		{
			resourcePath = component3.entityPrefab.resourcePath;
			return true;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void CreateSpray(RPCMessage msg)
	{
		if (IsBusy())
		{
			return;
		}
		ClientRPC(null, "Client_ChangeSprayColour", -1);
		SetFlag(Flags.Busy, b: true);
		Invoke(ClearBusy, SprayCooldown);
		Vector3 vector = msg.read.Vector3();
		Vector3 vector2 = msg.read.Vector3();
		Vector3 point = msg.read.Vector3();
		int num = msg.read.Int32();
		if (!(Vector3.Distance(vector, base.transform.position) > 4.5f))
		{
			Quaternion quaternion = Quaternion.LookRotation((new Plane(vector2, vector).ClosestPointOnPlane(point) - vector).normalized, vector2);
			quaternion *= Quaternion.Euler(0f, 0f, 90f);
			bool flag = false;
			if (num != 0 && !flag && !msg.player.blueprints.CheckSkinOwnership(num, msg.player.userID))
			{
				Debug.Log($"SprayCan.ChangeItemSkin player does not have item :{num}:");
			}
			else if (Interface.CallHook("OnSprayCreate", this, vector, quaternion) == null)
			{
				ulong num2 = ItemDefinition.FindSkin(SprayDecalItem.itemid, num);
				BaseEntity baseEntity = GameManager.server.CreateEntity(SprayDecalEntityRef.resourcePath, vector, quaternion);
				baseEntity.skinID = num2;
				baseEntity.OnDeployed(null, GetOwnerPlayer(), GetItem());
				baseEntity.Spawn();
				CheckAchievementPosition(vector);
				LoseCondition(ConditionLossPerSpray);
			}
		}
	}

	private void CheckAchievementPosition(Vector3 pos)
	{
	}

	private void LoseCondition(float amount)
	{
		GetOwnerItem()?.LoseCondition(amount);
	}

	public void ClearBusy()
	{
		SetFlag(Flags.Busy, b: false);
		SetFlag(Flags.Reserved1, b: false);
	}

	public override void OnHeldChanged()
	{
		if (IsDisabled())
		{
			ClearBusy();
			if (paintingLine != null)
			{
				paintingLine.Kill();
			}
			paintingLine = null;
		}
	}

	private bool CanEntityBeRespawned(BaseEntity targetEntity, out SprayFailReason reason)
	{
		if (targetEntity is BaseMountable baseMountable && baseMountable.IsMounted())
		{
			reason = SprayFailReason.MountedBlocked;
			return false;
		}
		if (targetEntity.isServer && targetEntity is BaseVehicle baseVehicle && (baseVehicle.HasDriver() || baseVehicle.AnyMounted()))
		{
			reason = SprayFailReason.MountedBlocked;
			return false;
		}
		if (targetEntity is IOEntity iOEntity && (iOEntity.GetConnectedInputCount() != 0 || iOEntity.GetConnectedOutputCount() != 0))
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
		if (be is BaseCombatEntity baseCombatEntity)
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
