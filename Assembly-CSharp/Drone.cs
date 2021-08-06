using Network;
using UnityEngine;

public class Drone : RemoteControlEntity
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

	[Header("Drone")]
	public Rigidbody body;

	public bool killInWater = true;

	public bool enableGrounding = true;

	public bool keepAboveTerrain = true;

	public float groundTraceDist = 0.1f;

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

	[Header("Sound")]
	public SoundDefinition movementLoopSoundDef;

	public SoundDefinition movementStartSoundDef;

	public SoundDefinition movementStopSoundDef;

	public AnimationCurve movementLoopPitchCurve;

	public Vector3? targetPosition;

	public DroneInputState currentInput;

	public float lastInputTime;

	public double lastCollision = -1000.0;

	public bool isGrounded;

	public override bool RequiresMouse => true;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override void UserInput(InputState inputState, BasePlayer player)
	{
		currentInput.Reset();
		int num = (inputState.IsDown(BUTTON.FORWARD) ? 1 : 0) + (inputState.IsDown(BUTTON.BACKWARD) ? (-1) : 0);
		int num2 = (inputState.IsDown(BUTTON.RIGHT) ? 1 : 0) + (inputState.IsDown(BUTTON.LEFT) ? (-1) : 0);
		currentInput.movement = new Vector3(num2, 0f, num).normalized;
		currentInput.throttle = (inputState.IsDown(BUTTON.SPRINT) ? 1 : 0) + (inputState.IsDown(BUTTON.DUCK) ? (-1) : 0);
		currentInput.yaw = inputState.current.mouseDelta.x;
		currentInput.pitch = inputState.current.mouseDelta.y;
		lastInputTime = Time.time;
	}

	public virtual void Update()
	{
		if (base.IsBeingControlled || !targetPosition.HasValue)
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
		Vector3 direction;
		float magnitude;
		(vector - vector2).XZ3D().ToDirectionAndMagnitude(out direction, out magnitude);
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
		if (!base.isServer || IsDead() || (!base.IsBeingControlled && !targetPosition.HasValue))
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
		double currentTimestamp = TimeEx.currentTimestamp;
		bool num2 = lastCollision > 0.0 && currentTimestamp - lastCollision < (double)collisionDisableTime;
		RaycastHit hitInfo;
		isGrounded = enableGrounding && body.SweepTest(-base.transform.up, out hitInfo, groundTraceDist);
		Vector3 vector = base.transform.TransformDirection(currentInput.movement);
		Vector3 direction;
		float magnitude;
		body.velocity.WithY(0f).ToDirectionAndMagnitude(out direction, out magnitude);
		float num3 = Mathf.Clamp01(magnitude / leanMaxVelocity);
		Vector3 vector2 = (Mathf.Approximately(vector.sqrMagnitude, 0f) ? ((0f - num3) * direction) : vector);
		Vector3 normalized = (Vector3.up + vector2 * leanWeight * num3).normalized;
		Vector3 up = base.transform.up;
		float num4 = Mathf.Max(Vector3.Dot(normalized, up), 0f);
		if (!num2 || isGrounded)
		{
			Vector3 obj = ((isGrounded && currentInput.throttle <= 0f) ? Vector3.zero : (-1f * base.transform.up * Physics.gravity.y));
			Vector3 vector3 = (isGrounded ? Vector3.zero : (vector * movementAcceleration));
			Vector3 vector4 = base.transform.up * currentInput.throttle * altitudeAcceleration;
			Vector3 vector5 = obj + vector3 + vector4;
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
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
			float magnitude = collision.relativeVelocity.magnitude;
			if (magnitude > hurtVelocityThreshold)
			{
				Hurt(Mathf.Pow(magnitude, hurtDamagePower));
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

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}
}
