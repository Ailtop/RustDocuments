#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
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

	public int modelState;

	public int lastModelState;

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

	public ConstructionGrade currentGrade
	{
		get
		{
			ConstructionGrade constructionGrade = GetGrade(grade);
			if (constructionGrade != null)
			{
				return constructionGrade;
			}
			for (int i = 0; i < blockDefinition.grades.Length; i++)
			{
				if (blockDefinition.grades[i] != null)
				{
					return blockDefinition.grades[i];
				}
			}
			Debug.LogWarning("Building block grade not found: " + grade);
			return null;
		}
	}

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
			Kill(DestroyMode.Gib);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoImmediateDemolish(RPCMessage msg)
	{
		if (msg.player.CanInteract() && msg.player.IsAdmin && Interface.CallHook("OnStructureDemolish", this, msg.player, true) == null)
		{
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

	public ConstructionGrade GetGrade(BuildingGrade.Enum iGrade)
	{
		if ((int)grade >= blockDefinition.grades.Length)
		{
			Debug.LogWarning(string.Concat("Grade out of range ", base.gameObject, " ", grade, " / ", blockDefinition.grades.Length));
			return blockDefinition.defaultGrade;
		}
		return blockDefinition.grades[(int)iGrade];
	}

	public bool CanChangeToGrade(BuildingGrade.Enum iGrade, BasePlayer player)
	{
		object obj = Interface.CallHook("CanChangeGrade", player, this, iGrade);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasUpgradePrivilege(iGrade, player))
		{
			return !IsUpgradeBlocked();
		}
		return false;
	}

	public bool HasUpgradePrivilege(BuildingGrade.Enum iGrade, BasePlayer player)
	{
		if (iGrade == grade)
		{
			return false;
		}
		if ((int)iGrade >= blockDefinition.grades.Length)
		{
			return false;
		}
		if (iGrade < BuildingGrade.Enum.Twigs)
		{
			return false;
		}
		if (iGrade < grade)
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

	public bool CanAffordUpgrade(BuildingGrade.Enum iGrade, BasePlayer player)
	{
		object obj = Interface.CallHook("CanAffordUpgrade", player, this, iGrade);
		if (obj is bool)
		{
			return (bool)obj;
		}
		foreach (ItemAmount item in GetGrade(iGrade).costToBuild)
		{
			if ((float)player.inventory.GetAmount(item.itemid) < item.amount)
			{
				return false;
			}
		}
		return true;
	}

	public void SetGrade(BuildingGrade.Enum iGradeID)
	{
		if (blockDefinition.grades == null || (int)iGradeID >= blockDefinition.grades.Length)
		{
			Debug.LogError("Tried to set to undefined grade! " + blockDefinition.fullName, base.gameObject);
			return;
		}
		grade = iGradeID;
		grade = currentGrade.gradeBase.type;
		UpdateGrade();
	}

	public void UpdateGrade()
	{
		baseProtection = currentGrade.gradeBase.damageProtecton;
	}

	public void SetHealthToMax()
	{
		base.health = MaxHealth();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void DoUpgradeToGrade(RPCMessage msg)
	{
		if (msg.player.CanInteract())
		{
			BuildingGrade.Enum @enum = (BuildingGrade.Enum)msg.read.Int32();
			ConstructionGrade constructionGrade = GetGrade(@enum);
			if (!(constructionGrade == null) && CanChangeToGrade(@enum, msg.player) && CanAffordUpgrade(@enum, msg.player) && Interface.CallHook("OnStructureUpgrade", this, msg.player, @enum) == null && !(base.SecondsSinceAttacked < 30f))
			{
				PayForUpgrade(constructionGrade, msg.player);
				ChangeGrade(@enum, playEffect: true);
			}
		}
	}

	public void ChangeGrade(BuildingGrade.Enum targetGrade, bool playEffect = false)
	{
		if (grade != targetGrade)
		{
			SetGrade(targetGrade);
			SetHealthToMax();
			StartBeingRotatable();
			SendNetworkUpdate();
			UpdateSkin();
			ResetUpkeepTime();
			UpdateSurroundingEntities();
			BuildingManager.server.GetBuilding(buildingID)?.Dirty();
			if (playEffect)
			{
				Effect.server.Run("assets/bundled/prefabs/fx/build/promote_" + targetGrade.ToString().ToLower() + ".prefab", this, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}

	public void PayForUpgrade(ConstructionGrade g, BasePlayer player)
	{
		if (Interface.CallHook("OnPayForUpgrade", player, this, g) != null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (ItemAmount item in g.costToBuild)
		{
			player.inventory.Take(list, item.itemid, (int)item.amount);
			player.Command("note.inv " + item.itemid + " " + item.amount * -1f);
		}
		foreach (Item item2 in list)
		{
			item2.Remove();
		}
	}

	public bool NeedsSkinChange()
	{
		if (!(currentSkin == null) && !forceSkinRefresh && lastGrade == grade)
		{
			return lastModelState != modelState;
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
		ConstructionGrade[] grades = blockDefinition.grades;
		foreach (ConstructionGrade constructionGrade2 in grades)
		{
			if (constructionGrade2.skinObject.isValid)
			{
				ChangeSkin(constructionGrade2.skinObject);
				return;
			}
		}
		Debug.LogWarning("No skins found for " + base.gameObject);
	}

	public void ChangeSkin(GameObjectRef prefab)
	{
		bool flag = lastGrade != grade;
		lastGrade = grade;
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
		if (flag || flag2 || forceSkinRefresh)
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
		return currentGrade.costToBuild;
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

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
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
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
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
