using Network;
using UnityEngine;

public class WaterInflatable : BaseMountable, IPoolVehicle, INotifyTrigger
{
	private enum PaddleDirection
	{
		Forward = 0,
		Left = 1,
		Right = 2,
		Back = 3
	}

	public Transform centerOfMass;

	public float forwardPushForce = 5f;

	public float rearPushForce = 5f;

	public float rotationForce = 5f;

	public float maxSpeed = 3f;

	public float maxPaddleFrequency = 0.5f;

	public SoundDefinition paddleSfx;

	public SoundDefinition smallPlayerMovementSound;

	public SoundDefinition largePlayerMovementSound;

	public BlendedSoundLoops waterLoops;

	public float waterSoundSpeedDivisor = 1f;

	public float additiveDownhillVelocity;

	public GameObjectRef handSplashForwardEffect;

	public GameObjectRef handSplashBackEffect;

	public GameObjectRef footSplashEffect;

	public float animationLerpSpeed = 1f;

	public Transform smoothedEyePosition;

	public float smoothedEyeSpeed = 1f;

	public Buoyancy buoyancy;

	public bool driftTowardsIsland;

	public GameObjectRef mountEffect;

	[Range(0f, 1f)]
	public float handSplashOffset = 1f;

	public float velocitySplashMultiplier = 4f;

	public Vector3 modifyEyeOffset = Vector3.zero;

	[Range(0f, 1f)]
	public float inheritVelocityMultiplier;

	private TimeSince lastPaddle;

	public ParticleSystem[] movingParticleSystems;

	public float movingParticlesThreshold = 0.0005f;

	public Transform headSpaceCheckPosition;

	public float headSpaceCheckRadius = 0.4f;

	private TimeSince landFacingCheck;

	private bool isFacingLand;

	private float landPushAcceleration;

	private TimeSince inPoolCheck;

	private bool isInPool;

	private Vector3 lastPos = Vector3.zero;

	private Vector3 lastClipCheckPosition;

	private bool forceClippingCheck;

	private bool prevSleeping;

