#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingBlock : StabilityEntity
{
	public static class BlockFlags
	{
		public const Flags CanRotate = Flags.Reserved1;

		public const Flags CanDemolish = Flags.Reserved2;
	}

	public class UpdateSkinWorkQueue : ObjectWorkQueue<BuildingBlock>
	{
		protected override void RunJob(BuildingBlock entity)
		{
			if (ShouldAdd(entity))
			{
				entity.UpdateSkin(force: true);
			}
		}

		protected override bool ShouldAdd(BuildingBlock entity)
		{
			return BaseNetworkableEx.IsValid(entity);
		}
	}

	private bool forceSkinRefresh;

	private ulong lastSkinID;

	public int modelState;

	public int lastModelState;

	private uint lastCustomColour;

	public uint playerCustomColourToApply;

	public BuildingGrade.Enum grade;

	public BuildingGrade.Enum lastGrade = BuildingGrade.Enum.None;

	public ConstructionSkin currentSkin;

	private DeferredAction skinChange;

	private MeshRenderer placeholderRenderer;

	private MeshCollider placeholderCollider;

	public static UpdateSkinWorkQueue updateSkinQueueServer = new UpdateSkinWorkQueue();

	public bool CullBushes;

	public bool CheckForPipesOnModelChange;

	[NonSerialized]
	public Construction blockDefinition;

	private static Vector3[] outsideLookupOffsets = new Vector3[5]
	{
		new Vector3(0f, 1f, 0f).normalized,
		new Vector3(1f, 1f, 0f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3(0f, 1f, 1f).normalized,
		new Vector3(0f, 1f, -1f).normalized
	};

	public uint customColour { get; private set; }

	public ConstructionGrade currentGrade => blockDefinition.GetGrade(grade, skinID);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BuildingBlock.OnRpcMessage"))
		{
			if (rpc == 2858062413u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoDemolish "));
				}
				using (TimeWarning.New("DoDemolish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2858062413u, "DoDemolish", this, player, 3f))
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
							DoDemolish(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoDemolish");
					}
				}
				return true;
			}
			if (rpc == 216608990 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoImmediateDemolish "));
				}
				using (TimeWarning.New("DoImmediateDemolish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(216608990u, "DoImmediateDemolish", this, player, 3f))
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
							DoImmediateDemolish(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in DoImmediateDemolish");
					}
				}
				return true;
			}
			if (rpc == 1956645865 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoRotation "));
				}
				using (TimeWarning.New("DoRotation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1956645865u, "DoRotation", this, player, 3f))
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
							DoRotation(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in DoRotation");
					}
				}
				return true;
			}
			if (rpc == 3746288057u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoUpgradeToGrade "));
				}
				using (TimeWarning.New("DoUpgradeToGrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3746288057u, "DoUpgradeToGrade", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							DoUpgradeToGrade(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in DoUpgradeToGrade");
					}
				}
				return true;
			}
			if (rpc == 4081052216u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoUpgradeToGrade_Delayed "));
				}
				using (TimeWarning.New("DoUpgradeToGrade_Delayed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4081052216u, "DoUpgradeToGrade_Delayed", this, player, 3f))
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
							RPCMessage msg6 = rPCMessage;
							DoUpgradeToGrade_Delayed(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in DoUpgradeToGrade_Delayed");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool CanDemolish(BasePlayer player)
	{
		object obj = Interface.CallHook("CanDemolish", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsDemolishable())
		{
			return HasDemolishPrivilege(player);
		}
		return false;
	}

	public bool IsDemolishable()
	{
		if (!ConVar.Server.pve && !HasFlag(Flags.Reserved2))
		{
			return false;
		}
		return true;
	}

	public bool HasDemolishPrivilege(BasePlayer player)
	{
		return player.IsBuildingAuthed(base.transform.position, base.transform.rotation, bounds);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoDemolish(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanDemolish(msg.player) && Interface.CallHook("OnStructureDemolish", this, msg.player, false) == null)
		{
			Facepunch.Rust.Analytics.Azure.OnBuildingBlockDemolished(msg.player, this);
			Kill(DestroyMode.Gib);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoImmediateDemolish(RPCMessage msg)
	{
		if (msg.player.CanInteract() && msg.player.IsAdmin && Interface.CallHook("OnStructureDemolish", this, msg.player, true) == null)
		{
			Facepunch.Rust.Analytics.Azure.OnBuildingBlockDemolished(msg.player, this);
			Kill(DestroyMode.Gib);
		}
	}

	public void StopBeingDemolishable()
	{
		SetFlag(Flags.Reserved2, b: false);
		SendNetworkUpdate();
	}

	public void StartBeingDemolishable()
	{
		SetFlag(Flags.Reserved2, b: true);
		Invoke(StopBeingDemolishable, 600f);
	}

	public void SetConditionalModel(int state)
	{
		modelState = state;
	}

	public bool GetConditionalModel(int index)
	{
		return (modelState & (1 << index)) != 0;
	}

	public bool CanChangeToGrade(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		object obj = Interface.CallHook("CanChangeGrade", player, this, iGrade, iSkin);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasUpgradePrivilege(iGrade, iSkin, player))
		{
			return !IsUpgradeBlocked();
		}
		return false;
	}

	public bool HasUpgradePrivilege(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		if (iGrade < grade)
		{
			return false;
		}
		if (iGrade == grade && iSkin == skinID)
		{
			return false;
		}
		if (iGrade <= BuildingGrade.Enum.None)
		{
			return false;
		}
		if (iGrade >= BuildingGrade.Enum.Count)
		{
			return false;
		}
		return !player.IsBuildingBlocked(base.transform.position, base.transform.rotation, bounds);
	}

	public bool IsUpgradeBlocked()
	{
		if (!blockDefinition.checkVolumeOnUpgrade)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(base.transform.position, base.transform.rotation, volumes, ~(1 << base.gameObject.layer));
	}

	public bool CanAffordUpgrade(BuildingGrade.Enum iGrade, ulong iSkin, BasePlayer player)
	{
		object obj = Interface.CallHook("CanAffordUpgrade", player, this, iGrade, iSkin);
		if (obj is bool)
		{
			return (bool)obj;
		}
		foreach (ItemAmount item in blockDefinition.GetGrade(iGrade, iSkin).CostToBuild(grade))
		{
			if ((float)player.inventory.GetAmount(item.itemid) < item.amount)
			{
				return false;
			}
		}
		return true;
	}

	public void SetGrade(BuildingGrade.Enum iGrade)
	{
		if (blockDefinition.grades == null || iGrade <= BuildingGrade.Enum.None || iGrade >= BuildingGrade.Enum.Count)
		{
			Debug.LogError("Tried to set to undefined grade! " + blockDefinition.fullName, base.gameObject);
			return;
		}
		grade = iGrade;
		grade = currentGrade.gradeBase.type;
		UpdateGrade();
	}

	public void UpdateGrade()
	{
		baseProtection = currentGrade.gradeBase.damageProtecton;
	}

	protected override void OnSkinChanged(ulong oldSkinID, ulong newSkinID)
	{
		if (oldSkinID != newSkinID)
		{
			skinID = newSkinID;
		}
	}

	protected override void OnSkinPreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}

	public void SetHealthToMax()
	{
		base.health = MaxHealth();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoUpgradeToGrade_Delayed(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		BuildingGrade.Enum @enum = (BuildingGrade.Enum)msg.read.Int32();
		ulong num = msg.read.UInt64();
		ConstructionGrade constructionGrade = blockDefinition.GetGrade(@enum, num);
		if (!(constructionGrade == null) && CanChangeToGrade(@enum, num, msg.player) && Interface.CallHook("OnStructureUpgrade", this, msg.player, @enum, num) == null && CanAffordUpgrade(@enum, num, msg.player) && !(base.SecondsSinceAttacked < 30f) && (num == 0L || msg.player.blueprints.steamInventory.HasItem((int)num)))
		{
			PayForUpgrade(constructionGrade, msg.player);
			Facepunch.Rust.Analytics.Azure.OnBuildingBlockUpgraded(msg.player, this, @enum);
			if (msg.player != null)
			{
				playerCustomColourToApply = msg.player.LastBlockColourChangeId;
			}
			ClientRPC(null, "DoUpgradeEffect", (int)@enum, num);
			OnSkinChanged(skinID, num);
			ChangeGrade(@enum, playEffect: true);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoUpgradeToGrade(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		BuildingGrade.Enum @enum = (BuildingGrade.Enum)msg.read.Int32();
		ulong num = msg.read.UInt64();
		ConstructionGrade constructionGrade = blockDefinition.GetGrade(@enum, num);
		if (!(constructionGrade == null) && CanChangeToGrade(@enum, num, msg.player) && Interface.CallHook("OnStructureUpgrade", this, msg.player, @enum, num) == null && CanAffordUpgrade(@enum, num, msg.player) && !(base.SecondsSinceAttacked < 30f) && (num == 0L || msg.player.blueprints.steamInventory.HasItem((int)num)))
		{
			PayForUpgrade(constructionGrade, msg.player);
			Facepunch.Rust.Analytics.Azure.OnBuildingBlockUpgraded(msg.player, this, @enum);
			if (msg.player != null)
			{
				playerCustomColourToApply = msg.player.LastBlockColourChangeId;
			}
			ClientRPC(null, "DoUpgradeEffect", (int)@enum, num);
			OnSkinChanged(skinID, num);
			ChangeGrade(@enum, playEffect: true);
		}
	}

	public void ChangeGradeAndSkin(BuildingGrade.Enum targetGrade, ulong skin, bool playEffect = false, bool updateSkin = true)
	{
		OnSkinChanged(skinID, skin);
		ChangeGrade(targetGrade, playEffect, updateSkin);
	}

	public void ChangeGrade(BuildingGrade.Enum targetGrade, bool playEffect = false, bool updateSkin = true)
	{
		SetGrade(targetGrade);
		if (grade != lastGrade)
		{
			SetHealthToMax();
			StartBeingRotatable();
		}
		if (updateSkin)
		{
			UpdateSkin();
		}
		SendNetworkUpdate();
		ResetUpkeepTime();
		UpdateSurroundingEntities();
		BuildingManager.server.GetBuilding(buildingID)?.Dirty();
	}

	public void PayForUpgrade(ConstructionGrade g, BasePlayer player)
	{
		if (Interface.CallHook("OnPayForUpgrade", player, this, g) != null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (ItemAmount item in g.CostToBuild(grade))
		{
			player.inventory.Take(list, item.itemid, (int)item.amount);
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition(item.itemid);
			Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "upgrade_block", itemDefinition.shortname, (int)item.amount, this, null, safezone: false, null, player.userID);
			player.Command("note.inv " + item.itemid + " " + item.amount * -1f);
		}
		foreach (Item item2 in list)
		{
			item2.Remove();
		}
	}

	public void SetCustomColour(uint newColour)
	{
		if (newColour != customColour)
		{
			customColour = newColour;
			SendNetworkUpdateImmediate();
			ClientRPC(null, "RefreshSkin");
		}
	}

	public bool NeedsSkinChange()
	{
		if (!(currentSkin == null) && !forceSkinRefresh && lastGrade == grade && lastModelState == modelState)
		{
			return lastSkinID != skinID;
		}
		return true;
	}

	public void UpdateSkin(bool force = false)
	{
		if (force)
		{
			forceSkinRefresh = true;
		}
		if (!NeedsSkinChange())
		{
			return;
		}
		if (cachedStability <= 0f || base.isServer)
		{
			ChangeSkin();
			return;
		}
		if (!skinChange)
		{
			skinChange = new DeferredAction(this, ChangeSkin);
		}
		if (skinChange.Idle)
		{
			skinChange.Invoke();
		}
	}

	private void DestroySkin()
	{
		if (currentSkin != null)
		{
			currentSkin.Destroy(this);
			currentSkin = null;
		}
	}

	public void RefreshNeighbours(bool linkToNeighbours)
	{
		List<EntityLink> entityLinks = GetEntityLinks(linkToNeighbours);
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				BuildingBlock buildingBlock = entityLink.connections[j].owner as BuildingBlock;
				if (!(buildingBlock == null))
				{
					if (Rust.Application.isLoading)
					{
						buildingBlock.UpdateSkin(force: true);
					}
					else
					{
						updateSkinQueueServer.Add(buildingBlock);
					}
				}
			}
		}
	}

	private void UpdatePlaceholder(bool state)
	{
		if ((bool)placeholderRenderer)
		{
			placeholderRenderer.enabled = state;
		}
		if ((bool)placeholderCollider)
		{
			placeholderCollider.enabled = state;
		}
	}

	private void ChangeSkin()
	{
		if (base.IsDestroyed)
		{
			return;
		}
		ConstructionGrade constructionGrade = currentGrade;
		if (constructionGrade.skinObject.isValid)
		{
			ChangeSkin(constructionGrade.skinObject);
			return;
		}
		ConstructionGrade defaultGrade = blockDefinition.defaultGrade;
		if (defaultGrade.skinObject.isValid)
		{
			ChangeSkin(defaultGrade.skinObject);
		}
		else
		{
			Debug.LogWarning("No skins found for " + base.gameObject);
		}
	}

	public void ChangeSkin(GameObjectRef prefab)
	{
		bool flag = lastGrade != grade || lastSkinID != skinID;
		lastGrade = grade;
		lastSkinID = skinID;
		if (flag)
		{
			if (currentSkin == null)
			{
				UpdatePlaceholder(state: false);
			}
			else
			{
				DestroySkin();
			}
			GameObject gameObject = base.gameManager.CreatePrefab(prefab.resourcePath, base.transform);
			currentSkin = gameObject.GetComponent<ConstructionSkin>();
			if (currentSkin != null && base.isServer && !Rust.Application.isLoading)
			{
				customColour = currentSkin.GetStartingDetailColour(playerCustomColourToApply);
			}
			Model component = currentSkin.GetComponent<Model>();
			SetModel(component);
			Assert.IsTrue(model == component, "Didn't manage to set model successfully!");
		}
		if (base.isServer)
		{
			modelState = currentSkin.DetermineConditionalModelState(this);
		}
		bool flag2 = lastModelState != modelState;
		lastModelState = modelState;
		bool flag3 = lastCustomColour != customColour;
		lastCustomColour = customColour;
		if (flag || flag2 || forceSkinRefresh || flag3)
		{
			currentSkin.Refresh(this);
			if (base.isServer && flag2)
			{
				CheckForPipes();
			}
			forceSkinRefresh = false;
		}
		if (base.isServer)
		{
			if (flag)
			{
				RefreshNeighbours(linkToNeighbours: true);
			}
			if (flag2)
			{
				SendNetworkUpdate();
			}
		}
	}

	public override bool ShouldBlockProjectiles()
	{
		return grade != BuildingGrade.Enum.Twigs;
	}

	public void CheckForPipes()
	{
		if (!CheckForPipesOnModelChange || !ConVar.Server.enforcePipeChecksOnBuildingBlockChanges || Rust.Application.isLoading)
		{
			return;
		}
		List<ColliderInfo_Pipe> obj = Facepunch.Pool.GetList<ColliderInfo_Pipe>();
		Vis.Components(new OBB(base.transform, bounds), obj, 536870912);
		foreach (ColliderInfo_Pipe item in obj)
		{
			if (!(item == null) && item.gameObject.activeInHierarchy && item.HasFlag(ColliderInfo.Flags.OnlyBlockBuildingBlock) && item.ParentEntity != null && item.ParentEntity.isServer)
			{
				WireTool.AttemptClearSlot(item.ParentEntity, null, item.OutputSlotIndex, isInput: false);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	private void OnHammered()
	{
	}

	public override float MaxHealth()
	{
		return currentGrade.maxHealth;
	}

	public override List<ItemAmount> BuildCost()
	{
		return currentGrade.CostToBuild();
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		if (base.isServer && Mathf.RoundToInt(oldvalue) != Mathf.RoundToInt(newvalue))
		{
			SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
		}
	}

	public override float RepairCostFraction()
	{
		return 1f;
	}

	public bool CanRotate(BasePlayer player)
	{
		if (IsRotatable() && HasRotationPrivilege(player))
		{
			return !IsRotationBlocked();
		}
		return false;
	}

	public bool IsRotatable()
	{
		if (blockDefinition.grades == null)
		{
			return false;
		}
		if (!blockDefinition.canRotateAfterPlacement)
		{
			return false;
		}
		if (!HasFlag(Flags.Reserved1))
		{
			return false;
		}
		return true;
	}

	public bool IsRotationBlocked()
	{
		if (!blockDefinition.checkVolumeOnRotate)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(base.transform.position, base.transform.rotation, volumes, ~(1 << base.gameObject.layer));
	}

	public bool HasRotationPrivilege(BasePlayer player)
	{
		return !player.IsBuildingBlocked(base.transform.position, base.transform.rotation, bounds);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void DoRotation(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanRotate(msg.player) && blockDefinition.canRotateAfterPlacement && Interface.CallHook("OnStructureRotate", this, msg.player) == null)
		{
			base.transform.localRotation *= Quaternion.Euler(blockDefinition.rotationAmount);
			RefreshEntityLinks();
			UpdateSurroundingEntities();
			UpdateSkin(force: true);
			RefreshNeighbours(linkToNeighbours: false);
			SendNetworkUpdateImmediate();
			ClientRPC(null, "RefreshSkin");
		}
	}

	public void StopBeingRotatable()
	{
		SetFlag(Flags.Reserved1, b: false);
		SendNetworkUpdate();
	}

	public void StartBeingRotatable()
	{
		if (blockDefinition.grades != null && blockDefinition.canRotateAfterPlacement)
		{
			SetFlag(Flags.Reserved1, b: true);
			Invoke(StopBeingRotatable, 600f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingBlock = Facepunch.Pool.Get<ProtoBuf.BuildingBlock>();
		info.msg.buildingBlock.model = modelState;
		info.msg.buildingBlock.grade = (int)grade;
		if (customColour != 0)
		{
			info.msg.simpleUint = Facepunch.Pool.Get<SimpleUInt>();
			info.msg.simpleUint.value = customColour;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		customColour = 0u;
		if (info.msg.simpleUint != null)
		{
			customColour = info.msg.simpleUint.value;
		}
		if (info.msg.buildingBlock != null)
		{
			SetConditionalModel(info.msg.buildingBlock.model);
			SetGrade((BuildingGrade.Enum)info.msg.buildingBlock.grade);
		}
		if (info.fromDisk)
		{
			SetFlag(Flags.Reserved2, b: false);
			SetFlag(Flags.Reserved1, b: false);
			UpdateSkin();
		}
	}

	public override void AttachToBuilding(DecayEntity other)
	{
		if (other != null && other is BuildingBlock)
		{
			AttachToBuilding(other.buildingID);
			BuildingManager.server.CheckMerge(this);
		}
		else
		{
			AttachToBuilding(BuildingManager.server.NewBuildingID());
		}
	}

	public override void ServerInit()
	{
		blockDefinition = PrefabAttribute.server.Find<Construction>(prefabID);
		if (blockDefinition == null)
		{
			Debug.LogError("Couldn't find Construction for prefab " + prefabID);
		}
		base.ServerInit();
		UpdateSkin();
		if (HasFlag(Flags.Reserved1) || !Rust.Application.isLoadingSave)
		{
			StartBeingRotatable();
		}
		if (HasFlag(Flags.Reserved2) || !Rust.Application.isLoadingSave)
		{
			StartBeingDemolishable();
		}
		if (!CullBushes || Rust.Application.isLoadingSave)
		{
			return;
		}
		List<BushEntity> obj = Facepunch.Pool.GetList<BushEntity>();
		Vis.Entities(WorldSpaceBounds(), obj, 67108864);
		foreach (BushEntity item in obj)
		{
			if (item.isServer)
			{
				item.Kill();
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public override void Hurt(HitInfo info)
	{
		if (ConVar.Server.pve && (bool)info.Initiator && info.Initiator is BasePlayer)
		{
			(info.Initiator as BasePlayer).Hurt(info.damageTypes.Total(), DamageType.Generic);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		blockDefinition = null;
		forceSkinRefresh = false;
		modelState = 0;
		lastModelState = 0;
		grade = BuildingGrade.Enum.Twigs;
		lastGrade = BuildingGrade.Enum.None;
		DestroySkin();
		UpdatePlaceholder(state: true);
	}

	public override void InitShared()
	{
		base.InitShared();
		placeholderRenderer = GetComponent<MeshRenderer>();
		placeholderCollider = GetComponent<MeshCollider>();
	}

	public override void PostInitShared()
	{
		baseProtection = currentGrade.gradeBase.damageProtecton;
		grade = currentGrade.gradeBase.type;
		base.PostInitShared();
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			RefreshNeighbours(linkToNeighbours: false);
		}
		base.DestroyShared();
	}

	public override string Categorize()
	{
		return "building";
	}

	public override float BoundsPadding()
	{
		return 1f;
	}

	public override bool IsOutside()
	{
		float outside_test_range = ConVar.Decay.outside_test_range;
		Vector3 vector = PivotPoint();
		for (int i = 0; i < outsideLookupOffsets.Length; i++)
		{
			Vector3 vector2 = outsideLookupOffsets[i];
			Vector3 origin = vector + vector2 * outside_test_range;
			if (!UnityEngine.Physics.Raycast(new Ray(origin, -vector2), outside_test_range - 0.5f, 2097152))
			{
				return true;
			}
		}
		return false;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
