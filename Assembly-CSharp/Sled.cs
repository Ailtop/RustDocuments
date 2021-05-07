using UnityEngine;

public class Sled : BaseVehicle, INotifyTrigger
{
	private const Flags BrakeOn = Flags.Reserved1;

	private const Flags OnSnow = Flags.Reserved2;

	private const Flags IsGrounded = Flags.Reserved3;

	private const Flags OnSand = Flags.Reserved4;

	public PhysicMaterial BrakeMaterial;

	public PhysicMaterial SnowMaterial;

	public PhysicMaterial NonSnowMaterial;

	public Transform CentreOfMassTransform;

	public Collider[] PhysicsMaterialTargets;

	public float InitialForceCutoff = 3f;

	public float InitialForceIncreaseRate = 0.05f;

	public float TurnForce = 1f;

	public float DirectionMatchForce = 1f;

	public float VerticalAdjustmentForce = 1f;

	public float VerticalAdjustmentAngleThreshold = 15f;

	public float NudgeCooldown = 3f;

	public float NudgeForce = 2f;

	public float MaxNudgeVelocity = 2f;

	public const float DecayFrequency = 60f;

	public float DecayAmount = 10f;

	public ParticleSystemContainer TrailEffects;

	public SoundDefinition enterSnowSoundDef;

	public SoundDefinition snowSlideLoopSoundDef;

	public SoundDefinition dirtSlideLoopSoundDef;

	public AnimationCurve movementLoopGainCurve;

	public AnimationCurve movementLoopPitchCurve;

	private VehicleTerrainHandler terrainHandler;

	private PhysicMaterial cachedMaterial;

	private float initialForceScale;

	private TimeSince leftIce;

	private TimeSince lastNudge;

	public override bool BlocksDoors => false;

	public override void ServerInit()
	{
		base.ServerInit();
		terrainHandler = new VehicleTerrainHandler(this);
		terrainHandler.RayLength = 0.6f;
		rigidBody.centerOfMass = CentreOfMassTransform.localPosition;
		InvokeRandomized(DecayOverTime, Random.Range(30f, 60f), 60f, 6f);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy)
	{
		base.OnDeployed(parent, deployedBy);
		SetFlag(Flags.Reserved1, true);
		UpdateGroundedFlag();
		UpdatePhysicsMaterial();
	}

