#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackHelicopter : PlayerHelicopter
{
	public class GunnerInputState
	{
		public bool fire1;

		public bool fire2;

		public bool reload;

		public Ray eyeRay;

		public Vector3 eyePos;

		public void Reset()
		{
			fire1 = false;
			fire2 = false;
			reload = false;
			eyeRay = default(Ray);
		}
	}

	[Header("Attack Helicopter")]
	public Transform gunnerEyePos;

	[SerializeField]
	private Transform turbofanBone;

	[SerializeField]
	private GameObjectRef turretStoragePrefab;

	[SerializeField]
	private GameObjectRef rocketStoragePrefab;

	[SerializeField]
	private GameObjectRef gunCamUIPrefab;

	[SerializeField]
	private GameObjectRef gunCamUIDialogPrefab;

	[SerializeField]
	private GameObject gunCamUIParent;

	[SerializeField]
	private ParticleSystemContainer fxLightDamage;

	[SerializeField]
	private ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	private ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	private SoundDefinition damagedLightLoop;

	[SerializeField]
	private SoundDefinition damagedHeavyLoop;

	[SerializeField]
	private GameObject damageSoundTarget;

	[SerializeField]
	private MeshRenderer monitorStaticRenderer;

	[SerializeField]
	private Material monitorStatic;

	[SerializeField]
	private Material monitorStaticSafeZone;

	[SerializeField]
	[Header("Heli Pilot Flares")]
	public GameObjectRef flareFireFX;

	[SerializeField]
	public GameObjectRef pilotFlare;

	[SerializeField]
	public Transform leftFlareLaunchPos;

	[SerializeField]
	public Transform rightFlareLaunchPos;

	[SerializeField]
	public float flareLaunchVel = 10f;

	[Header("Heli Turret")]
	public Vector2 turretPitchClamp = new Vector2(-15f, 70f);

	public Vector2 turretYawClamp = new Vector2(-90f, 90f);

	public const Flags IN_GUNNER_VIEW_FLAG = Flags.Reserved9;

	public const Flags IN_SAFE_ZONE_FLAG = Flags.Reserved10;

	protected static int headingGaugeIndex = Animator.StringToHash("headingFraction");

	protected static int altGaugeIndex = Animator.StringToHash("altFraction");

	protected int altShakeIndex = -1;

	public EntityRef<AttackHelicopterTurret> turretInstance;

	public EntityRef<AttackHelicopterRockets> rocketsInstance;

	public GunnerInputState gunnerInputState = new GunnerInputState();

	public TimeSince timeSinceLastGunnerInput;

	public TimeSince timeSinceFailedWeaponFireRPC;

	public TimeSince timeSinceFailedFlareRPC;

	public bool HasSafeZoneFlag => HasFlag(Flags.Reserved10);

	public bool GunnerIsInGunnerView => HasFlag(Flags.Reserved9);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AttackHelicopter.OnRpcMessage"))
		{
			if (rpc == 3309981499u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_CloseGunnerView ");
				}
				using (TimeWarning.New("RPC_CloseGunnerView"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3309981499u, "RPC_CloseGunnerView", this, player, 3f))
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
							RPC_CloseGunnerView(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_CloseGunnerView");
					}
				}
				return true;
			}
			if (rpc == 1427416040 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenGunnerView ");
				}
				using (TimeWarning.New("RPC_OpenGunnerView"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1427416040u, "RPC_OpenGunnerView", this, player, 3f))
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
							RPC_OpenGunnerView(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_OpenGunnerView");
					}
				}
				return true;
			}
			if (rpc == 4185921214u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenStorage ");
				}
				using (TimeWarning.New("RPC_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4185921214u, "RPC_OpenStorage", this, player, 3f))
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
							RPC_OpenStorage(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenStorage");
					}
				}
				return true;
			}
			if (rpc == 148009183 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenTurret ");
				}
				using (TimeWarning.New("RPC_OpenTurret"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(148009183u, "RPC_OpenTurret", this, player, 3f))
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
							RPC_OpenTurret(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_OpenTurret");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			VehicleEngineController<PlayerHelicopter>.EngineState engineState = engineController.EngineStateFrom(old);
			if (engineController.CurEngineState != engineState)
			{
				SetFlag(Flags.Reserved5, engineController.IsStartingOrOn);
			}
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == turretStoragePrefab.GetEntity().prefabID)
		{
			AttackHelicopterTurret attackHelicopterTurret = (AttackHelicopterTurret)child;
			turretInstance.Set(attackHelicopterTurret);
			attackHelicopterTurret.owner = this;
		}
		if (child.prefabID == rocketStoragePrefab.GetEntity().prefabID)
		{
			AttackHelicopterRockets attackHelicopterRockets = (AttackHelicopterRockets)child;
			rocketsInstance.Set(attackHelicopterRockets);
			attackHelicopterRockets.owner = this;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.attackHeli != null)
		{
			turretInstance.uid = info.msg.attackHeli.turretID;
			rocketsInstance.uid = info.msg.attackHeli.rocketsID;
		}
	}

	public AttackHelicopterTurret GetTurret()
	{
		AttackHelicopterTurret attackHelicopterTurret = turretInstance.Get(base.isServer);
		if (BaseNetworkableEx.IsValid(attackHelicopterTurret))
		{
			return attackHelicopterTurret;
		}
		return null;
	}

	public AttackHelicopterRockets GetRockets()
	{
		AttackHelicopterRockets attackHelicopterRockets = rocketsInstance.Get(base.isServer);
		if (BaseNetworkableEx.IsValid(attackHelicopterRockets))
		{
			return attackHelicopterRockets;
		}
		return null;
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (!IsOn())
		{
			return;
		}
		bool num = inputState.IsDown(BUTTON.FIRE_PRIMARY);
		bool flag = inputState.WasJustPressed(BUTTON.FIRE_SECONDARY);
		if (num)
		{
			AttackHelicopterRockets rockets = GetRockets();
			if (rockets.TryFireRocket(player))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				WeaponFireFailed(rockets.GetAmmoAmount(), player);
			}
		}
		if (flag && !TryFireFlare())
		{
			FlareFireFailed(player);
		}
	}

	public override void PassengerInput(InputState inputState, BasePlayer player)
	{
		base.PassengerInput(inputState, player);
		timeSinceLastGunnerInput = 0f;
		gunnerInputState.fire1 = inputState.IsDown(BUTTON.FIRE_PRIMARY);
		gunnerInputState.fire2 = inputState.IsDown(BUTTON.FIRE_SECONDARY);
		gunnerInputState.reload = inputState.IsDown(BUTTON.RELOAD);
		gunnerInputState.eyeRay.direction = Quaternion.Euler(inputState.current.aimAngles) * Vector3.forward;
		gunnerInputState.eyeRay.origin = player.eyes.position + gunnerInputState.eyeRay.direction * 0.5f;
		if (IsOn() && GunnerIsInGunnerView)
		{
			AttackHelicopterTurret turret = GetTurret();
			if (turret.InputTick(gunnerInputState))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				turret.GetAmmoAmounts(out var _, out var available);
				WeaponFireFailed(available, player);
			}
			AttackHelicopterRockets rockets = GetRockets();
			if (rockets.InputTick(gunnerInputState, player))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_SECONDARY))
			{
				WeaponFireFailed(rockets.GetAmmoAmount(), player);
			}
		}
	}

	public void WeaponFireFailed(int ammo, BasePlayer player)
	{
		if (!((float)timeSinceFailedWeaponFireRPC <= 1f) && ammo <= 0)
		{
			ClientRPCPlayer(null, player, "WeaponFireFailed");
			timeSinceFailedWeaponFireRPC = 0f;
		}
	}

	public void FlareFireFailed(BasePlayer player)
	{
		if (!((float)timeSinceFailedFlareRPC <= 1f))
		{
			ClientRPCPlayer(null, player, "FlareFireFailed");
			timeSinceFailedFlareRPC = 0f;
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if ((float)timeSinceLastGunnerInput > 0.5f)
		{
			gunnerInputState.Reset();
		}
	}

	public override bool EnterTrigger(TriggerBase trigger)
	{
		bool result = base.EnterTrigger(trigger);
		SetFlag(Flags.Reserved10, InSafeZone());
		return result;
	}

	public override void LeaveTrigger(TriggerBase trigger)
	{
		base.LeaveTrigger(trigger);
		SetFlag(Flags.Reserved10, InSafeZone());
	}

	public override void PrePlayerDismount(BasePlayer player, BaseMountable seat)
	{
		base.PrePlayerDismount(player, seat);
		if (HasFlag(Flags.Reserved9) && IsPassenger(player))
		{
			SetFlag(Flags.Reserved9, b: false);
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			if (turretInstance.IsValid(base.isServer))
			{
				turretInstance.Get(base.isServer).DropItems();
			}
			if (rocketsInstance.IsValid(base.isServer))
			{
				rocketsInstance.Get(base.isServer).DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.attackHeli = Facepunch.Pool.Get<AttackHeli>();
		info.msg.attackHeli.turretID = turretInstance.uid;
		info.msg.attackHeli.rocketsID = rocketsInstance.uid;
	}

	public void MarkAllMountedPlayersAsHostile()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if (mounted != null)
				{
					mounted.MarkHostileFor();
				}
			}
		}
	}

	public override bool AdminFixUp(int tier)
	{
		if (!base.AdminFixUp(tier))
		{
			return false;
		}
		AttackHelicopterTurret turret = GetTurret();
		if (turret != null && turret.GetAttachedHeldEntity() == null)
		{
			ItemDefinition itemToCreate;
			ItemDefinition itemDefinition;
			switch (tier)
			{
			case 1:
				itemToCreate = ItemManager.FindItemDefinition("hmlmg");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			case 2:
				itemToCreate = ItemManager.FindItemDefinition("rifle.ak");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			default:
				itemToCreate = ItemManager.FindItemDefinition("lmg.m249");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			}
			turret.inventory.AddItem(itemToCreate, 1, 0uL);
			turret.GetAmmoAmounts(out var _, out var available);
			int num = itemDefinition.stackable * (turret.inventory.capacity - 1);
			turret.forceAcceptAmmo = true;
			if (available < num)
			{
				int num2 = num - available;
				while (num2 > 0)
				{
					int num3 = Mathf.Min(num2, itemDefinition.stackable);
					turret.inventory.AddItem(itemDefinition, itemDefinition.stackable, 0uL);
					num2 -= num3;
				}
			}
			turret.forceAcceptAmmo = false;
		}
		AttackHelicopterRockets rockets = GetRockets();
		if (rockets != null)
		{
			ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition("flare");
			ItemDefinition itemDefinition3 = tier switch
			{
				1 => ItemManager.FindItemDefinition("ammo.rocket.hv"), 
				2 => ItemManager.FindItemDefinition("ammo.rocket.hv"), 
				_ => ItemManager.FindItemDefinition("ammo.rocket.fire"), 
			};
			int num4 = itemDefinition2.stackable * 2;
			int ammoAmount = rockets.GetAmmoAmount();
			int num5 = itemDefinition3.stackable * (rockets.inventory.capacity - num4);
			if (ammoAmount < num5)
			{
				int num6 = num5 - ammoAmount;
				while (num6 > 0)
				{
					int num7 = Mathf.Min(num6, itemDefinition3.stackable);
					rockets.inventory.AddItem(itemDefinition3, itemDefinition3.stackable, 0uL);
					num6 -= num7;
				}
			}
			rockets.inventory.AddItem(itemDefinition2, num4, 0uL, ItemContainer.LimitStack.All);
		}
		return true;
	}

	public bool TryFireFlare()
	{
		AttackHelicopterRockets rockets = GetRockets();
		if (rockets != null && rockets.TryTakeFlare())
		{
			LaunchFlare();
			return true;
		}
		return false;
	}

	public void LaunchFlare()
	{
		Effect.server.Run(flareFireFX.resourcePath, this, StringPool.Get("FlareLaunchPos"), Vector3.zero, Vector3.zero);
		UnityEngine.Object.Instantiate(pilotFlare.Get(), leftFlareLaunchPos.position, Quaternion.identity).GetComponent<AttackHeliPilotFlare>().Init(-base.transform.right * flareLaunchVel);
		UnityEngine.Object.Instantiate(pilotFlare.Get(), rightFlareLaunchPos.position, Quaternion.identity).GetComponent<AttackHeliPilotFlare>().Init(base.transform.right * flareLaunchVel);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenTurret(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.isMounted || (IsSafe() && player != creatorEntity))
		{
			return;
		}
		StorageContainer turret = GetTurret();
		if (!(turret == null))
		{
			BasePlayer driver = GetDriver();
			if (!(driver != null) || !(driver != player))
			{
				turret.PlayerOpenLoot(player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.isMounted || (IsSafe() && player != creatorEntity))
		{
			return;
		}
		StorageContainer rockets = GetRockets();
		if (!(rockets == null))
		{
			BasePlayer driver = GetDriver();
			if (!(driver != null) || !(driver != player))
			{
				rockets.PlayerOpenLoot(player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenGunnerView(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player) && IsOn() && IsPassenger(player) && !InSafeZone())
		{
			SetFlag(Flags.Reserved9, b: true);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_CloseGunnerView(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsPassenger(player))
		{
			SetFlag(Flags.Reserved9, b: false);
		}
	}
}
