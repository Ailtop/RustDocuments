#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Planner : HeldEntity
{
	public BaseEntity[] buildableList;

	public bool isTypeDeployable => GetModDeployable() != null;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Planner.OnRpcMessage"))
		{
			if (rpc == 1872774636 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DoPlace ");
				}
				using (TimeWarning.New("DoPlace"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1872774636u, "DoPlace", this, player))
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
							DoPlace(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoPlace");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public ItemModDeployable GetModDeployable()
	{
		ItemDefinition ownerItemDefinition = GetOwnerItemDefinition();
		if (ownerItemDefinition == null)
		{
			return null;
		}
		return ownerItemDefinition.GetComponent<ItemModDeployable>();
	}

	public Deployable GetDeployable()
	{
		ItemModDeployable modDeployable = GetModDeployable();
		if (modDeployable == null)
		{
			return null;
		}
		return modDeployable.GetDeployable(this);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void DoPlace(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		using CreateBuilding msg2 = CreateBuilding.Deserialize(msg.read);
		DoBuild(msg2);
	}

	public Socket_Base FindSocket(string name, uint prefabIDToFind)
	{
		return PrefabAttribute.server.FindAll<Socket_Base>(prefabIDToFind).FirstOrDefault((Socket_Base s) => s.socketName == name);
	}

	public void DoBuild(CreateBuilding msg)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return;
		}
		if (ConVar.AntiHack.objectplacement && ownerPlayer.TriggeredAntiHack())
		{
			ownerPlayer.ChatMessage("AntiHack!");
			return;
		}
		Construction construction = PrefabAttribute.server.Find<Construction>(msg.blockID);
		if (construction == null)
		{
			ownerPlayer.ChatMessage("Couldn't find Construction " + msg.blockID);
			return;
		}
		if (!CanAffordToPlace(construction))
		{
			ownerPlayer.ChatMessage("Can't afford to place!");
			return;
		}
		if (!ownerPlayer.CanBuild() && !construction.canBypassBuildingPermission)
		{
			ownerPlayer.ChatMessage("Building is blocked!");
			return;
		}
		Deployable deployable = GetDeployable();
		if (construction.deployable != deployable)
		{
			ownerPlayer.ChatMessage("Deployable mismatch!");
			AntiHack.NoteAdminHack(ownerPlayer);
			return;
		}
		if (ConVar.Server.max_sleeping_bags > 0)
		{
			SleepingBag.CanBuildResult? canBuildResult = SleepingBag.CanBuildBed(ownerPlayer, construction);
			if (canBuildResult.HasValue)
			{
				if (canBuildResult.Value.Phrase != null)
				{
					ownerPlayer.ShowToast((!canBuildResult.Value.Result) ? GameTip.Styles.Red_Normal : GameTip.Styles.Blue_Long, canBuildResult.Value.Phrase, canBuildResult.Value.Arguments);
				}
				if (!canBuildResult.Value.Result)
				{
					return;
				}
			}
		}
		Construction.Target target = default(Construction.Target);
		if (msg.entity.IsValid)
		{
			target.entity = BaseNetworkable.serverEntities.Find(msg.entity) as BaseEntity;
			if (target.entity == null)
			{
				NetworkableId entity = msg.entity;
				ownerPlayer.ChatMessage("Couldn't find entity " + entity.ToString());
				return;
			}
			msg.ray = new Ray(target.entity.transform.TransformPoint(msg.ray.origin), target.entity.transform.TransformDirection(msg.ray.direction));
			msg.position = target.entity.transform.TransformPoint(msg.position);
			msg.normal = target.entity.transform.TransformDirection(msg.normal);
			msg.rotation = target.entity.transform.rotation * msg.rotation;
			if (msg.socket != 0)
			{
				string text = StringPool.Get(msg.socket);
				if (text != "")
				{
					target.socket = FindSocket(text, target.entity.prefabID);
				}
				if (target.socket == null)
				{
					ownerPlayer.ChatMessage("Couldn't find socket " + msg.socket);
					return;
				}
			}
			else if (target.entity is Door)
			{
				ownerPlayer.ChatMessage("Can't deploy on door");
				return;
			}
		}
		target.ray = msg.ray;
		target.onTerrain = msg.onterrain;
		target.position = msg.position;
		target.normal = msg.normal;
		target.rotation = msg.rotation;
		target.player = ownerPlayer;
		target.valid = true;
		if (Interface.CallHook("CanBuild", this, construction, target) != null)
		{
			return;
		}
		if (ShouldParent(target.entity, deployable))
		{
			Vector3 position = ((target.socket != null) ? target.GetWorldPosition() : target.position);
			float num = target.entity.Distance(position);
			if (num > 1f)
			{
				ownerPlayer.ChatMessage("Parent too far away: " + num);
				return;
			}
		}
		DoBuild(target, construction);
	}

	public void DoBuild(Construction.Target target, Construction component)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer || RayEx.IsNaNOrInfinity(target.ray) || target.position.IsNaNOrInfinity() || target.normal.IsNaNOrInfinity())
		{
			return;
		}
		if (target.socket != null)
		{
			if (!target.socket.female)
			{
				ownerPlayer.ChatMessage("Target socket is not female. (" + target.socket.socketName + ")");
				return;
			}
			if (target.entity != null && target.entity.IsOccupied(target.socket))
			{
				ownerPlayer.ChatMessage("Target socket is occupied. (" + target.socket.socketName + ")");
				return;
			}
			if (target.onTerrain)
			{
				ownerPlayer.ChatMessage("Target on terrain is not allowed when attaching to socket. (" + target.socket.socketName + ")");
				return;
			}
		}
		Vector3 vector = ((target.entity != null && target.socket != null) ? target.GetWorldPosition() : target.position);
		if (AntiHack.TestIsBuildingInsideSomething(target, vector))
		{
			ownerPlayer.ChatMessage("Can't deploy inside objects");
			return;
		}
		if (ConVar.AntiHack.eye_protection >= 2)
		{
			Vector3 center = ownerPlayer.eyes.center;
			Vector3 position = ownerPlayer.eyes.position;
			Vector3 origin = target.ray.origin;
			Vector3 p = vector;
			int num = 2097152;
			int num2 = 2162688;
			if (ConVar.AntiHack.build_terraincheck)
			{
				num2 |= 0x800000;
			}
			if (ConVar.AntiHack.build_vehiclecheck)
			{
				num2 |= 0x8000000;
			}
			float num3 = ConVar.AntiHack.build_losradius;
			float padding = ConVar.AntiHack.build_losradius + 0.01f;
			int layerMask = num2;
			if (target.socket != null)
			{
				num3 = 0f;
				padding = 0.5f;
				layerMask = num;
			}
			if (component.isSleepingBag)
			{
				num3 = ConVar.AntiHack.build_losradius_sleepingbag;
				padding = ConVar.AntiHack.build_losradius_sleepingbag + 0.01f;
				layerMask = num2;
			}
			if (num3 > 0f)
			{
				p += target.normal.normalized * num3;
			}
			if (target.entity != null)
			{
				DeployShell deployShell = PrefabAttribute.server.Find<DeployShell>(target.entity.prefabID);
				if (deployShell != null)
				{
					p += target.normal.normalized * deployShell.LineOfSightPadding();
				}
			}
			if (!GamePhysics.LineOfSightRadius(center, position, layerMask, num3) || !GamePhysics.LineOfSightRadius(position, origin, layerMask, num3) || !GamePhysics.LineOfSightRadius(origin, p, layerMask, num3, 0f, padding))
			{
				ownerPlayer.ChatMessage("Line of sight blocked.");
				return;
			}
		}
		Construction.lastPlacementError = "No Error";
		GameObject gameObject = DoPlacement(target, component);
		if (gameObject == null)
		{
			ownerPlayer.ChatMessage("Can't place: " + Construction.lastPlacementError);
		}
		if (!(gameObject != null))
		{
			return;
		}
		Interface.CallHook("OnEntityBuilt", this, gameObject);
		Deployable deployable = GetDeployable();
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(gameObject);
		if (baseEntity != null && deployable != null)
		{
			if (ShouldParent(target.entity, deployable))
			{
				baseEntity.SetParent(target.entity, worldPositionStays: true);
			}
			if (deployable.wantsInstanceData && GetOwnerItem().instanceData != null)
			{
				(baseEntity as IInstanceDataReceiver).ReceiveInstanceData(GetOwnerItem().instanceData);
			}
			if (deployable.copyInventoryFromItem)
			{
				StorageContainer component2 = baseEntity.GetComponent<StorageContainer>();
				if ((bool)component2)
				{
					component2.ReceiveInventoryFromItem(GetOwnerItem());
				}
			}
			ItemModDeployable modDeployable = GetModDeployable();
			if (modDeployable != null)
			{
				modDeployable.OnDeployed(baseEntity, ownerPlayer);
			}
			baseEntity.OnDeployed(baseEntity.GetParentEntity(), ownerPlayer, GetOwnerItem());
			if (deployable.placeEffect.isValid)
			{
				if ((bool)target.entity && target.socket != null)
				{
					Effect.server.Run(deployable.placeEffect.resourcePath, target.entity.transform.TransformPoint(target.socket.worldPosition), target.entity.transform.up);
				}
				else
				{
					Effect.server.Run(deployable.placeEffect.resourcePath, target.position, target.normal);
				}
			}
		}
		if (baseEntity != null)
		{
			Facepunch.Rust.Analytics.Azure.OnEntityBuilt(baseEntity, ownerPlayer);
		}
		PayForPlacement(ownerPlayer, component);
	}

	public GameObject DoPlacement(Construction.Target placement, Construction component)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return null;
		}
		BaseEntity baseEntity = component.CreateConstruction(placement, bNeedsValidPlacement: true);
		if (!baseEntity)
		{
			return null;
		}
		float num = 1f;
		float num2 = 0f;
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			baseEntity.skinID = ownerItem.skin;
			if (ownerItem.hasCondition)
			{
				num = ownerItem.conditionNormalized;
			}
		}
		PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
		BuildingBlock buildingBlock = baseEntity as BuildingBlock;
		if ((bool)buildingBlock)
		{
			buildingBlock.blockDefinition = PrefabAttribute.server.Find<Construction>(buildingBlock.prefabID);
			if (!buildingBlock.blockDefinition)
			{
				Debug.LogError("Placing a building block that has no block definition!");
				return null;
			}
			buildingBlock.SetGrade(buildingBlock.blockDefinition.defaultGrade.gradeBase.type);
			num2 = buildingBlock.currentGrade.maxHealth;
		}
		BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
		if ((bool)baseCombatEntity)
		{
			num2 = ((buildingBlock != null) ? buildingBlock.currentGrade.maxHealth : baseCombatEntity.startHealth);
			baseCombatEntity.ResetLifeStateOnSpawn = false;
			baseCombatEntity.InitializeHealth(num2 * num, num2);
		}
		if (Interface.CallHook("OnConstructionPlace", baseEntity, component, placement, ownerPlayer) != null)
		{
			if (BaseNetworkableEx.IsValid(baseEntity))
			{
				baseEntity.KillMessage();
			}
			else
			{
				GameManager.Destroy(baseEntity);
			}
			return null;
		}
		baseEntity.gameObject.SendMessage("SetDeployedBy", ownerPlayer, SendMessageOptions.DontRequireReceiver);
		baseEntity.OwnerID = ownerPlayer.userID;
		baseEntity.Spawn();
		if ((bool)buildingBlock)
		{
			Effect.server.Run("assets/bundled/prefabs/fx/build/frame_place.prefab", baseEntity, 0u, Vector3.zero, Vector3.zero);
		}
		StabilityEntity stabilityEntity = baseEntity as StabilityEntity;
		if ((bool)stabilityEntity)
		{
			stabilityEntity.UpdateSurroundingEntities();
		}
		return baseEntity.gameObject;
	}

	public void PayForPlacement(BasePlayer player, Construction component)
	{
		if (Interface.CallHook("OnPayForPlacement", player, this, component) != null)
		{
			return;
		}
		if (isTypeDeployable)
		{
			GetItem().UseItem();
			return;
		}
		List<Item> list = new List<Item>();
		foreach (ItemAmount item in component.defaultGrade.CostToBuild())
		{
			player.inventory.Take(list, item.itemDef.itemid, (int)item.amount);
			player.Command("note.inv", item.itemDef.itemid, item.amount * -1f);
		}
		foreach (Item item2 in list)
		{
			item2.Remove();
		}
	}

	public bool CanAffordToPlace(Construction component)
	{
		if (isTypeDeployable)
		{
			return true;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return false;
		}
		object obj = Interface.CallHook("CanAffordToPlace", ownerPlayer, this, component);
		if (obj is bool)
		{
			return (bool)obj;
		}
		foreach (ItemAmount item in component.defaultGrade.CostToBuild())
		{
			if ((float)ownerPlayer.inventory.GetAmount(item.itemDef.itemid) < item.amount)
			{
				return false;
			}
		}
		return true;
	}

	private bool ShouldParent(BaseEntity targetEntity, Deployable deployable)
	{
		if (targetEntity != null && targetEntity.SupportsChildDeployables() && (targetEntity.ForceDeployableSetParent() || (deployable != null && deployable.setSocketParent)))
		{
			return true;
		}
		return false;
	}
}
