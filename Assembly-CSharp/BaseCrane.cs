#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseCrane : BaseVehicle, TriggerHurtNotChild.IHurtTriggerUser
{
	public float extensionArmState;

	public float raiseArmState;

	public float yawState = 5f;

	public Transform COM;

	public float extensionDirection;

	public float yawDirection;

	public float raiseArmDirection;

	public float arm1Speed = 0.01f;

	public float arm2Speed = 0.01f;

	public float turnYawSpeed = 0.01f;

	public Animator animator;

	public BaseMagnet Magnet;

	public Rigidbody myRigidbody;

	public WheelCollider[] leftWheels;

	public WheelCollider[] rightWheels;

	public float brakeStrength = 1000f;

	public float engineStrength = 1000f;

	public Transform[] collisionTestingPoints;

	public float maxDistanceFromOrigin;

	public GameObjectRef selfDamageEffect;

	public GameObjectRef explosionEffect;

	public Transform explosionPoint;

	public CapsuleCollider driverCollision;

	public Transform leftHandTarget;

	public Transform rightHandTarget;

	[Header("Fuel")]
	public GameObjectRef fuelStoragePrefab;

	public float fuelPerSec;

	public EntityFuelSystem fuelSystem;

	public GameObject[] OnTriggers;

	public TriggerHurtEx magnetDamage;

	public static readonly Translate.Phrase ReturnMessage = new Translate.Phrase("junkyardcrane.return", "Return to the Junkyard. Excessive damage will occur.");

	private Vector3 spawnOrigin = Vector3.zero;

	public float nextInputTime;

	private float nextToggleTime;

	public float turnAmount;

	public float throttle;

	public float lastExtensionArmState;

	public float lastRaiseArmState;

	public float lastYawState;

	public bool handbrakeOn = true;

	private float nextSelfHealTime;

	public Vector3 lastDamagePos = Vector3.zero;

	public float lastDrivenTime;

	private float testPreviousYaw = 5f;

	public float GetPlayerDamageMultiplier()
	{
		return Mathf.Abs(GetLocalVelocity().magnitude) * 60f;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(UpdateParams, 0f, 0.1f);
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		myRigidbody.centerOfMass = COM.localPosition;
		SetMagnetEnabled(false);
		spawnOrigin = base.transform.position;
		lastDrivenTime = UnityEngine.Time.realtimeSinceStartup;
		GameObject[] onTriggers = OnTriggers;
		for (int i = 0; i < onTriggers.Length; i++)
		{
			onTriggers[i].SetActive(false);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned)
		{
			fuelSystem.CheckNewChild(child);
		}
	}

	public void SetMagnetEnabled(bool wantsOn)
	{
		Magnet.SetMagnetEnabled(wantsOn);
		SetFlag(Flags.Reserved6, wantsOn);
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (!EngineOn())
		{
			return;
		}
		bool num = inputState.IsDown(BUTTON.SPRINT);
		if (inputState.IsDown(BUTTON.RELOAD) && UnityEngine.Time.realtimeSinceStartup > nextToggleTime)
		{
			SetMagnetEnabled(!Magnet.IsMagnetOn());
			nextToggleTime = UnityEngine.Time.realtimeSinceStartup + 0.5f;
		}
		throttle = 0f;
		turnAmount = 0f;
		if (num)
		{
			if (inputState.IsDown(BUTTON.FORWARD))
			{
				throttle = 1f;
			}
			if (inputState.IsDown(BUTTON.BACKWARD))
			{
				throttle = -1f;
			}
			if (inputState.IsDown(BUTTON.RIGHT))
			{
				turnAmount = -1f;
			}
			if (inputState.IsDown(BUTTON.LEFT))
			{
				turnAmount = 1f;
			}
		}
		else if (UnityEngine.Time.realtimeSinceStartup >= nextInputTime)
		{
			if (inputState.IsDown(BUTTON.FIRE_PRIMARY))
			{
				extensionDirection = 1f;
			}
			if (inputState.IsDown(BUTTON.FIRE_SECONDARY))
			{
				extensionDirection = -1f;
			}
			if (inputState.IsDown(BUTTON.RIGHT))
			{
				yawDirection = -1f;
			}
			if (inputState.IsDown(BUTTON.LEFT))
			{
				yawDirection = 1f;
			}
			if (inputState.IsDown(BUTTON.FORWARD))
			{
				raiseArmDirection = 1f;
			}
			if (inputState.IsDown(BUTTON.BACKWARD))
			{
				raiseArmDirection = -1f;
			}
		}
		handbrakeOn = throttle == 0f && turnAmount == 0f;
	}

	public bool EngineOn()
	{
		return IsOn();
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		bool flag = EngineOn();
		if (EngineOn())
		{
			fuelSystem.TryUseFuel(UnityEngine.Time.fixedDeltaTime, fuelPerSec);
		}
		SetFlag(Flags.On, HasDriver() && GetFuelSystem().HasFuel());
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
		if (UnityEngine.Time.realtimeSinceStartup > lastDrivenTime + 14400f)
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
					nextSelfHealTime = UnityEngine.Time.realtimeSinceStartup + 3600f;
					Effect.server.Run(selfDamageEffect.resourcePath, base.transform.position + Vector3.up * 2f, Vector3.up);
					return;
				}
			}
			else if (base.healthFraction < 1f && UnityEngine.Time.realtimeSinceStartup > nextSelfHealTime && base.SecondsSinceAttacked > 600f)
			{
				Heal(1000f);
			}
		}
		if (!HasDriver() || !EngineOn())
		{
			handbrakeOn = true;
			throttle = 0f;
			turnAmount = 0f;
			SetFlag(Flags.Reserved10, false);
			SetFlag(Flags.Reserved5, false);
			SetMagnetEnabled(false);
		}
		else
		{
			lastDrivenTime = UnityEngine.Time.realtimeSinceStartup;
			if (Magnet.IsMagnetOn() && Magnet.HasConnectedObject() && GamePhysics.CheckOBB(Magnet.GetConnectedOBB(0.75f), 1084293121, QueryTriggerInteraction.Ignore))
			{
				SetMagnetEnabled(false);
				nextToggleTime = UnityEngine.Time.realtimeSinceStartup + 2f;
				Effect.server.Run(selfDamageEffect.resourcePath, Magnet.transform.position, Vector3.up);
			}
		}
		extensionDirection = Mathf.MoveTowards(extensionDirection, 0f, UnityEngine.Time.fixedDeltaTime * 3f);
		yawDirection = Mathf.MoveTowards(yawDirection, 0f, UnityEngine.Time.fixedDeltaTime * 3f);
		raiseArmDirection = Mathf.MoveTowards(raiseArmDirection, 0f, UnityEngine.Time.fixedDeltaTime * 3f);
		bool flag2 = extensionDirection != 0f || raiseArmDirection != 0f || yawDirection != 0f;
		SetFlag(Flags.Reserved7, flag2);
		magnetDamage.damageEnabled = IsOn() && flag2;
		extensionArmState += extensionDirection * arm1Speed * UnityEngine.Time.fixedDeltaTime;
		raiseArmState += raiseArmDirection * arm2Speed * UnityEngine.Time.fixedDeltaTime;
		yawState += yawDirection * turnYawSpeed * UnityEngine.Time.fixedDeltaTime;
		extensionArmState = Mathf.Clamp(extensionArmState, -1f, 1f);
		raiseArmState = Mathf.Clamp(raiseArmState, -1f, 1f);
		UpdateAnimator(false);
		Magnet.MagnetThink(UnityEngine.Time.fixedDeltaTime);
		float num = throttle;
		float num2 = throttle;
		if (turnAmount == 1f)
		{
			num = -1f;
			num2 = 1f;
		}
		else if (turnAmount == -1f)
		{
			num = 1f;
			num2 = -1f;
		}
		UpdateMotorSpeed(num * engineStrength, num2 * engineStrength, handbrakeOn ? brakeStrength : 0f);
	}

	public void UpdateMotorSpeed(float speedLeft, float speedRight, float brakeSpeed)
	{
		WheelCollider[] array = leftWheels;
		foreach (WheelCollider obj in array)
		{
			obj.motorTorque = speedLeft;
			obj.brakeTorque = brakeSpeed;
		}
		array = rightWheels;
		foreach (WheelCollider obj2 in array)
		{
			obj2.motorTorque = speedRight;
			obj2.brakeTorque = brakeSpeed;
		}
		SetFlag(Flags.Reserved10, speedLeft != 0f && speedRight != 0f);
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
				extensionDirection = 0f - extensionDirection;
				yawDirection = 0f - yawDirection;
				raiseArmDirection = 0f - raiseArmDirection;
			}
			UpdateAnimator(false);
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

	public override void LightToggle(BasePlayer player)
	{
		SetFlag(Flags.Reserved5, !HasFlag(Flags.Reserved5));
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

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && (!HasDriver() || IsDriver(player)))
		{
			fuelSystem.LootFuel(player);
		}
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return fuelSystem;
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

	public void UpdateAnimator(bool shouldLerp = true)
	{
		float @float = animator.GetFloat("Arm_01");
		float float2 = animator.GetFloat("Arm_02");
		float testPreviousYaw2 = testPreviousYaw;
		animator.SetFloat("Arm_01", shouldLerp ? Mathf.Lerp(@float, extensionArmState, UnityEngine.Time.deltaTime * 6f) : extensionArmState);
		animator.SetFloat("Arm_02", shouldLerp ? Mathf.Lerp(float2, raiseArmState, UnityEngine.Time.deltaTime * 6f) : raiseArmState);
		float num = Mathf.Lerp(testPreviousYaw, yawState, UnityEngine.Time.deltaTime * 6f);
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

	public override void InitShared()
	{
		base.InitShared();
		fuelSystem = new EntityFuelSystem(base.isServer, fuelStoragePrefab, children);
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseCrane.OnRpcMessage"))
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
}