	protected override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (!HasAnyPassengers())
		{
			return;
		}
		terrainHandler.FixedUpdate();
		if (!terrainHandler.IsGrounded)
		{
			Quaternion b = Quaternion.FromToRotation(base.transform.up, Vector3.up) * rigidBody.rotation;
			if (Quaternion.Angle(rigidBody.rotation, b) > VerticalAdjustmentAngleThreshold)
			{
				rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, b, Time.fixedDeltaTime * VerticalAdjustmentForce));
			}
		}
	}

	private void UpdatePhysicsMaterial()
	{
		cachedMaterial = GetPhysicMaterial();
		Collider[] physicsMaterialTargets = PhysicsMaterialTargets;
		for (int i = 0; i < physicsMaterialTargets.Length; i++)
		{
			physicsMaterialTargets[i].sharedMaterial = cachedMaterial;
		}
		if (!AnyMounted() && rigidBody.IsSleeping())
		{
			CancelInvoke(UpdatePhysicsMaterial);
		}
		SetFlag(Flags.Reserved2, terrainHandler.IsOnSnowOrIce);
		SetFlag(Flags.Reserved4, terrainHandler.OnSurface == VehicleTerrainHandler.Surface.Sand);
	}

	private void UpdateGroundedFlag()
	{
		if (!AnyMounted() && rigidBody.IsSleeping())
		{
			CancelInvoke(UpdateGroundedFlag);
		}
		SetFlag(Flags.Reserved3, terrainHandler.IsGrounded);
	}

	private PhysicMaterial GetPhysicMaterial()
	{
		if (HasFlag(Flags.Reserved1) || !HasAnyPassengers())
		{
			return BrakeMaterial;
		}
		bool flag = terrainHandler.IsOnSnowOrIce || terrainHandler.OnSurface == VehicleTerrainHandler.Surface.Sand;
		if (flag)
		{
			leftIce = 0f;
		}
		else if ((float)leftIce < 2f)
		{
			flag = true;
		}
		if (!flag)
		{
			return NonSnowMaterial;
		}
		return SnowMaterial;
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		if (HasFlag(Flags.Reserved1))
		{
			initialForceScale = 0f;
			InvokeRepeating(ApplyInitialForce, 0f, 0.1f);
			SetFlag(Flags.Reserved1, false);
		}
		if (!IsInvoking(UpdatePhysicsMaterial))
		{
			InvokeRepeating(UpdatePhysicsMaterial, 0f, 0.5f);
		}
		if (!IsInvoking(UpdateGroundedFlag))
		{
			InvokeRepeating(UpdateGroundedFlag, 0f, 0.1f);
		}
		if (rigidBody.IsSleeping())
		{
			rigidBody.WakeUp();
		}
	}

	private void ApplyInitialForce()
	{
		Vector3 forward = base.transform.forward;
		Vector3 a = ((Vector3.Dot(forward, -Vector3.up) > Vector3.Dot(-forward, -Vector3.up)) ? forward : (-forward));
		rigidBody.AddForce(a * initialForceScale * (terrainHandler.IsOnSnowOrIce ? 1f : 0.25f), ForceMode.Acceleration);
		initialForceScale += InitialForceIncreaseRate;
		if (initialForceScale >= InitialForceCutoff && (rigidBody.velocity.magnitude > 1f || !terrainHandler.IsOnSnowOrIce))
		{
			CancelInvoke(ApplyInitialForce);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (Vector3.Dot(base.transform.up, Vector3.up) < 0.1f || WaterFactor() > 0.25f)
		{
			DismountAllPlayers();
			return;
		}
		float num = (inputState.IsDown(BUTTON.LEFT) ? (-1f) : 0f);
		num += (inputState.IsDown(BUTTON.RIGHT) ? 1f : 0f);
		if (inputState.IsDown(BUTTON.FORWARD) && (float)lastNudge > NudgeCooldown && rigidBody.velocity.magnitude < MaxNudgeVelocity)
		{
			rigidBody.WakeUp();
			rigidBody.AddForce(base.transform.forward * NudgeForce, ForceMode.Impulse);
			rigidBody.AddForce(base.transform.up * NudgeForce * 0.5f, ForceMode.Impulse);
			lastNudge = 0f;
		}
		num *= TurnForce;
		Vector3 velocity = rigidBody.velocity;
		if (num != 0f)
		{
			base.transform.Rotate(Vector3.up * num * Time.deltaTime * velocity.magnitude, Space.Self);
		}
		if (terrainHandler.IsGrounded && Vector3.Dot(rigidBody.velocity.normalized, base.transform.forward) >= 0.5f)
		{
			rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, base.transform.forward * velocity.magnitude, Time.deltaTime * DirectionMatchForce);
		}
	}

	private void DecayOverTime()
	{
		if (!HasAnyPassengers())
		{
			Hurt(DecayAmount);
		}
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return !player.isMounted;
		}
		return false;
	}

	public void OnObjects(TriggerNotify trigger)
	{
		foreach (BaseEntity entityContent in trigger.entityContents)
		{
			if (!(entityContent is Sled))
			{
				BaseVehicleModule baseVehicleModule;
				if ((object)(baseVehicleModule = entityContent as BaseVehicleModule) != null && baseVehicleModule.Vehicle != null && (baseVehicleModule.Vehicle.IsOn() || !baseVehicleModule.Vehicle.IsStationary()))
				{
					Kill(DestroyMode.Gib);
					break;
				}
				BaseVehicle baseVehicle;
				if ((object)(baseVehicle = entityContent as BaseVehicle) != null && baseVehicle.HasDriver() && (baseVehicle.IsMoving() || baseVehicle.HasFlag(Flags.On)))
				{
					Kill(DestroyMode.Gib);
					break;
				}
			}
		}
	}

	public void OnEmpty()
	{
	}
}
