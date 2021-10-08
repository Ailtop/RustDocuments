#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Door : AnimatedBuildingBlock, INotifyTrigger
{
	public GameObjectRef knockEffect;

	public bool canTakeLock = true;

	public bool hasHatch;

	public bool canTakeCloser;

	public bool canTakeKnocker;

	public bool canNpcOpen = true;

	public bool canHandOpen = true;

	public bool isSecurityDoor;

	public TriggerNotify[] vehiclePhysBoxes;

	public bool checkPhysBoxesOnOpen;

	public SoundDefinition vehicleCollisionSfx;

	public GameObject[] ClosedColliderRoots;

	private float decayResetTimeLast = float.NegativeInfinity;

	public NavMeshModifierVolume NavMeshVolumeAnimals;

	public NavMeshModifierVolume NavMeshVolumeHumanoids;

	public NavMeshLink NavMeshLink;

	public NPCDoorTriggerBox NpcTriggerBox;

	private static int nonWalkableArea = -1;

	private static int animalAgentTypeId = -1;

	private static int humanoidAgentTypeId = -1;

	private Dictionary<BasePlayer, TimeSince> woundedOpens = new Dictionary<BasePlayer, TimeSince>();

	private Dictionary<BasePlayer, TimeSince> woundedCloses = new Dictionary<BasePlayer, TimeSince>();

	private float nextKnockTime = float.NegativeInfinity;

	private static int openHash = Animator.StringToHash("open");

	private static int closeHash = Animator.StringToHash("close");

	private bool HasVehiclePushBoxes
	{
		get
		{
			if (vehiclePhysBoxes != null)
			{
				return vehiclePhysBoxes.Length != 0;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Door.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 3999508679u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_CloseDoor "));
				}
				using (TimeWarning.New("RPC_CloseDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3999508679u, "RPC_CloseDoor", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_CloseDoor(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_CloseDoor");
					}
				}
				return true;
			}
			if (rpc == 1487779344 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_KnockDoor "));
				}
				using (TimeWarning.New("RPC_KnockDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1487779344u, "RPC_KnockDoor", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							RPC_KnockDoor(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_KnockDoor");
					}
				}
				return true;
			}
			if (rpc == 3314360565u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenDoor "));
				}
				using (TimeWarning.New("RPC_OpenDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3314360565u, "RPC_OpenDoor", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							RPC_OpenDoor(rpc4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenDoor");
					}
				}
				return true;
			}
			if (rpc == 3000490601u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_ToggleHatch "));
				}
				using (TimeWarning.New("RPC_ToggleHatch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3000490601u, "RPC_ToggleHatch", this, player, 3f))
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
							RPCMessage rpc5 = rPCMessage;
							RPC_ToggleHatch(rpc5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_ToggleHatch");
					}
				}
				return true;
			}
			if (rpc == 3672787865u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_NotifyWoundedClose "));
				}
				using (TimeWarning.New("Server_NotifyWoundedClose"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3672787865u, "Server_NotifyWoundedClose", this, player, 3f))
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
							Server_NotifyWoundedClose(msg2);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in Server_NotifyWoundedClose");
					}
				}
				return true;
			}
			if (rpc == 3730851545u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_NotifyWoundedOpen "));
				}
				using (TimeWarning.New("Server_NotifyWoundedOpen"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3730851545u, "Server_NotifyWoundedOpen", this, player, 3f))
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
							Server_NotifyWoundedOpen(msg3);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in Server_NotifyWoundedOpen");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		if (base.isServer)
		{
			decayResetTimeLast = float.NegativeInfinity;
			if (isSecurityDoor && NavMeshLink != null)
			{
				SetNavMeshLinkEnabled(false);
			}
			woundedCloses.Clear();
			woundedOpens.Clear();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (nonWalkableArea < 0)
		{
			nonWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
		}
		if (animalAgentTypeId < 0)
		{
			animalAgentTypeId = NavMesh.GetSettingsByIndex(1).agentTypeID;
		}
		if (NavMeshVolumeAnimals == null)
		{
			NavMeshVolumeAnimals = base.gameObject.AddComponent<NavMeshModifierVolume>();
			NavMeshVolumeAnimals.area = nonWalkableArea;
			NavMeshVolumeAnimals.AddAgentType(animalAgentTypeId);
			NavMeshVolumeAnimals.center = Vector3.zero;
			NavMeshVolumeAnimals.size = Vector3.one;
		}
		if (HasSlot(Slot.Lock))
		{
			canNpcOpen = false;
		}
		if (!canNpcOpen)
		{
			if (humanoidAgentTypeId < 0)
			{
				humanoidAgentTypeId = NavMesh.GetSettingsByIndex(0).agentTypeID;
			}
			if (NavMeshVolumeHumanoids == null)
			{
				NavMeshVolumeHumanoids = base.gameObject.AddComponent<NavMeshModifierVolume>();
				NavMeshVolumeHumanoids.area = nonWalkableArea;
				NavMeshVolumeHumanoids.AddAgentType(humanoidAgentTypeId);
				NavMeshVolumeHumanoids.center = Vector3.zero;
				NavMeshVolumeHumanoids.size = Vector3.one + Vector3.up + Vector3.forward;
			}
		}
		else if (NpcTriggerBox == null)
		{
			if (isSecurityDoor)
			{
				NavMeshObstacle navMeshObstacle = base.gameObject.AddComponent<NavMeshObstacle>();
				navMeshObstacle.carving = true;
				navMeshObstacle.center = Vector3.zero;
				navMeshObstacle.size = Vector3.one;
				navMeshObstacle.shape = NavMeshObstacleShape.Box;
			}
			NpcTriggerBox = new GameObject("NpcTriggerBox").AddComponent<NPCDoorTriggerBox>();
			NpcTriggerBox.Setup(this);
		}
		AIInformationZone forPoint = AIInformationZone.GetForPoint(base.transform.position);
		if (forPoint != null && NavMeshLink == null)
		{
			NavMeshLink = forPoint.GetClosestNavMeshLink(base.transform.position);
		}
		DisableVehiclePhysBox();
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock && canTakeLock)
		{
			return true;
		}
		switch (slot)
		{
		case Slot.UpperModifier:
			return true;
		case Slot.CenterDecoration:
			if (canTakeCloser)
			{
				return true;
			}
			break;
		}
		if (slot == Slot.LowerCenterDecoration && canTakeKnocker)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (!IsOpen())
		{
			return false;
		}
		if ((bool)GetSlot(Slot.Lock))
		{
			return false;
		}
		if ((bool)GetSlot(Slot.UpperModifier))
		{
			return false;
		}
		if ((bool)GetSlot(Slot.CenterDecoration))
		{
			return false;
		}
		if ((bool)GetSlot(Slot.LowerCenterDecoration))
		{
			return false;
		}
		return base.CanPickup(player);
	}

	public void CloseRequest()
	{
		SetOpen(false);
	}

	public void SetOpen(bool open, bool suppressBlockageChecks = false)
	{
		SetFlag(Flags.Open, open);
		SendNetworkUpdateImmediate();
		if (isSecurityDoor && NavMeshLink != null)
		{
			SetNavMeshLinkEnabled(open);
		}
		if (!suppressBlockageChecks && (!open || checkPhysBoxesOnOpen))
		{
			StartCheckingForBlockages();
		}
	}

	public void SetLocked(bool locked)
	{
		SetFlag(Flags.Locked, false);
		SendNetworkUpdateImmediate();
	}

	public bool GetPlayerLockPermission(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock == null)
		{
			return true;
		}
		return baseLock.GetPlayerLockPermission(player);
	}

	public void SetNavMeshLinkEnabled(bool wantsOn)
	{
		if (NavMeshLink != null)
		{
			if (wantsOn)
			{
				NavMeshLink.gameObject.SetActive(true);
				NavMeshLink.enabled = true;
			}
			else
			{
				NavMeshLink.enabled = false;
				NavMeshLink.gameObject.SetActive(false);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_OpenDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(true) || !canHandOpen || IsOpen() || IsBusy() || IsLocked())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(rpc.player) || !((float)woundedOpens[rpc.player] > 2.5f))
			{
				return;
			}
			woundedOpens.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock != null)
		{
			if (!baseLock.OnTryToOpen(rpc.player))
			{
				return;
			}
			if (baseLock.IsLocked() && UnityEngine.Time.realtimeSinceStartup - decayResetTimeLast > 60f)
			{
				BuildingBlock buildingBlock = FindLinkedEntity<BuildingBlock>();
				if ((bool)buildingBlock)
				{
					Decay.BuildingDecayTouch(buildingBlock);
				}
				else
				{
					Decay.RadialDecayTouch(base.transform.position, 40f, 2097408);
				}
				decayResetTimeLast = UnityEngine.Time.realtimeSinceStartup;
			}
		}
		SetFlag(Flags.Open, true);
		SendNetworkUpdateImmediate();
		if (isSecurityDoor && NavMeshLink != null)
		{
			SetNavMeshLinkEnabled(true);
		}
		if (checkPhysBoxesOnOpen)
		{
			StartCheckingForBlockages();
		}
		Interface.CallHook("OnDoorOpened", this, rpc.player);
	}

	private void StartCheckingForBlockages()
	{
		if (HasVehiclePushBoxes)
		{
			Invoke(EnableVehiclePhysBoxes, 0.25f);
			Invoke(DisableVehiclePhysBox, 4f);
		}
	}

	private void StopCheckingForBlockages()
	{
		if (HasVehiclePushBoxes)
		{
			ToggleVehiclePushBoxes(false);
			CancelInvoke(DisableVehiclePhysBox);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_CloseDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(true) || !canHandOpen || !IsOpen() || IsBusy() || IsLocked())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(rpc.player) || !((float)woundedCloses[rpc.player] > 2.5f))
			{
				return;
			}
			woundedCloses.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (!(baseLock != null) || baseLock.OnTryToClose(rpc.player))
		{
			SetFlag(Flags.Open, false);
			SendNetworkUpdateImmediate();
			if (isSecurityDoor && NavMeshLink != null)
			{
				SetNavMeshLinkEnabled(false);
			}
			StartCheckingForBlockages();
			Interface.CallHook("OnDoorClosed", this, rpc.player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_KnockDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(true) || !knockEffect.isValid || UnityEngine.Time.realtimeSinceStartup < nextKnockTime)
		{
			return;
		}
		nextKnockTime = UnityEngine.Time.realtimeSinceStartup + 0.5f;
		BaseEntity slot = GetSlot(Slot.LowerCenterDecoration);
		if (slot != null)
		{
			DoorKnocker component = slot.GetComponent<DoorKnocker>();
			if ((bool)component)
			{
				component.Knock(rpc.player);
				return;
			}
		}
		Effect.server.Run(knockEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		Interface.CallHook("OnDoorKnocked", this, rpc.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_ToggleHatch(RPCMessage rpc)
	{
		if (rpc.player.CanInteract(true) && hasHatch)
		{
			BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
			if (!baseLock || baseLock.OnTryToOpen(rpc.player))
			{
				SetFlag(Flags.Reserved3, !HasFlag(Flags.Reserved3));
			}
		}
	}

	private void EnableVehiclePhysBoxes()
	{
		ToggleVehiclePushBoxes(true);
	}

	private void DisableVehiclePhysBox()
	{
		ToggleVehiclePushBoxes(false);
	}

	private void ToggleVehiclePushBoxes(bool state)
	{
		if (vehiclePhysBoxes == null)
		{
			return;
		}
		TriggerNotify[] array = vehiclePhysBoxes;
		foreach (TriggerNotify triggerNotify in array)
		{
			if (triggerNotify != null)
			{
				triggerNotify.gameObject.SetActive(state);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_NotifyWoundedOpen(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(player))
			{
				woundedOpens.Add(player, default(TimeSince));
			}
			else
			{
				woundedOpens[player] = 0f;
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedOpens);
			}, 5f);
		}
	}

	private void CheckTimedOutPlayers(Dictionary<BasePlayer, TimeSince> dictionary)
	{
		List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
		foreach (KeyValuePair<BasePlayer, TimeSince> item in dictionary)
		{
			if ((float)item.Value > 5f)
			{
				obj.Add(item.Key);
			}
		}
		foreach (BasePlayer item2 in obj)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary.Remove(item2);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_NotifyWoundedClose(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(player))
			{
				woundedCloses.Add(player, default(TimeSince));
			}
			else
			{
				woundedCloses[player] = 0f;
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedCloses);
			}, 5f);
		}
	}

	private void ReverseDoorAnimation(bool wasOpening)
	{
		if (!(model == null) && !(model.animator == null))
		{
			AnimatorStateInfo currentAnimatorStateInfo = model.animator.GetCurrentAnimatorStateInfo(0);
			model.animator.Play(wasOpening ? closeHash : openHash, 0, 1f - currentAnimatorStateInfo.normalizedTime);
		}
	}

	public override float BoundsPadding()
	{
		return 2f;
	}

	public void OnObjects(TriggerNotify trigger)
	{
		if (!base.isServer)
		{
			return;
		}
		bool flag = false;
		foreach (BaseEntity entityContent in trigger.entityContents)
		{
			BaseMountable baseMountable;
			if ((object)(baseMountable = entityContent as BaseMountable) != null && baseMountable.BlocksDoors)
			{
				flag = true;
				break;
			}
			BaseVehicleModule baseVehicleModule;
			if ((object)(baseVehicleModule = entityContent as BaseVehicleModule) != null && baseVehicleModule.Vehicle != null && baseVehicleModule.Vehicle.BlocksDoors)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			bool flag2 = HasFlag(Flags.Open);
			SetOpen(!flag2, true);
			ReverseDoorAnimation(flag2);
			StopCheckingForBlockages();
			ClientRPC(null, "OnDoorInterrupted", flag2 ? 1 : 0);
		}
	}

	public void OnEmpty()
	{
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			BaseEntity slot = GetSlot(Slot.UpperModifier);
			if ((bool)slot)
			{
				slot.SendMessage("Think");
			}
		}
		if (ClosedColliderRoots == null)
		{
			return;
		}
		bool active = !HasFlag(Flags.Open) || HasFlag(Flags.Busy);
		GameObject[] closedColliderRoots = ClosedColliderRoots;
		foreach (GameObject gameObject in closedColliderRoots)
		{
			if (gameObject != null)
			{
				gameObject.gameObject.SetActive(active);
			}
		}
	}
}
