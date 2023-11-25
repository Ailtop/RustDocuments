using Facepunch.Rust;
using Rust;
using UnityEngine;

public class Parachute : BaseVehicle, SamSite.ISamSiteTarget
{
	public Collider ParachuteCollider;

	public ItemDefinition PackedParachute;

	public GameObjectRef DetachedParachute;

	public Transform DetachedSpawnPoint;

	public float ConditionLossPerUse = 0.2f;

	public float HurtDeployTime = 1f;

	public float HurtAmount = 80f;

	public Animator ColliderAnimator;

	public Animator ColliderWorldAnimator;

	public float UprightLerpForce = 5f;

	public float ConstantForwardForce = 2f;

	public ForceMode ForwardForceMode = ForceMode.Acceleration;

	public float TurnForce = 2f;

	public ForceMode TurnForceMode = ForceMode.Acceleration;

	public float ForwardTiltAcceleration = 2f;

	public float BackInputForceMultiplier = 0.2f;

	public float DeployAnimationLength = 3f;

	public float TargetDrag = 1f;

	public float TargetAngularDrag = 1f;

	public AnimationCurve DragCurve = new AnimationCurve();

	public AnimationCurve DragDamageCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve MassDamageCurve = AnimationCurve.Linear(0f, 30f, 1f, 1f);

	public AnimationCurve DamageHorizontalVelocityCurve = AnimationCurve.Linear(0f, 5f, 1f, 20f);

	[Range(0f, 1f)]
	public float DamageTester = 1f;

	public float AnimationInputSmoothness = 1f;

	public Vector2 AnimationInputScale = new Vector2(0.5f, 0.5f);

	public ParachuteWearable FirstPersonCanopy;

	public GameObjectRef ParachuteLandScreenBounce;

	private static int AnimatorInputXParameter = Animator.StringToHash("InputX");

	private static int AnimatorInputYParameter = Animator.StringToHash("InputY");

	private TimeSince mountTime;

	public const Flags Flag_InputForward = Flags.Reserved1;

	public const Flags Flag_InputBack = Flags.Reserved2;

	public const Flags Flag_InputLeft = Flags.Reserved3;

	public const Flags Flag_InputRight = Flags.Reserved4;

	public SoundDefinition deploySoundDef;

	public SoundDefinition releaseSoundDef;

	public SoundDefinition flightLoopSoundDef;

	public SoundDefinition steerSoundDef;

	public AnimationCurve flightLoopPitchCurve;

	public AnimationCurve flightLoopGainCurve;

	[ServerVar(Saved = true)]
	public static bool BypassRepack = false;

	[ServerVar(Saved = true)]
	public static bool LandingAnimations = false;

	private bool collisionDeath;

	private Vector3 collisionImpulse = Vector3.zero;

	private float startHeight;

	private float distanceTravelled;

	private Vector3 lastPosition = Vector3.zero;

	private Vector2 lerpedInput = Vector2.zero;

	private Vector3 collisionLocalPos;