	public override bool IsSummerDlcVehicle => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WaterInflatable.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		rigidBody.centerOfMass = centerOfMass.localPosition;
		prevSleeping = false;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (deployedBy != null)
		{
			Vector3 estimatedVelocity = deployedBy.estimatedVelocity;
			float value = Vector3.Dot(base.transform.forward, estimatedVelocity.normalized);
			Vector3 force = Vector3.Lerp(Vector3.zero, estimatedVelocity, Mathf.Clamp(value, 0f, 1f));
			force *= inheritVelocityMultiplier;
			rigidBody.AddForce(force, ForceMode.VelocityChange);
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		bool flag = rigidBody.IsSleeping();
		if (prevSleeping && !flag && buoyancy != null)
		{
			buoyancy.Wake();
		}
		prevSleeping = flag;
		if (rigidBody.velocity.magnitude > maxSpeed)
		{
			rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);
		}
		if (!AnyMounted() || !(headSpaceCheckPosition != null))
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (!forceClippingCheck && !(Vector3.Distance(position, lastClipCheckPosition) > headSpaceCheckRadius * 0.5f))
		{
			return;
		}
		forceClippingCheck = false;
		if (GamePhysics.CheckSphere(headSpaceCheckPosition.position, headSpaceCheckRadius, 1218511105, QueryTriggerInteraction.Ignore))
		{
			if (!GetDismountPosition(GetMounted(), out var _))
			{
				base.transform.position = lastClipCheckPosition;
			}
			DismountAllPlayers();
		}
		lastClipCheckPosition = position;
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		lastPos = base.transform.position;
		forceClippingCheck = true;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (Vector3.Dot(base.transform.up, Vector3.up) < 0.1f)
		{
			DismountAllPlayers();
		}
		else
		{
			if ((float)lastPaddle < maxPaddleFrequency || (buoyancy != null && IsOutOfWaterServer()))
			{
				return;
			}
			if (player.GetHeldEntity() == null)
			{
				if (inputState.IsDown(BUTTON.FORWARD))
				{
					if (rigidBody.velocity.magnitude < maxSpeed)
					{
						rigidBody.AddForce(base.transform.forward * forwardPushForce, ForceMode.Impulse);
					}
					rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, base.transform.forward, 0.5f);
					lastPaddle = 0f;
					ClientRPC(null, "OnPaddled", 0);
				}
				if (inputState.IsDown(BUTTON.BACKWARD))
				{
					rigidBody.AddForce(-base.transform.forward * rearPushForce, ForceMode.Impulse);
					rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, -base.transform.forward, 0.5f);
					lastPaddle = 0f;
					ClientRPC(null, "OnPaddled", 3);
				}
				if (inputState.IsDown(BUTTON.LEFT))
				{
					PaddleTurn(PaddleDirection.Left);
				}
				if (inputState.IsDown(BUTTON.RIGHT))
				{
					PaddleTurn(PaddleDirection.Right);
				}
			}
			if ((float)inPoolCheck > 2f)
			{
				isInPool = IsInWaterVolume(base.transform.position);
				inPoolCheck = 0f;
			}
			if (additiveDownhillVelocity > 0f && !isInPool)
			{
				Vector3 vector = base.transform.TransformPoint(Vector3.forward);
				Vector3 position = base.transform.position;
				if (vector.y < position.y)
				{
					float num = additiveDownhillVelocity * (position.y - vector.y);
					rigidBody.AddForce(num * Time.fixedDeltaTime * base.transform.forward, ForceMode.Acceleration);
				}
				Vector3 velocity = rigidBody.velocity;
				rigidBody.velocity = Vector3.Lerp(velocity, base.transform.forward * velocity.magnitude, 0.4f);
			}
			if (driftTowardsIsland && (float)landFacingCheck > 2f && !isInPool)
			{
				isFacingLand = false;
				landFacingCheck = 0f;
				Vector3 position2 = base.transform.position;
				if (!WaterResource.IsFreshWater(position2))
				{
					int num2 = 5;
					Vector3 forward = base.transform.forward;
					forward.y = 0f;
					for (int i = 1; i <= num2; i++)
					{
						int mask = 128;
						if (!TerrainMeta.TopologyMap.GetTopology(position2 + (float)i * 15f * forward, mask))
						{
							isFacingLand = true;
							break;
						}
					}
				}
			}
			if (driftTowardsIsland && isFacingLand && !isInPool)
			{
				landPushAcceleration = Mathf.Clamp(landPushAcceleration + Time.deltaTime, 0f, 3f);
				rigidBody.AddForce(base.transform.forward * (Time.deltaTime * landPushAcceleration), ForceMode.VelocityChange);
			}
			else
			{
				landPushAcceleration = 0f;
			}
			lastPos = base.transform.position;
		}
	}

	private void PaddleTurn(PaddleDirection direction)
	{
		if (direction != 0 && direction != PaddleDirection.Back)
		{
			rigidBody.AddRelativeTorque(rotationForce * ((direction == PaddleDirection.Left) ? (-Vector3.up) : Vector3.up), ForceMode.Impulse);
			lastPaddle = 0f;
			ClientRPC(null, "OnPaddled", (int)direction);
		}
	}

	public override float WaterFactorForPlayer(BasePlayer player)
	{
		return 0f;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (hitEntity is BaseVehicle baseVehicle && (baseVehicle.HasDriver() || baseVehicle.IsMoving() || baseVehicle.HasFlag(Flags.On)))
		{
			Kill(DestroyMode.Gib);
		}
	}

	private bool IsOutOfWaterServer()
	{
		return buoyancy.timeOutOfWater > 0.2f;
	}

	public void OnPoolDestroyed()
	{
		Kill(DestroyMode.Gib);
	}

	public void WakeUp()
	{
		if (rigidBody != null)
		{
			rigidBody.WakeUp();
			rigidBody.AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
		}
		if (buoyancy != null)
		{
			buoyancy.Wake();
		}
	}

	public void OnObjects(TriggerNotify trigger)
	{
		if (base.isClient)
		{
			return;
		}
		foreach (BaseEntity entityContent in trigger.entityContents)
		{
			if (entityContent is BaseVehicle baseVehicle && (baseVehicle.HasDriver() || baseVehicle.IsMoving() || baseVehicle.HasFlag(Flags.On)))
			{
				Kill(DestroyMode.Gib);
				break;
			}
		}
	}

	public void OnEmpty()
	{
	}
}
