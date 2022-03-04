#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class MagnetCrane : GroundVehicle, CarPhysics<MagnetCrane>.ICar
{
	public float nextInputTime;

	public float steerInput;

	public float throttleInput;

	private float brakeInput;

	public float extensionInput;

	public float yawInput;

	public float raiseArmInput;

	public float extensionMove;

	public float yawMove;

	public float raiseArmMove;

	public float nextToggleTime;

	public Vector3 spawnOrigin = Vector3.zero;

	public float lastExtensionArmState;

	public float lastRaiseArmState;

	public float lastYawState;

	public bool handbrakeOn = true;

	public float nextSelfHealTime;

	public Vector3 lastDamagePos = Vector3.zero;

	public float lastDrivenTime;

	public float lastFixedUpdateTime;

	public CarPhysics<MagnetCrane> carPhysics;

	public VehicleTerrainHandler serverTerrainHandler;

	private Vector3 inertiaTensor = new Vector3(25000f, 11000f, 19000f);

	[Header("Magnet Crane")]
	public Animator animator;

	[SerializeField]
	private Transform COM;

	[SerializeField]
	public float arm1Speed = 0.01f;

	[SerializeField]
	public float arm2Speed = 0.01f;

	[SerializeField]
	public float turnYawSpeed = 0.01f;

	[SerializeField]
	public BaseMagnet Magnet;

	[SerializeField]
	private MagnetCraneAudio mcAudio;

	[SerializeField]
	public Rigidbody myRigidbody;

	[SerializeField]
	private Transform[] collisionTestingPoints;

	[SerializeField]
	public float maxDistanceFromOrigin;

	[SerializeField]
	public GameObjectRef selfDamageEffect;

	[SerializeField]
	private GameObjectRef explosionEffect;

	[SerializeField]
	private Transform explosionPoint;

	[SerializeField]
	private CapsuleCollider driverCollision;

	[SerializeField]
	private Transform leftHandTarget;

	[SerializeField]
	private Transform rightHandTarget;

	[SerializeField]
	public float idleFuelPerSec;

	[SerializeField]
	public float maxFuelPerSec;

	[SerializeField]
	private GameObject[] OnTriggers;

	[SerializeField]
	public TriggerHurtEx magnetDamage;

	[SerializeField]
	public int engineKW = 250;

	[SerializeField]
	private CarWheel[] wheels;

	[SerializeField]
	public CarSettings carSettings;

	[SerializeField]
	private ParticleSystem exhaustInner;

	[SerializeField]
	private ParticleSystem exhaustOuter;

	public static readonly Translate.Phrase ReturnMessage = new Translate.Phrase("junkyardcrane.return", "Return to the Junkyard. Excessive damage will occur.");

	public float extensionArmState;

	public float raiseArmState;

	public float yawState = 5f;

	private float testPreviousYaw = 5f;

	private const Flags Flag_ArmMovement = Flags.Reserved7;

	private const Flags Flag_BaseMovementInput = Flags.Reserved10;

	public VehicleTerrainHandler.Surface OnSurface
	{
		get
		{
			if (serverTerrainHandler == null)
			{
				return VehicleTerrainHandler.Surface.Default;
			}
			return serverTerrainHandler.OnSurface;
		}
	}

	public override float DriveWheelVelocity => GetSpeed();

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MagnetCrane.OnRpcMessage"))
		{
			if (rpc == 1851540757 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenFuel "));
				}
				using (TimeWarning.New("RPC_OpenFuel"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_OpenFuel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenFuel");
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
		InvokeRepeating(UpdateParams, 0f, 0.1f);
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		myRigidbody.centerOfMass = COM.localPosition;
		carPhysics = new CarPhysics<MagnetCrane>(this, base.transform, rigidBody, carSettings);
		serverTerrainHandler = new VehicleTerrainHandler(this);
		Magnet.SetMagnetEnabled(false);
		spawnOrigin = base.transform.position;
		lastDrivenTime = UnityEngine.Time.realtimeSinceStartup;
		GameObject[] onTriggers = OnTriggers;
		for (int i = 0; i < onTriggers.Length; i++)
		{
			onTriggers[i].SetActive(false);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (!IsDriver(player))
		{
			return;
		}
		throttleInput = 0f;
		steerInput = 0f;
		extensionInput = 0f;
		yawInput = 0f;
		raiseArmInput = 0f;
		if (engineController.IsOff)
		{
			if (inputState.IsAnyDown())
			{
				engineController.TryStartEngine(player);
			}
		}
		else if (engineController.IsOn)
		{
			bool num = inputState.IsDown(BUTTON.SPRINT);
			if (inputState.IsDown(BUTTON.RELOAD) && UnityEngine.Time.realtimeSinceStartup > nextToggleTime)
			{
				Magnet.SetMagnetEnabled(!Magnet.IsMagnetOn());
				nextToggleTime = UnityEngine.Time.realtimeSinceStartup + 0.5f;
			}
			if (num)
			{
				float speed = GetSpeed();
				float num2 = 0f;
				if (inputState.IsDown(BUTTON.FORWARD))
				{
					num2 = 1f;
				}
				else if (inputState.IsDown(BUTTON.BACKWARD))
				{
					num2 = -1f;
				}
				if (speed > 1f && num2 < 0f)
				{
					throttleInput = 0f;
					brakeInput = 0f - num2;
				}
				else if (speed < -1f && num2 > 0f)
				{
					throttleInput = 0f;
					brakeInput = num2;
				}
				else
				{
					throttleInput = num2;
					brakeInput = 0f;
				}
				if (inputState.IsDown(BUTTON.RIGHT))
				{
					steerInput = -1f;
				}
				if (inputState.IsDown(BUTTON.LEFT))
				{
					steerInput = 1f;
				}
			}
			else if (UnityEngine.Time.realtimeSinceStartup >= nextInputTime)
			{
				if (inputState.IsDown(BUTTON.RIGHT))
				{
					yawInput = -1f;
				}
				if (inputState.IsDown(BUTTON.LEFT))
				{
					yawInput = 1f;
				}
				if (inputState.IsDown(BUTTON.FORWARD))
				{
					raiseArmInput = 1f;
				}
				if (inputState.IsDown(BUTTON.BACKWARD))
				{
					raiseArmInput = -1f;
				}
			}
			if (UnityEngine.Time.realtimeSinceStartup >= nextInputTime)
			{
				if (inputState.IsDown(BUTTON.FIRE_PRIMARY))
				{
					extensionInput = 1f;
				}
				if (inputState.IsDown(BUTTON.FIRE_SECONDARY))
				{
					extensionInput = -1f;
				}
			}
		}
		handbrakeOn = throttleInput == 0f && steerInput == 0f;
	}

	public override float MaxVelocity()
	{
		return Mathf.Max(GetMaxForwardSpeed() * 1.3f, 30f);
	}

	public float GetSteerInput()
	{
		return steerInput;
	}

	public bool GetSteerModInput()
	{
		return false;
	}

	public override void OnEngineStartFailed()
	{
	}

	public override bool MeetsEngineRequirements()
	{
		return HasDriver();
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		rigidBody.inertiaTensor = inertiaTensor;
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = Mathf.Clamp(realtimeSinceStartup - lastFixedUpdateTime, 0f, 0.5f);
		lastFixedUpdateTime = realtimeSinceStartup;
		float speed = GetSpeed();
		carPhysics.FixedUpdate(UnityEngine.Time.fixedDeltaTime, speed);
		serverTerrainHandler.FixedUpdate();
		bool flag = IsOn();
		if (IsOn())
		{
			float t = Mathf.Max(Mathf.Abs(throttleInput), Mathf.Abs(steerInput));
			float num2 = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, t);
			if (!Magnet.HasConnectedObject())
			{
				num2 = Mathf.Min(num2, maxFuelPerSec * 0.75f);
			}
			engineController.TickFuel(num2);
		}
		engineController.CheckEngineState();
		if (IsOn() != flag)
		{
			GameObject[] onTriggers = OnTriggers;
			for (int i = 0; i < onTriggers.Length; i++)
			{
				onTriggers[i].SetActive(IsOn());
			}
		}
		if (Vector3.Dot(base.transform.up, Vector3.down) >= 0.4f)
		{
			Kill(DestroyMode.Gib);
			return;
		}
		if (realtimeSinceStartup > lastDrivenTime + 14400f)
		{
			Kill(DestroyMode.Gib);
			return;
		}
		if (spawnOrigin != Vector3.zero && maxDistanceFromOrigin != 0f)
		{
			if (Vector3Ex.Distance2D(base.transform.position, spawnOrigin) > maxDistanceFromOrigin)
			{
				if (Vector3Ex.Distance2D(base.transform.position, lastDamagePos) > 6f)
				{
					if (GetDriver() != null)
					{
						GetDriver().ShowToast(1, ReturnMessage);
					}
					Hurt(MaxHealth() * 0.15f, DamageType.Generic, this, false);
					lastDamagePos = base.transform.position;
					nextSelfHealTime = realtimeSinceStartup + 3600f;
					Effect.server.Run(selfDamageEffect.resourcePath, base.transform.position + Vector3.up * 2f, Vector3.up);
					return;
				}
			}
			else if (base.healthFraction < 1f && realtimeSinceStartup > nextSelfHealTime && base.SecondsSinceAttacked > 600f)
			{
				Heal(1000f);
			}
		}
		if (!HasDriver() || !IsOn())
		{
			handbrakeOn = true;
			throttleInput = 0f;
			steerInput = 0f;
			SetFlag(Flags.Reserved10, false);
			Magnet.SetMagnetEnabled(false);
		}
		else
		{
			lastDrivenTime = realtimeSinceStartup;
			if (Magnet.IsMagnetOn() && Magnet.HasConnectedObject() && GamePhysics.CheckOBB(Magnet.GetConnectedOBB(0.75f), 1084293121, QueryTriggerInteraction.Ignore))
			{
				Magnet.SetMagnetEnabled(false);
				nextToggleTime = realtimeSinceStartup + 2f;
				Effect.server.Run(selfDamageEffect.resourcePath, Magnet.transform.position, Vector3.up);
			}
		}
		extensionMove = _003CVehicleFixedUpdate_003Eg__UpdateMoveInput_007C33_0(extensionInput, extensionMove, 3f, UnityEngine.Time.fixedDeltaTime);
		yawMove = _003CVehicleFixedUpdate_003Eg__UpdateMoveInput_007C33_0(yawInput, yawMove, 3f, UnityEngine.Time.fixedDeltaTime);
		raiseArmMove = _003CVehicleFixedUpdate_003Eg__UpdateMoveInput_007C33_0(raiseArmInput, raiseArmMove, 3f, UnityEngine.Time.fixedDeltaTime);
		bool flag2 = extensionInput != 0f || raiseArmInput != 0f || yawInput != 0f;
		SetFlag(Flags.Reserved7, flag2);
		magnetDamage.damageEnabled = IsOn() && flag2;
		extensionArmState += extensionInput * arm1Speed * num;
		raiseArmState += raiseArmInput * arm2Speed * num;
		yawState += yawInput * turnYawSpeed * num;
		extensionArmState = Mathf.Clamp(extensionArmState, -1f, 1f);
		raiseArmState = Mathf.Clamp(raiseArmState, -1f, 1f);
		UpdateAnimator(UnityEngine.Time.fixedDeltaTime, false);
		Magnet.MagnetThink(UnityEngine.Time.fixedDeltaTime);
		SetFlag(Flags.Reserved10, throttleInput != 0f || steerInput != 0f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.crane = Facepunch.Pool.Get<Crane>();
		info.msg.crane.arm1 = extensionArmState;
		info.msg.crane.arm2 = raiseArmState;
		info.msg.crane.yaw = yawState;
	}

	public void UpdateParams()
	{
		SendNetworkUpdate();
	}

	public void LateUpdate()
	{
		if (base.isClient)
		{
			return;
		}
		if (HasDriver() && DidCollide())
		{
			if (UnityEngine.Time.realtimeSinceStartup > nextInputTime)
			{
				nextInputTime = UnityEngine.Time.realtimeSinceStartup + 0.5f;
				extensionArmState = lastExtensionArmState;
				raiseArmState = lastRaiseArmState;
				yawState = lastYawState;
				extensionInput = 0f - extensionInput;
				yawInput = 0f - yawInput;
				raiseArmInput = 0f - raiseArmInput;
			}
			UpdateAnimator(UnityEngine.Time.deltaTime, false);
		}
		else
		{
			lastExtensionArmState = extensionArmState;
			lastRaiseArmState = raiseArmState;
			lastYawState = yawState;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			BasePlayer driver = GetDriver();
			if (driver != null && info.damageTypes.Has(DamageType.Bullet))
			{
				Capsule capsule = new Capsule(driverCollision.transform.position, driverCollision.radius, driverCollision.height);
				float num = Vector3.Distance(info.PointStart, info.PointEnd);
				Ray ray = new Ray(info.PointStart, Vector3Ex.Direction(info.PointEnd, info.PointStart));
				RaycastHit hit;
				if (capsule.Trace(ray, out hit, 0.05f, num * 1.2f))
				{
					driver.Hurt(info.damageTypes.Total() * 0.15f, DamageType.Bullet, info.Initiator);
				}
			}
		}
		base.OnAttacked(info);
	}

	public override void OnKilled(HitInfo info)
	{
		if (HasDriver())
		{
			GetDriver().Hurt(10000f, DamageType.Blunt, info.Initiator, false);
		}
		if (explosionEffect.isValid)
		{
			Effect.server.Run(explosionEffect.resourcePath, explosionPoint.position, Vector3.up);
		}
		base.OnKilled(info);
	}

	public bool DidCollide()
	{
		Transform[] array = collisionTestingPoints;
		foreach (Transform transform in array)
		{
			if (transform.gameObject.activeSelf)
			{
				Vector3 position = transform.position;
				Quaternion rotation = transform.rotation;
				if (GamePhysics.CheckOBB(new OBB(position, new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z), rotation), 1084293121, QueryTriggerInteraction.Ignore))
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetMaxDriveForce()
	{
		return (float)engineKW * 10f;
	}

	public float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float a = MathEx.BiasedLerp(1f - absSpeed / topSpeed, 0.5f);
		a = Mathf.Lerp(a, 1f, Mathf.Abs(steerInput));
		return GetMaxDriveForce() * a;
	}

	public CarWheel[] GetWheels()
	{
		return wheels;
	}

	public float GetWheelsMidPos()
	{
		return 0f;
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player))
		{
			GetFuelSystem().LootFuel(player);
		}
	}

	public override float GetThrottleInput()
	{
		if (base.isServer)
		{
			return throttleInput;
		}
		throw new NotImplementedException("We don't know magnet crane throttle input on the client.");
	}

	public override float GetBrakeInput()
	{
		if (base.isServer)
		{
			if (handbrakeOn)
			{
				return 1f;
			}
			return brakeInput;
		}
		throw new NotImplementedException("We don't know magnet crane brake input on the client.");
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.crane != null)
		{
			yawState = info.msg.crane.yaw;
			extensionArmState = info.msg.crane.arm1;
			raiseArmState = info.msg.crane.arm2;
		}
	}

	public void UpdateAnimator(float dt, bool shouldLerp = true)
	{
		float @float = animator.GetFloat("Arm_01");
		float float2 = animator.GetFloat("Arm_02");
		animator.SetFloat("Arm_01", shouldLerp ? Mathf.Lerp(@float, extensionArmState, dt * 6f) : extensionArmState);
		animator.SetFloat("Arm_02", shouldLerp ? Mathf.Lerp(float2, raiseArmState, dt * 6f) : raiseArmState);
		float num = Mathf.Lerp(testPreviousYaw, yawState, dt * 6f);
		if (num % 1f < 0f)
		{
			num += 1f;
		}
		if (yawState % 1f < 0f)
		{
			yawState += 1f;
		}
		animator.SetFloat("Yaw", (shouldLerp ? num : yawState) % 1f);
		testPreviousYaw = num;
	}

	public override float GetMaxForwardSpeed()
	{
		return 13f;
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (!PlayerIsMounted(player))
		{
			return !IsOn();
		}
		return true;
	}
}
