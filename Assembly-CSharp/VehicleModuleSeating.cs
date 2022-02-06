#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleSeating : BaseVehicleModule, IPrefabPreProcess
{
	[Serializable]
	public class MountHotSpot
	{
		public Transform transform;

		public Vector2 size;
	}

	[Serializable]
	public class Seating
	{
		[Header("Seating & Controls")]
		public bool doorsAreLockable = true;

		[Obsolete("Use BaseVehicle.mountPoints instead")]
		[HideInInspector]
		public MountPointInfo[] mountPoints;

		public Transform steeringWheel;

		public Transform accelPedal;

		public Transform brakePedal;

		public Transform steeringWheelLeftGrip;

		public Transform steeringWheelRightGrip;

		public Transform accelPedalGrip;

		public Transform brakePedalGrip;

		public MountHotSpot[] mountHotSpots;

		[Header("Dashboard")]
		public Transform speedometer;

		public Transform fuelGauge;

		public Renderer dashboardRenderer;

		[Range(0f, 3f)]
		public int checkEngineLightMatIndex = 2;

		[ColorUsage(true, true)]
		public Color checkEngineLightEmission;

		[Range(0f, 3f)]
		public int fuelLightMatIndex = 3;

		[ColorUsage(true, true)]
		public Color fuelLightEmission;
	}

	[SerializeField]
	private ProtectionProperties passengerProtection;

	[SerializeField]
	private Seating seating;

	[SerializeField]
	[HideInInspector]
	private Vector3 steerAngle;

	[SerializeField]
	[HideInInspector]
	private Vector3 accelAngle;

	[SerializeField]
	[HideInInspector]
	private Vector3 brakeAngle;

	[SerializeField]
	[HideInInspector]
	private Vector3 speedometerAngle;

	[SerializeField]
	[HideInInspector]
	private Vector3 fuelAngle;

	[Header("Horn")]
	[SerializeField]
	private SoundDefinition hornLoop;

	[SerializeField]
	private SoundDefinition hornStart;

	private const Flags FLAG_HORN = Flags.Reserved8;

	private float steerPercent;

	private float throttlePercent;

	private float brakePercent;

	private bool? checkEngineLightOn;

	private bool? fuelLightOn;

	protected IVehicleLockUser VehicleLockUser;

	private MaterialPropertyBlock dashboardLightPB;

	private static int emissionColorID = Shader.PropertyToID("_EmissionColor");

	private BasePlayer hornPlayer;

	public override bool HasSeating => mountPoints.Count > 0;

	protected ModularCar Car { get; private set; }

	protected bool IsOnACar => Car != null;

	protected bool IsOnAVehicleLockUser => VehicleLockUser != null;

	public bool DoorsAreLockable => seating.doorsAreLockable;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleSeating.OnRpcMessage"))
		{
			if (rpc == 2791546333u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_DestroyLock "));
				}
				using (TimeWarning.New("RPC_DestroyLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2791546333u, "RPC_DestroyLock", this, player, 3f))
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
							RPC_DestroyLock(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_DestroyLock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (seating.steeringWheel != null)
		{
			steerAngle = seating.steeringWheel.localEulerAngles;
		}
		if (seating.accelPedal != null)
		{
			accelAngle = seating.accelPedal.localEulerAngles;
		}
		if (seating.brakePedal != null)
		{
			brakeAngle = seating.brakePedal.localEulerAngles;
		}
		if (seating.speedometer != null)
		{
			speedometerAngle = new Vector3(-160f, 0f, -40f);
		}
		if (seating.fuelGauge != null)
		{
			fuelAngle = seating.fuelGauge.localEulerAngles;
		}
	}

	public virtual bool IsOnThisModule(BasePlayer player)
	{
		BaseMountable mounted = player.GetMounted();
		if (mounted != null)
		{
			return mounted.GetParentEntity() as VehicleModuleSeating == this;
		}
		return false;
	}

	public bool HasADriverSeat()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.isDriver)
			{
				return true;
			}
		}
		return false;
	}

	public override void ModuleAdded(BaseModularVehicle vehicle, int firstSocketIndex)
	{
		base.ModuleAdded(vehicle, firstSocketIndex);
		Car = vehicle as ModularCar;
		VehicleLockUser = vehicle as IVehicleLockUser;
		if (!HasSeating || !base.isServer)
		{
			return;
		}
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			ModularCarSeat modularCarSeat;
			if ((object)(modularCarSeat = mountPoint.mountable as ModularCarSeat) != null)
			{
				modularCarSeat.associatedSeatingModule = this;
			}
		}
	}

	public override void ModuleRemoved()
	{
		base.ModuleRemoved();
		Car = null;
		VehicleLockUser = null;
	}

	public bool PlayerCanDestroyLock(BasePlayer player)
	{
		if (!IsOnAVehicleLockUser || player == null)
		{
			return false;
		}
		if (base.Vehicle.IsDead())
		{
			return false;
		}
		if (!HasADriverSeat())
		{
			return false;
		}
		if (!VehicleLockUser.PlayerCanDestroyLock(player, this))
		{
			return false;
		}
		if (player.isMounted)
		{
			return !VehicleLockUser.PlayerHasUnlockPermission(player);
		}
		return true;
	}

	protected BaseVehicleSeat GetSeatAtIndex(int index)
	{
		return mountPoints[index].mountable as BaseVehicleSeat;
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		if (passengerProtection != null)
		{
			passengerProtection.Scale(info.damageTypes);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (hornLoop != null && IsOnThisModule(player))
		{
			bool flag = inputState.IsDown(BUTTON.FIRE_PRIMARY);
			if (flag != HasFlag(Flags.Reserved8))
			{
				SetFlag(Flags.Reserved8, flag);
			}
			if (flag)
			{
				hornPlayer = player;
			}
		}
	}

	public override void OnPlayerDismountedVehicle(BasePlayer player)
	{
		base.OnPlayerDismountedVehicle(player);
		if (HasFlag(Flags.Reserved8) && player == hornPlayer)
		{
			SetFlag(Flags.Reserved8, false);
		}
	}

	protected bool ModuleHasMountPoint(MountPointInfo mountPointInfo)
	{
		ModularCarSeat modularCarSeat = mountPointInfo.mountable as ModularCarSeat;
		if (modularCarSeat != null)
		{
			return modularCarSeat.associatedSeatingModule == this;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_DestroyLock(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsOnAVehicleLockUser && PlayerCanDestroyLock(player))
		{
			VehicleLockUser.RemoveLock();
		}
	}

	protected virtual Vector3 ModifySeatPositionLocalSpace(int index, Vector3 desiredPos)
	{
		return desiredPos;
	}
}
