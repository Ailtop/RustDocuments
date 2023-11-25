#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ElevatorLift : BaseCombatEntity
{
	public GameObject DescendingHurtTrigger;

	public GameObject MovementCollider;

	public Transform UpButtonPoint;

	public Transform DownButtonPoint;

	public TriggerNotify VehicleTrigger;

	public GameObjectRef LiftArrivalScreenBounce;

	public SoundDefinition liftMovementLoopDef;

	public SoundDefinition liftMovementStartDef;

	public SoundDefinition liftMovementStopDef;

	public SoundDefinition liftMovementAccentSoundDef;

	public GameObjectRef liftButtonPressedEffect;

	public float movementAccentMinInterval = 0.75f;

	public float movementAccentMaxInterval = 3f;

	private Sound liftMovementLoopSound;

	private float nextMovementAccent;

	public Vector3 lastPosition;

	private const Flags PressedUp = Flags.Reserved1;

	private const Flags PressedDown = Flags.Reserved2;

	private Elevator owner => GetParentEntity() as Elevator;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ElevatorLift.OnRpcMessage"))
		{
			if (rpc == 4061236510u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RaiseLowerFloor ");
				}
				using (TimeWarning.New("Server_RaiseLowerFloor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4061236510u, "Server_RaiseLowerFloor", this, player, 3f))
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
							Server_RaiseLowerFloor(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_RaiseLowerFloor");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ToggleHurtTrigger(state: false);
	}

	public void ToggleHurtTrigger(bool state)
	{
		if (DescendingHurtTrigger != null)
		{
			DescendingHurtTrigger.SetActive(state);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void Server_RaiseLowerFloor(RPCMessage msg)
	{
		if (!CanMove())
		{
			return;
		}
		Elevator.Direction direction = (Elevator.Direction)msg.read.Int32();
		bool flag = msg.read.Bit();
		if (Interface.CallHook("OnElevatorButtonPress", this, msg.player, direction, flag) == null)
		{
			SetFlag((direction == Elevator.Direction.Up) ? Flags.Reserved1 : Flags.Reserved2, b: true);
			owner.Server_RaiseLowerElevator(direction, flag);
			Invoke(ClearDirection, 0.7f);
			if (liftButtonPressedEffect.isValid)
			{
				Effect.server.Run(liftButtonPressedEffect.resourcePath, base.transform.position, Vector3.up);
			}
		}
	}

	private void ClearDirection()
	{
		SetFlag(Flags.Reserved1, b: false);
		SetFlag(Flags.Reserved2, b: false);
	}

	public override void Hurt(HitInfo info)
	{
		if (HasParent() && GetParentEntity() is BaseCombatEntity baseCombatEntity)
		{
			baseCombatEntity.Hurt(info);
		}
	}

	public override void AdminKill()
	{
		if (HasParent())
		{
			GetParentEntity().AdminKill();
		}
		else
		{
			base.AdminKill();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		ClearDirection();
	}

	public bool CanMove()
	{
		object obj = Interface.CallHook("CanElevatorLiftMove", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (VehicleTrigger.HasContents && VehicleTrigger.entityContents != null)
		{
			foreach (BaseEntity entityContent in VehicleTrigger.entityContents)
			{
				if (!(entityContent is Drone))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual void NotifyNewFloor(int newFloor, int totalFloors)
	{
	}

	public void ToggleMovementCollider(bool state)
	{
		if (MovementCollider != null)
		{
			MovementCollider.SetActive(state);
		}
	}
}