	private Vector3 collisionWorldNormal;

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeVehicle;

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		rigidBody.velocity = player.estimatedVelocity;
		mountTime = 0f;
		startHeight = base.transform.position.y;
		distanceTravelled = 0f;
		canTriggerParent = false;
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res)
	{
		ParachuteCollider.enabled = false;
		bool dismountPosition = base.GetDismountPosition(player, out res);
		ParachuteCollider.enabled = true;
		return dismountPosition;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		player.PlayHeavyLandingAnimation = true;
		Vector3 position = base.transform.position;
		float num = Vector3.Distance(lastPosition, position);
		distanceTravelled += num;
		lastPosition = position;
		if (WaterLevel.Test(base.transform.position, waves: true, volumes: true, this))
		{
			DismountAllPlayers();
		}
		else if (!((float)mountTime < DeployAnimationLength))
		{
			Vector2 b = ProcessInputVector(inputState, player);
			lerpedInput = Vector2.Lerp(lerpedInput, b, Time.deltaTime * 5f);
			ColliderAnimator.SetFloat(AnimatorInputXParameter, lerpedInput.x);
			ColliderAnimator.SetFloat(AnimatorInputYParameter, lerpedInput.y);
			SetFlag(Flags.Reserved1, inputState.IsDown(BUTTON.FORWARD), recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved2, inputState.IsDown(BUTTON.BACKWARD), recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved3, inputState.IsDown(BUTTON.LEFT), recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved4, inputState.IsDown(BUTTON.RIGHT));
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float num = base.healthFraction * DamageTester;
		float t = DragCurve.Evaluate(mountTime);
		float num2 = DragDamageCurve.Evaluate(num);
		float mass = MassDamageCurve.Evaluate(num);
		rigidBody.mass = mass;
		rigidBody.drag = Mathf.Lerp(0f, TargetDrag * num2, t);
		rigidBody.angularDrag = Mathf.Lerp(0f, TargetAngularDrag * num2, t);
		float num3 = Mathf.Clamp01((float)mountTime / 1f);
		Vector3 forward = base.transform.forward;
		Vector3 force = (forward * ConstantForwardForce + forward * (ForwardTiltAcceleration * Mathf.Clamp(lerpedInput.y, 0f, 1f))) * Time.fixedDeltaTime * num3;
		if (lerpedInput.y < -0.1f)
		{
			force *= 1f - BackInputForceMultiplier * Mathf.Abs(lerpedInput.y);
		}
		force *= num;
		rigidBody.AddForce(force, ForwardForceMode);
		if (lerpedInput.x != 0f)
		{
			Quaternion b = Quaternion.Euler(rigidBody.rotation.eulerAngles.WithZ(Mathx.RemapValClamped(lerpedInput.x, -1f, 1f, 40f, -40f)));
			rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, b, Time.fixedDeltaTime * 30f));
			rigidBody.AddTorque(base.transform.TransformDirection(Vector3.up * (TurnForce * num * 0.2f * lerpedInput.x)), TurnForceMode);
		}
		if (lerpedInput.y > 0f)
		{
			Quaternion b2 = Quaternion.Euler(rigidBody.rotation.eulerAngles.WithX(Mathx.RemapValClamped(lerpedInput.y, -1f, 1f, -50f, 60f)));
			rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, b2, Time.fixedDeltaTime * 60f));
		}
		Quaternion b3 = Quaternion.Euler(rigidBody.rotation.eulerAngles.WithX(0f).WithZ(0f));
		rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, b3, Time.fixedDeltaTime * UprightLerpForce);
		float num4 = DamageHorizontalVelocityCurve.Evaluate(num);
		Vector3 velocity = rigidBody.velocity;
		velocity.x = Mathf.Clamp(velocity.x, 0f - num4, num4);
		velocity.z = Mathf.Clamp(velocity.z, 0f - num4, num4);
		rigidBody.velocity = velocity;
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		if (collisionDeath)
		{
			if ((float)mountTime < HurtDeployTime)
			{
				float num = 1f - Mathf.Clamp01((float)mountTime / HurtDeployTime);
				player.Hurt(HurtAmount * num, DamageType.Fall);
			}
			else
			{
				float magnitude = collisionImpulse.magnitude;
				if (magnitude > 50f)
				{
					float amount = Mathx.RemapValClamped(magnitude, 50f, 400f, 5f, 50f);
					player.Hurt(amount, DamageType.Fall);
				}
			}
		}
		if (BypassRepack)
		{
			Item item = ItemManager.Create(PackedParachute, 1, skinID);
			item.RepairCondition(item.maxCondition);
			player.inventory.containerWear.GiveItem(item);
		}
		Analytics.Azure.OnParachuteUsed(player, distanceTravelled, startHeight, mountTime);
		if (collisionDeath && LandingAnimations)
		{
			Effect.server.Run(ParachuteLandScreenBounce.resourcePath, player, 0u, Vector3.zero, Vector3.zero);
			if (collisionLocalPos.y < 0.15f)
			{
				player.Server_StartGesture(GestureCollection.HeavyLandingId);
				player.PlayHeavyLandingAnimation = false;
			}
		}
		ProcessDeath();
		collisionDeath = false;
	}

	private void ProcessDeath()
	{
		float num = base.healthFraction;
		num -= ConditionLossPerUse;
		bool num2 = num > 0f;
		if (num2 && !BypassRepack)
		{
			ParachuteUnpacked parachuteUnpacked = GameManager.server.CreateEntity(DetachedParachute.resourcePath, DetachedSpawnPoint.position, DetachedSpawnPoint.rotation) as ParachuteUnpacked;
			if (parachuteUnpacked != null)
			{
				parachuteUnpacked.skinID = skinID;
				parachuteUnpacked.Spawn();
				parachuteUnpacked.Hurt(parachuteUnpacked.MaxHealth() * (1f - num), DamageType.Generic, null, useProtection: false);
				if (parachuteUnpacked.TryGetComponent<Rigidbody>(out var component))
				{
					component.velocity = rigidBody.velocity;
				}
			}
		}
		DestroyMode mode = DestroyMode.None;
		if (!num2)
		{
			mode = DestroyMode.Gib;
		}
		Kill(mode);
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (hitEntity == null)
		{
			hitEntity = GameObjectEx.ToBaseEntity(collision.collider);
		}
		if (!(hitEntity == this) && (!(hitEntity != null) || hitEntity.isServer == base.isServer) && base.isServer && !(hitEntity is TimedExplosive) && !collisionDeath)
		{
			collisionImpulse = collision.impulse;
			collisionLocalPos = base.transform.InverseTransformPoint(collision.GetContact(0).point);
			collisionWorldNormal = collision.GetContact(0).normal;
			collisionDeath = true;
			Invoke(DelayedDismount, 0f);
		}
	}

	private void DelayedDismount()
	{
		if (collisionDeath && distanceTravelled > 0f && (!(mountPoints[0].mountable != null) || !GetDismountPosition(mountPoints[0].mountable.GetMounted(), out var _)))
		{
			base.transform.position += collisionWorldNormal * 0.35f;
		}
		DismountAllPlayers();
	}

	public override float MaxVelocity()
	{
		return 13.5f;
	}

	public override bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		if ((float)mountTime < 1.5f)
		{
			return false;
		}
		return base.AllowPlayerInstigatedDismount(player);
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		if ((float)mountTime > 1f)
		{
			return !InSafeZone();
		}
		return false;
	}

	private Vector2 ProcessInputVector(InputState inputState, BasePlayer player)
	{
		if (player.GetHeldEntity() != null)
		{
			return Vector2.zero;
		}
		bool leftDown = inputState.IsDown(BUTTON.LEFT);
		bool rightDown = inputState.IsDown(BUTTON.RIGHT);
		bool forwardDown = inputState.IsDown(BUTTON.FORWARD);
		bool backDown = inputState.IsDown(BUTTON.BACKWARD);
		return ProcessInputVector(leftDown, rightDown, forwardDown, backDown);
	}

	private Vector2 ProcessInputVectorFromFlags(BasePlayer player)
	{
		if (player.GetHeldEntity() != null)
		{
			return Vector2.zero;
		}
		bool leftDown = HasFlag(Flags.Reserved3);
		bool rightDown = HasFlag(Flags.Reserved4);
		bool forwardDown = HasFlag(Flags.Reserved1);
		bool backDown = HasFlag(Flags.Reserved2);
		return ProcessInputVector(leftDown, rightDown, forwardDown, backDown);
	}

	private static Vector2 ProcessInputVector(bool leftDown, bool rightDown, bool forwardDown, bool backDown)
	{
		Vector2 zero = Vector2.zero;
		if (leftDown && rightDown)
		{
			leftDown = (rightDown = false);
		}
		if (forwardDown && backDown)
		{
			forwardDown = (backDown = false);
		}
		if (forwardDown)
		{
			zero.y = 1f;
		}
		else if (backDown)
		{
			zero.y = -1f;
		}
		if (rightDown)
		{
			zero.x = 1f;
		}
		else if (leftDown)
		{
			zero.x = -1f;
		}
		return zero;
	}
}
