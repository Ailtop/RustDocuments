#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ModularCarGarage : ContainerIOEntity
{
	[Serializable]
	public class ChassisBuildOption
	{
		public GameObjectRef prefab;

		public ItemDefinition itemDef;
	}

	public enum OccupantLock
	{
		CannotHaveLock,
		NoLock,
		HasLock
	}

	private enum VehicleLiftState
	{
		Down,
		Up
	}

	public ModularCar lockedOccupant;

	public readonly HashSet<BasePlayer> lootingPlayers = new HashSet<BasePlayer>();

	public MagnetSnap magnetSnap;

	[SerializeField]
	public Transform vehicleLift;

	[SerializeField]
	public Animation vehicleLiftAnim;

	[SerializeField]
	private string animName = "LiftUp";

	[SerializeField]
	public VehicleLiftOccupantTrigger occupantTrigger;

	[SerializeField]
	public float liftMoveTime = 1f;

	[SerializeField]
	public EmissionToggle poweredLight;

	[SerializeField]
	public EmissionToggle inUseLight;

	[SerializeField]
	public Transform vehicleLiftPos;

	[SerializeField]
	[Range(0f, 1f)]
	public float recycleEfficiency = 0.5f;

	[SerializeField]
	public Transform recycleDropPos;

	[SerializeField]
	public bool needsElectricity;

	[SerializeField]
	private SoundDefinition liftStartSoundDef;

	[SerializeField]
	private SoundDefinition liftStopSoundDef;

	[SerializeField]
	private SoundDefinition liftStopDownSoundDef;

	[SerializeField]
	private SoundDefinition liftLoopSoundDef;

	public SoundDefinition liftOpenSoundDef;

	public SoundDefinition liftCloseSoundDef;

	public ChassisBuildOption[] chassisBuildOptions;

	public ItemAmount lockResourceCost;

	public ItemDefinition carKeyDefinition;

	public VehicleLiftState vehicleLiftState;

	private Sound liftLoopSound;

	public Vector3 downPos;

	public const Flags DestroyingChassis = Flags.Reserved6;

	public const float TimeToDestroyChassis = 10f;

	public ModularCar carOccupant
	{
		get
		{
			if (!(lockedOccupant != null))
			{
				return occupantTrigger.carOccupant;
			}
			return lockedOccupant;
		}
	}

	public bool HasOccupant
	{
		get
		{
			if (carOccupant != null)
			{
				return carOccupant.IsFullySpawned();
			}
			return false;
		}
	}

	public bool PlatformIsOccupied { get; set; }

	public bool HasEditableOccupant { get; set; }

	public bool HasDriveableOccupant { get; set; }

	public OccupantLock OccupantLockState { get; set; }

	public int OccupantLockID { get; set; }

	public bool LiftIsUp => vehicleLiftState == VehicleLiftState.Up;

	public bool LiftIsMoving => vehicleLiftAnim.isPlaying;

	public bool LiftIsDown => vehicleLiftState == VehicleLiftState.Down;

	public bool IsDestroyingChassis => HasFlag(Flags.Reserved6);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ModularCarGarage.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 554177909 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_DeselectedLootItem "));
				}
				using (TimeWarning.New("RPC_DeselectedLootItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(554177909u, "RPC_DeselectedLootItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_DeselectedLootItem(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_DeselectedLootItem");
					}
				}
				return true;
			}
			if (rpc == 3659332720u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenEditing "));
				}
				using (TimeWarning.New("RPC_OpenEditing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3659332720u, "RPC_OpenEditing", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3659332720u, "RPC_OpenEditing", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_OpenEditing(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_OpenEditing");
					}
				}
				return true;
			}
			if (rpc == 1582295101 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_RepairItem "));
				}
				using (TimeWarning.New("RPC_RepairItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1582295101u, "RPC_RepairItem", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1582295101u, "RPC_RepairItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							RPC_RepairItem(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_RepairItem");
					}
				}
				return true;
			}
			if (rpc == 3710764312u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_RequestAddLock "));
				}
				using (TimeWarning.New("RPC_RequestAddLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg5 = rPCMessage;
							RPC_RequestAddLock(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_RequestAddLock");
					}
				}
				return true;
			}
			if (rpc == 1151989253 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_RequestCarKey "));
				}
				using (TimeWarning.New("RPC_RequestCarKey"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1151989253u, "RPC_RequestCarKey", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1151989253u, "RPC_RequestCarKey", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg6 = rPCMessage;
							RPC_RequestCarKey(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_RequestCarKey");
					}
				}
				return true;
			}
			if (rpc == 1046853419 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_RequestRemoveLock "));
				}
				using (TimeWarning.New("RPC_RequestRemoveLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1046853419u, "RPC_RequestRemoveLock", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1046853419u, "RPC_RequestRemoveLock", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg7 = rPCMessage;
							RPC_RequestRemoveLock(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in RPC_RequestRemoveLock");
					}
				}
				return true;
			}
			if (rpc == 4033916654u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_SelectedLootItem "));
				}
				using (TimeWarning.New("RPC_SelectedLootItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4033916654u, "RPC_SelectedLootItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg8 = rPCMessage;
							RPC_SelectedLootItem(msg8);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in RPC_SelectedLootItem");
					}
				}
				return true;
			}
			if (rpc == 2974124904u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_StartDestroyingChassis "));
				}
				using (TimeWarning.New("RPC_StartDestroyingChassis"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2974124904u, "RPC_StartDestroyingChassis", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg9 = rPCMessage;
							RPC_StartDestroyingChassis(msg9);
						}
					}
					catch (Exception exception8)
					{
						Debug.LogException(exception8);
						player.Kick("RPC Error in RPC_StartDestroyingChassis");
					}
				}
				return true;
			}
			if (rpc == 3830531963u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_StopDestroyingChassis "));
				}
				using (TimeWarning.New("RPC_StopDestroyingChassis"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3830531963u, "RPC_StopDestroyingChassis", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg10 = rPCMessage;
							RPC_StopDestroyingChassis(msg10);
						}
					}
					catch (Exception exception9)
					{
						Debug.LogException(exception9);
						player.Kick("RPC Error in RPC_StopDestroyingChassis");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void FixedUpdate()
	{
		if (!base.isServer || magnetSnap == null)
		{
			return;
		}
		UpdateCarOccupant();
		if (HasOccupant && carOccupant.CouldBeEdited() && carOccupant.GetSpeed() <= 1f)
		{
			if (IsOn() || !carOccupant.IsComplete())
			{
				if (lockedOccupant == null)
				{
					GrabOccupant(occupantTrigger.carOccupant);
				}
				magnetSnap.FixedUpdate(carOccupant.transform);
			}
			if (carOccupant.carLock.HasALock && !carOccupant.carLock.CanHaveALock())
			{
				carOccupant.carLock.RemoveLock();
			}
		}
		else if (HasOccupant && carOccupant.rigidBody.isKinematic)
		{
			ReleaseOccupant();
		}
		if (HasOccupant && IsDestroyingChassis && carOccupant.HasAnyModules)
		{
			StopChassisDestroy();
		}
	}

	internal override void DoServerDestroy()
	{
		if (HasOccupant)
		{
			ReleaseOccupant();
			if (!HasDriveableOccupant)
			{
				carOccupant.Kill(DestroyMode.Gib);
			}
		}
		base.DoServerDestroy();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		magnetSnap = new MagnetSnap(vehicleLiftPos);
		RefreshOnOffState();
		SetOccupantState(false, false, false, OccupantLock.CannotHaveLock, 0, true);
		RefreshLiftState(true);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.vehicleLift = Facepunch.Pool.Get<VehicleLift>();
		info.msg.vehicleLift.platformIsOccupied = PlatformIsOccupied;
		info.msg.vehicleLift.editableOccupant = HasEditableOccupant;
		info.msg.vehicleLift.driveableOccupant = HasDriveableOccupant;
		info.msg.vehicleLift.occupantLockState = (int)OccupantLockState;
		info.msg.vehicleLift.occupantLockID = OccupantLockID;
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (player == null)
		{
			return false;
		}
		bool flag = base.PlayerOpenLoot(player, panelToOpen);
		if (!flag)
		{
			return false;
		}
		if (HasEditableOccupant)
		{
			player.inventory.loot.AddContainer(carOccupant.Inventory.ModuleContainer);
			player.inventory.loot.AddContainer(carOccupant.Inventory.ChassisContainer);
			player.inventory.loot.SendImmediate();
		}
		lootingPlayers.Add(player);
		RefreshLiftState();
		return flag;
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		lootingPlayers.Remove(player);
		base.PlayerStoppedLooting(player);
		RefreshLiftState();
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		RefreshOnOffState();
	}

	public bool TryGetModuleForItem(Item item, out BaseVehicleModule result)
	{
		if (!HasOccupant)
		{
			result = null;
			return false;
		}
		result = carOccupant.GetModuleForItem(item);
		return result != null;
	}

	public void RefreshOnOffState()
	{
		bool flag = !needsElectricity || currentEnergy >= ConsumptionAmount();
		if (flag != IsOn())
		{
			SetFlag(Flags.On, flag);
		}
	}

	public void UpdateCarOccupant()
	{
		if (base.isServer)
		{
			if (HasOccupant)
			{
				bool editableOccupant = Vector3.SqrMagnitude(carOccupant.transform.position - vehicleLiftPos.position) < 1f && carOccupant.CouldBeEdited();
				bool driveableOccupant = carOccupant.IsComplete();
				OccupantLock occupantLockState = (carOccupant.carLock.CanHaveALock() ? ((!carOccupant.carLock.HasALock) ? OccupantLock.NoLock : OccupantLock.HasLock) : OccupantLock.CannotHaveLock);
				int lockID = carOccupant.carLock.LockID;
				SetOccupantState(HasOccupant, editableOccupant, driveableOccupant, occupantLockState, lockID);
			}
			else
			{
				SetOccupantState(false, false, false, OccupantLock.CannotHaveLock, 0);
			}
		}
	}

	public void UpdateOccupantMode()
	{
		if (HasOccupant)
		{
			carOccupant.inEditableLocation = HasEditableOccupant && LiftIsUp;
			carOccupant.immuneToDecay = IsOn();
		}
	}

	public void WakeNearbyRigidbodies()
	{
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		Vis.Colliders(base.transform.position, 7f, obj, 34816);
		foreach (Collider item in obj)
		{
			Rigidbody attachedRigidbody = item.attachedRigidbody;
			if (attachedRigidbody != null && attachedRigidbody.IsSleeping())
			{
				attachedRigidbody.WakeUp();
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
			BaseRidableAnimal baseRidableAnimal;
			if (baseEntity != null && (object)(baseRidableAnimal = baseEntity as BaseRidableAnimal) != null && baseRidableAnimal.isServer)
			{
				baseRidableAnimal.UpdateDropToGroundForDuration(2f);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public void EditableOccupantEntered()
	{
		RefreshLoot();
	}

	public void EditableOccupantLeft()
	{
		RefreshLoot();
	}

	public void RefreshLoot()
	{
		List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
		obj.AddRange(lootingPlayers);
		foreach (BasePlayer item in obj)
		{
			item.inventory.loot.Clear();
			PlayerOpenLoot(item);
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public void GrabOccupant(ModularCar occupant)
	{
		if (!(occupant == null))
		{
			lockedOccupant = occupant;
			lockedOccupant.DisablePhysics();
		}
	}

	public void ReleaseOccupant()
	{
		carOccupant.inEditableLocation = false;
		carOccupant.immuneToDecay = false;
		if (lockedOccupant != null)
		{
			lockedOccupant.EnablePhysics();
			lockedOccupant = null;
		}
	}

	public void StopChassisDestroy()
	{
		if (IsInvoking(FinishDestroyingChassis))
		{
			CancelInvoke(FinishDestroyingChassis);
		}
		SetFlag(Flags.Reserved6, false);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RepairItem(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		uint num = msg.read.UInt32();
		if (!(player == null))
		{
			Item vehicleItem = carOccupant.GetVehicleItem(num);
			if (vehicleItem != null)
			{
				RepairBench.RepairAnItem(vehicleItem, player, this, 0f, false);
			}
			else
			{
				Debug.LogError(GetType().Name + ": Couldn't get item to repair, with ID: " + num);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenEditing(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && !LiftIsMoving)
		{
			PlayerOpenLoot(player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_SelectedLootItem(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		uint itemUID = msg.read.UInt32();
		if (player == null || !player.inventory.loot.IsLooting() || player.inventory.loot.entitySource != this)
		{
			return;
		}
		Item vehicleItem = carOccupant.GetVehicleItem(itemUID);
		if (vehicleItem == null || Interface.CallHook("OnVehicleModuleSelect", vehicleItem, this, player) != null)
		{
			return;
		}
		bool flag = player.inventory.loot.RemoveContainerAt(3);
		BaseVehicleModule result;
		if (TryGetModuleForItem(vehicleItem, out result))
		{
			VehicleModuleStorage vehicleModuleStorage;
			VehicleModuleCamper vehicleModuleCamper;
			if ((object)(vehicleModuleStorage = result as VehicleModuleStorage) != null)
			{
				IItemContainerEntity container = vehicleModuleStorage.GetContainer();
				if (!ObjectEx.IsUnityNull(container))
				{
					player.inventory.loot.AddContainer(container.inventory);
					flag = true;
				}
			}
			else if ((object)(vehicleModuleCamper = result as VehicleModuleCamper) != null)
			{
				IItemContainerEntity container2 = vehicleModuleCamper.GetContainer();
				if (!ObjectEx.IsUnityNull(container2))
				{
					player.inventory.loot.AddContainer(container2.inventory);
					flag = true;
				}
			}
		}
		if (flag)
		{
			player.inventory.loot.SendImmediate();
		}
		Interface.CallHook("OnVehicleModuleSelected", vehicleItem, this, player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_DeselectedLootItem(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.inventory.loot.IsLooting() && !(player.inventory.loot.entitySource != this))
		{
			if (player.inventory.loot.RemoveContainerAt(3))
			{
				player.inventory.loot.SendImmediate();
			}
			Interface.CallHook("OnVehicleModuleDeselected", this, player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestAddLock(RPCMessage msg)
	{
		if (!HasOccupant || carOccupant.carLock.HasALock)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			ItemAmount itemAmount = lockResourceCost;
			if ((float)player.inventory.GetAmount(itemAmount.itemDef.itemid) >= itemAmount.amount && carOccupant.carLock.CanCraftAKey(player, true))
			{
				player.inventory.Take(null, itemAmount.itemDef.itemid, Mathf.CeilToInt(itemAmount.amount));
				carOccupant.carLock.AddALock();
				carOccupant.carLock.TryCraftAKey(player, true);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestRemoveLock(RPCMessage msg)
	{
		if (HasOccupant && carOccupant.carLock.HasALock)
		{
			carOccupant.carLock.RemoveLock();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestCarKey(RPCMessage msg)
	{
		if (HasOccupant && carOccupant.carLock.HasALock)
		{
			BasePlayer player = msg.player;
			if (!(player == null))
			{
				carOccupant.carLock.TryCraftAKey(player, false);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_StartDestroyingChassis(RPCMessage msg)
	{
		if (!carOccupant.HasAnyModules)
		{
			Invoke(FinishDestroyingChassis, 10f);
			SetFlag(Flags.Reserved6, true);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_StopDestroyingChassis(RPCMessage msg)
	{
		StopChassisDestroy();
	}

	public void FinishDestroyingChassis()
	{
		if (HasOccupant && !carOccupant.HasAnyModules)
		{
			carOccupant.Kill(DestroyMode.Gib);
			SetFlag(Flags.Reserved6, false);
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		downPos = vehicleLift.transform.position;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			UpdateOccupantMode();
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		return IsOn();
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public void SetOccupantState(bool hasOccupant, bool editableOccupant, bool driveableOccupant, OccupantLock occupantLockState, int occupantLockID, bool forced = false)
	{
		if (PlatformIsOccupied == hasOccupant && HasEditableOccupant == editableOccupant && HasDriveableOccupant == driveableOccupant && OccupantLockState == occupantLockState && OccupantLockID == occupantLockID && !forced)
		{
			return;
		}
		bool hasEditableOccupant = HasEditableOccupant;
		PlatformIsOccupied = hasOccupant;
		HasEditableOccupant = editableOccupant;
		HasDriveableOccupant = driveableOccupant;
		OccupantLockState = occupantLockState;
		OccupantLockID = occupantLockID;
		if (base.isServer)
		{
			UpdateOccupantMode();
			SendNetworkUpdate();
			if (hasEditableOccupant && !editableOccupant)
			{
				EditableOccupantLeft();
			}
			else if (editableOccupant && !hasEditableOccupant)
			{
				EditableOccupantEntered();
			}
		}
		RefreshLiftState();
	}

	public void RefreshLiftState(bool forced = false)
	{
		VehicleLiftState desiredLiftState = ((IsOpen() || (HasEditableOccupant && !HasDriveableOccupant)) ? VehicleLiftState.Up : VehicleLiftState.Down);
		MoveLift(desiredLiftState, 0f, forced);
	}

	public void MoveLift(VehicleLiftState desiredLiftState, float startDelay = 0f, bool forced = false)
	{
		if (vehicleLiftState != desiredLiftState || forced)
		{
			VehicleLiftState vehicleLiftState2 = vehicleLiftState;
			vehicleLiftState = desiredLiftState;
			if (base.isServer)
			{
				UpdateOccupantMode();
				WakeNearbyRigidbodies();
			}
			if (!base.gameObject.activeSelf)
			{
				vehicleLiftAnim[animName].time = ((desiredLiftState == VehicleLiftState.Up) ? 1f : 0f);
				vehicleLiftAnim.Play();
			}
			else if (desiredLiftState == VehicleLiftState.Up)
			{
				Invoke(MoveLiftUp, startDelay);
			}
			else
			{
				Invoke(MoveLiftDown, startDelay);
			}
		}
	}

	public void MoveLiftUp()
	{
		AnimationState animationState = vehicleLiftAnim[animName];
		animationState.speed = animationState.length / liftMoveTime;
		vehicleLiftAnim.Play();
	}

	public void MoveLiftDown()
	{
		AnimationState animationState = vehicleLiftAnim[animName];
		animationState.speed = animationState.length / liftMoveTime;
		if (!vehicleLiftAnim.isPlaying && Vector3.Distance(vehicleLift.transform.position, downPos) > 0.01f)
		{
			animationState.time = 1f;
		}
		animationState.speed *= -1f;
		vehicleLiftAnim.Play();
	}
}
