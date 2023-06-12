using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class Drone : RemoteControlEntity, IRemoteControllableClientCallbacks, IRemoteControllable
{
	public struct DroneInputState
	{
		public Vector3 movement;

		public float throttle;

		public float pitch;

		public float yaw;

		public void Reset()
		{
			movement = Vector3.zero;
			pitch = 0f;
			yaw = 0f;
		}
	}

	[ReplicatedVar(Help = "How far drones can be flown away from the controlling computer station", ShowInAdminUI = true, Default = "250")]
	public static float maxControlRange = 500f;

	[ServerVar(Help = "If greater than zero, overrides the drone's planar movement speed")]
	public static float movementSpeedOverride = 0f;

	[ServerVar(Help = "If greater than zero, overrides the drone's vertical movement speed")]
	public static float altitudeSpeedOverride = 0f;

	[ClientVar(ClientAdmin = true)]
	public static float windTimeDivisor = 10f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionDivisor = 100f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionScale = 1f;

	[ClientVar(ClientAdmin = true)]
	public static float windRotationMultiplier = 45f;

	[ClientVar(ClientAdmin = true)]
	public static float windLerpSpeed = 0.1f;

	public const Flags Flag_ThrottleUp = Flags.Reserved1;

	public const Flags Flag_Flying = Flags.Reserved2;

	[Header("Drone")]
	public Rigidbody body;

	public Transform modelRoot;

	public bool killInWater = true;

	public bool enableGrounding = true;

	public bool keepAboveTerrain = true;

	public float groundTraceDist = 0.1f;

	public float groundCheckInterval = 0.05f;

	public float altitudeAcceleration = 10f;

	public float movementAcceleration = 10f;

	public float yawSpeed = 2f;

	public float uprightSpeed = 2f;

	public float uprightPrediction = 0.15f;

	public float uprightDot = 0.5f;

	public float leanWeight = 0.1f;

	public float leanMaxVelocity = 5f;

	public float hurtVelocityThreshold = 3f;

	public float hurtDamagePower = 3f;

	public float collisionDisableTime = 0.25f;

	public float pitchMin = -60f;

	public float pitchMax = 60f;

	public float pitchSensitivity = -5f;

	public bool disableWhenHurt;

	[Range(0f, 1f)]
	public float disableWhenHurtChance = 0.25f;

	public float playerCheckInterval = 0.1f;

	public float playerCheckRadius;

	public float deployYOffset = 0.1f;

	[Header("Sound")]
	public SoundDefinition movementLoopSoundDef;

	public SoundDefinition movementStartSoundDef;

	public SoundDefinition movementStopSoundDef;

	public AnimationCurve movementLoopPitchCurve;

	public float movementSpeedReference = 50f;

	[Header("Animation")]
	public float propellerMaxSpeed = 1000f;

	public float propellerAcceleration = 3f;

	public Transform propellerA;

	public Transform propellerB;

	public Transform propellerC;

	public Transform propellerD;

	public float pitch;

	public Vector3? targetPosition;

	public DroneInputState currentInput;

	public float lastInputTime;

	public double lastCollision = -1000.0;

	public TimeSince lastGroundCheck;

	public bool isGrounded;

	public RealTimeSinceEx lastPlayerCheck;

	public override bool RequiresMouse => true;

	public override float MaxRange => maxControlRange;

	public override bool CanAcceptInput => true;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		isGrounded = true;
	}

	public override void StopControl(CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (viewerID == controllingViewerId)
		{
			SetFlag(Flags.Reserved1, b: false, recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved2, b: false, recursive: false, networkupdate: false);
			pitch = 0f;
			SendNetworkUpdate();
		}
		base.StopControl(viewerID);
	}

	public override void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (!(viewerID != controllingViewerId))
		{
			currentInput.Reset();
			int num = (inputState.IsDown(BUTTON.FORWARD) ? 1 : 0) + (inputState.IsDown(BUTTON.BACKWARD) ? (-1) : 0);
			int num2 = (inputState.IsDown(BUTTON.RIGHT) ? 1 : 0) + (inputState.IsDown(BUTTON.LEFT) ? (-1) : 0);
			currentInput.movement = new Vector3(num2, 0f, num).normalized;
			currentInput.throttle = (inputState.IsDown(BUTTON.SPRINT) ? 1 : 0) + (inputState.IsDown(BUTTON.DUCK) ? (-1) : 0);
			currentInput.yaw = inputState.current.mouseDelta.x;
			currentInput.pitch = inputState.current.mouseDelta.y;
			lastInputTime = Time.time;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = currentInput.throttle > 0f;
			if (flag3 != HasFlag(Flags.Reserved1))
			{
				SetFlag(Flags.Reserved1, flag3, recursive: false, networkupdate: false);
				flag = true;
			}
			float b = pitch;
			pitch += currentInput.pitch * pitchSensitivity;
			pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
			if (!Mathf.Approximately(pitch, b))
			{
				flag2 = true;
			}
			if (flag2)
			{
				SendNetworkUpdateImmediate();
			}
			else if (flag)
			{
				SendNetworkUpdate_Flags();
			}
		}
	}

	public virtual void Update_Server()
	{
		if (!base.isServer || IsDead() || base.IsBeingControlled || !targetPosition.HasValue)
		{
			return;
		}
		Vector3 position = base.transform.position;
		float height = TerrainMeta.HeightMap.GetHeight(position);
		Vector3 v = targetPosition.Value - body.velocity * 0.5f;
		if (keepAboveTerrain)
		{
			v.y = Mathf.Max(v.y, height + 1f);
		}
		Vector2 vector = v.XZ2D();
		Vector2 vector2 = position.XZ2D();
		(vector - vector2).XZ3D().ToDirectionAndMagnitude(out var direction, out var magnitude);
		currentInput.Reset();
		lastInputTime = Time.time;
		if (position.y - height > 1f)
		{
			float num = Mathf.Clamp01(magnitude);
			currentInput.movement = base.transform.InverseTransformVector(direction) * num;
			if (magnitude > 0.5f)
			{
				float y = base.transform.rotation.eulerAngles.y;
				float y2 = Quaternion.FromToRotation(Vector3.forward, direction).eulerAngles.y;
				currentInput.yaw = Mathf.Clamp(Mathf.LerpAngle(y, y2, Time.deltaTime) - y, -2f, 2f);
			}
		}
		currentInput.throttle = Mathf.Clamp(v.y - position.y, -1f, 1f);
	}

	public void FixedUpdate()
	{
		if (!base.isServer || IsDead())
		{
			return;
		}
		float num = WaterFactor();
		if (killInWater && num > 0f)
		{
			if (num > 0.99f)
			{
				Kill();
			}
			return;
		}
		if ((!base.IsBeingControlled && !targetPosition.HasValue) || (isGrounded && currentInput.throttle <= 0f))
		{
			if (HasFlag(Flags.Reserved2))
			{
				SetFlag(Flags.Reserved2, b: false, recursive: false, networkupdate: false);
				SendNetworkUpdate_Flags();
			}
			return;
		}
		if (playerCheckRadius > 0f && (double)lastPlayerCheck > (double)playerCheckInterval)
		{
			lastPlayerCheck = 0.0;
			List<BasePlayer> obj = Pool.GetList<BasePlayer>();
			Vis.Entities(base.transform.position, playerCheckRadius, obj, 131072);
			if (obj.Count > 0)
			{
				lastCollision = TimeEx.currentTimestamp;
			}
			Pool.FreeList(ref obj);
		}
		double currentTimestamp = TimeEx.currentTimestamp;
		bool num2 = lastCollision > 0.0 && currentTimestamp - lastCollision < (double)collisionDisableTime;
		if (enableGrounding)
		{
			if ((float)lastGroundCheck >= groundCheckInterval)
			{
				lastGroundCheck = 0f;
				RaycastHit hitInfo;
				bool flag = body.SweepTest(Vector3.down, out hitInfo, groundTraceDist, QueryTriggerInteraction.Ignore);
				if (!flag && isGrounded)
				{
					lastPlayerCheck = playerCheckInterval;
				}
				isGrounded = flag;
			}
		}
		else
		{
			isGrounded = false;
		}
		Vector3 vector = base.transform.TransformDirection(currentInput.movement);
		body.velocity.WithY(0f).ToDirectionAndMagnitude(out var direction, out var magnitude);
		float num3 = Mathf.Clamp01(magnitude / leanMaxVelocity);
		Vector3 vector2 = (Mathf.Approximately(vector.sqrMagnitude, 0f) ? ((0f - num3) * direction) : vector);
		Vector3 normalized = (Vector3.up + vector2 * leanWeight * num3).normalized;
		Vector3 up = base.transform.up;
		float num4 = Mathf.Max(Vector3.Dot(normalized, up), 0f);
		if (!num2 || isGrounded)
		{
			Vector3 obj2 = ((isGrounded && currentInput.throttle <= 0f) ? Vector3.zero : (-1f * base.transform.up * Physics.gravity.y));
			Vector3 vector3 = (isGrounded ? Vector3.zero : (vector * ((movementSpeedOverride > 0f) ? movementSpeedOverride : movementAcceleration)));
			Vector3 vector4 = base.transform.up * currentInput.throttle * ((altitudeSpeedOverride > 0f) ? altitudeSpeedOverride : altitudeAcceleration);
			Vector3 vector5 = obj2 + vector3 + vector4;
			body.AddForce(vector5 * num4, ForceMode.Acceleration);
		}
		if (!num2 && !isGrounded)
		{
			Vector3 vector6 = base.transform.TransformVector(0f, currentInput.yaw * yawSpeed, 0f);
			Vector3 vector7 = Vector3.Cross(Quaternion.Euler(body.angularVelocity * uprightPrediction) * up, normalized) * uprightSpeed;
			float num5 = ((num4 < uprightDot) ? 0f : num4);
			Vector3 vector8 = vector6 * num4 + vector7 * num5;
			body.AddTorque(vector8 * num4, ForceMode.Acceleration);
		}
		bool flag2 = !num2;
		if (flag2 != HasFlag(Flags.Reserved2))
		{
			SetFlag(Flags.Reserved2, flag2, recursive: false, networkupdate: false);
			SendNetworkUpdate_Flags();
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
			float magnitude = collision.relativeVelocity.magnitude;
			if (magnitude > hurtVelocityThreshold)
			{
				Hurt(Mathf.Pow(magnitude, hurtDamagePower), DamageType.Fall, null, useProtection: false);
			}
		}
	}

	public void OnCollisionStay()
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (base.isServer && disableWhenHurt && info.damageTypes.GetMajorityDamageType() != DamageType.Fall && Random.value < disableWhenHurtChance)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		if (body == null)
		{
			return Vector3.zero;
		}
		return body.velocity;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.drone = Pool.Get<ProtoBuf.Drone>();
			info.msg.drone.pitch = pitch;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.drone != null)
		{
			pitch = info.msg.drone.pitch;
		}
	}

	public virtual void Update()
	{
		Update_Server();
		if (HasFlag(Flags.Reserved2))
		{
			Vector3 eulerAngles = viewEyes.localRotation.eulerAngles;
			eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, pitch, 0.1f);
			viewEyes.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	public override bool CanChangeID(BasePlayer player)
	{
		if (player != null && base.OwnerID == player.userID)
		{
			return !HasFlag(Flags.Reserved2);
		}
		return false;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return !HasFlag(Flags.Reserved2);
		}
		return false;
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		if (player != null && player.userID == base.OwnerID)
		{
			createdItem.text = GetIdentifier();
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		base.transform.position += base.transform.up * deployYOffset;
		if (body != null)
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
		if (fromItem != null && !string.IsNullOrEmpty(fromItem.text) && ComputerStation.IsValidIdentifier(fromItem.text))
		{
			UpdateIdentifier(fromItem.text);
		}
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	public override float MaxVelocity()
	{
		return 30f;
	}
}
