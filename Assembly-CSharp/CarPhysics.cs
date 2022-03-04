using UnityEngine;

public class CarPhysics<TCar> where TCar : BaseVehicle, CarPhysics<TCar>.ICar
{
	public interface ICar
	{
		VehicleTerrainHandler.Surface OnSurface { get; }

		float GetThrottleInput();

		float GetBrakeInput();

		float GetSteerInput();

		bool GetSteerModInput();

		float GetMaxForwardSpeed();

		float GetMaxDriveForce();

		float GetAdjustedDriveForce(float absSpeed, float topSpeed);

		float GetModifiedDrag();

		CarWheel[] GetWheels();

		float GetWheelsMidPos();
	}

	private class ServerWheelData
	{
		public CarWheel wheel;

		public Transform wheelColliderTransform;

		public WheelCollider wheelCollider;

		public bool isGrounded;

		public float downforce;

		public float forceDistance;

		public WheelHit hit;

		public Vector2 localRigForce;

		public Vector2 localVelocity;

		public float angularVelocity;

		public Vector3 origin;

		public Vector2 tyreForce;

		public Vector2 tyreSlip;

		public Vector3 velocity;

		public bool isBraking;

		public bool hasThrottleInput;

		public bool isFrontWheel;

		public bool isLeftWheel;
	}

	private readonly ServerWheelData[] wheelData;

	private readonly TCar car;

	private readonly Transform transform;

	private readonly Rigidbody rBody;

	private readonly CarSettings vehicleSettings;

	private float speedAngle;

	private bool wasSleeping = true;

	private bool hasDriver;

	private bool hadDriver;

	private float lastMovingTime = float.MinValue;

	private WheelFrictionCurve zeroFriction = new WheelFrictionCurve
	{
		stiffness = 0f
	};

	private Vector3 prevLocalCOM;

	private readonly float midWheelPos;

	private const bool WHEEL_HIT_CORRECTION = true;

	private const float SLEEP_SPEED = 0.25f;

	private const float SLEEP_DELAY = 10f;

	private const float AIR_DRAG = 0.25f;

	private const float DEFAULT_GROUND_GRIP = 0.75f;

	private const float ROAD_GROUND_GRIP = 1f;

	private const float ICE_GROUND_GRIP = 0.25f;

	private bool slowSpeedExitFlag;

	private const float SLOW_SPEED_EXIT_SPEED = 4f;

	public TimeSince timeSinceWaterCheck;

	public float DriveWheelVelocity { get; private set; }

	public float DriveWheelSlip { get; private set; }

	public float SteerAngle { get; private set; }

	private bool InSlowSpeedExitMode
	{
		get
		{
			if (!hasDriver)
			{
				return slowSpeedExitFlag;
			}
			return false;
		}
	}

	public CarPhysics(TCar car, Transform transform, Rigidbody rBody, CarSettings vehicleSettings)
	{
		_003C_003Ec__DisplayClass39_0 _003C_003Ec__DisplayClass39_ = default(_003C_003Ec__DisplayClass39_0);
		_003C_003Ec__DisplayClass39_.transform = transform;
		base._002Ector();
		_003C_003Ec__DisplayClass39_._003C_003E4__this = this;
		this.car = car;
		this.transform = _003C_003Ec__DisplayClass39_.transform;
		this.rBody = rBody;
		this.vehicleSettings = vehicleSettings;
		timeSinceWaterCheck = default(TimeSince);
		timeSinceWaterCheck = float.MaxValue;
		prevLocalCOM = rBody.centerOfMass;
		CarWheel[] wheels = car.GetWheels();
		wheelData = new ServerWheelData[wheels.Length];
		for (int i = 0; i < wheelData.Length; i++)
		{
			wheelData[i] = _003C_002Ector_003Eg__AddWheel_007C39_0(wheels[i], ref _003C_003Ec__DisplayClass39_);
		}
		midWheelPos = car.GetWheelsMidPos();
		wheelData[0].wheel.wheelCollider.ConfigureVehicleSubsteps(1000f, 1, 1);
		lastMovingTime = Time.realtimeSinceStartup;
	}

	public void FixedUpdate(float dt, float speed)
	{
		if (rBody.centerOfMass != prevLocalCOM)
		{
			COMChanged();
		}
		float num = Mathf.Abs(speed);
		hasDriver = car.HasDriver();
		if (!hasDriver && hadDriver)
		{
			if (num <= 4f)
			{
				slowSpeedExitFlag = true;
			}
		}
		else if (hasDriver && !hadDriver)
		{
			slowSpeedExitFlag = false;
		}
		if ((hasDriver || !vehicleSettings.canSleep) && rBody.IsSleeping())
		{
			rBody.WakeUp();
		}
		if (!rBody.IsSleeping())
		{
			if ((wasSleeping && !rBody.isKinematic) || num > 0.25f || Mathf.Abs(rBody.angularVelocity.magnitude) > 0.25f)
			{
				lastMovingTime = Time.time;
			}
			if (vehicleSettings.canSleep && !hasDriver && Time.time > lastMovingTime + 10f)
			{
				for (int i = 0; i < wheelData.Length; i++)
				{
					ServerWheelData obj = wheelData[i];
					obj.wheelCollider.motorTorque = 0f;
					obj.wheelCollider.brakeTorque = 0f;
					obj.wheelCollider.steerAngle = 0f;
				}
				rBody.Sleep();
			}
			else
			{
				speedAngle = Vector3.Angle(rBody.velocity, transform.forward) * Mathf.Sign(Vector3.Dot(rBody.velocity, transform.right));
				float maxDriveForce = car.GetMaxDriveForce();
				float maxForwardSpeed = car.GetMaxForwardSpeed();
				float num2 = (car.IsOn() ? car.GetThrottleInput() : 0f);
				float steerInput = car.GetSteerInput();
				float brakeInput = (InSlowSpeedExitMode ? 1f : car.GetBrakeInput());
				float num3 = 1f;
				if (num < 3f)
				{
					num3 = 2.75f;
				}
				else if (num < 9f)
				{
					float t = Mathf.InverseLerp(9f, 3f, num);
					num3 = Mathf.Lerp(1f, 2.75f, t);
				}
				maxDriveForce *= num3;
				if (car.IsOn())
				{
					ComputeSteerAngle(steerInput, dt, speed);
				}
				if ((float)timeSinceWaterCheck > 0.25f)
				{
					float a = car.WaterFactor();
					float b = 0f;
					TriggerVehicleDrag result;
					if (car.FindTrigger<TriggerVehicleDrag>(out result))
					{
						b = result.vehicleDrag;
					}
					float a2 = ((num2 != 0f) ? 0f : 0.25f);
					float a3 = Mathf.Max(a, b);
					a3 = Mathf.Max(a3, car.GetModifiedDrag());
					rBody.drag = Mathf.Max(a2, a3);
					rBody.angularDrag = a3 * 0.5f;
					timeSinceWaterCheck = 0f;
				}
				int num4 = 0;
				float num5 = 0f;
				bool flag = !hasDriver && rBody.velocity.magnitude < 2.5f && (float)car.timeSinceLastPush > 2f;
				for (int j = 0; j < wheelData.Length; j++)
				{
					ServerWheelData serverWheelData = wheelData[j];
					serverWheelData.wheelCollider.motorTorque = 1E-05f;
					if (flag && car.OnSurface != VehicleTerrainHandler.Surface.Frictionless)
					{
						serverWheelData.wheelCollider.brakeTorque = 10000f;
					}
					else
					{
						serverWheelData.wheelCollider.brakeTorque = 0f;
					}
					if (serverWheelData.wheel.steerWheel)
					{
						serverWheelData.wheel.wheelCollider.steerAngle = (serverWheelData.isFrontWheel ? SteerAngle : (vehicleSettings.rearWheelSteer * (0f - SteerAngle)));
					}
					UpdateSuspension(serverWheelData);
					if (serverWheelData.isGrounded)
					{
						num4++;
						num5 += wheelData[j].downforce;
					}
				}
				AdjustHitForces(num4, num5 / (float)num4);
				for (int k = 0; k < wheelData.Length; k++)
				{
					ServerWheelData wd = wheelData[k];
					UpdateLocalFrame(wd, dt);
					ComputeTyreForces(wd, speed, maxDriveForce, maxForwardSpeed, num2, steerInput, brakeInput, num3);
					ApplyTyreForces(wd, num2, steerInput, speed);
				}
				ComputeOverallForces();
			}
			wasSleeping = false;
		}
		else
		{
			wasSleeping = true;
		}
		hadDriver = hasDriver;
	}

	public bool IsGrounded()
	{
		int num = 0;
		for (int i = 0; i < wheelData.Length; i++)
		{
			if (wheelData[i].isGrounded)
			{
				num++;
			}
			if (num >= 2)
			{
				return true;
			}
		}
		return false;
	}

	private void COMChanged()
	{
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			serverWheelData.forceDistance = GetWheelForceDistance(serverWheelData.wheel.wheelCollider);
		}
		prevLocalCOM = rBody.centerOfMass;
	}

	private void ComputeSteerAngle(float steerInput, float dt, float speed)
	{
		if (vehicleSettings.tankSteering)
		{
			SteerAngle = 0f;
			return;
		}
		float num = vehicleSettings.maxSteerAngle * steerInput;
		float num2 = Mathf.InverseLerp(0f, vehicleSettings.minSteerLimitSpeed, speed);
		if (vehicleSettings.steeringLimit)
		{
			float num3 = Mathf.Lerp(vehicleSettings.maxSteerAngle, vehicleSettings.minSteerLimitAngle, num2);
			num = Mathf.Clamp(num, 0f - num3, num3);
		}
		float num4 = 0f;
		if (vehicleSettings.steeringAssist)
		{
			float num5 = Mathf.InverseLerp(0.1f, 3f, speed);
			num4 = speedAngle * vehicleSettings.steeringAssistRatio * num5 * Mathf.InverseLerp(2f, 3f, Mathf.Abs(speedAngle));
		}
		float num6 = Mathf.Clamp(num + num4, 0f - vehicleSettings.maxSteerAngle, vehicleSettings.maxSteerAngle);
		if (SteerAngle != num6)
		{
			float num7 = 1f - num2 * 0.7f;
			float num9;
			if ((SteerAngle == 0f || Mathf.Sign(num6) == Mathf.Sign(SteerAngle)) && Mathf.Abs(num6) > Mathf.Abs(SteerAngle))
			{
				float num8 = SteerAngle / vehicleSettings.maxSteerAngle;
				num9 = Mathf.Lerp(vehicleSettings.steerMinLerpSpeed * num7, vehicleSettings.steerMaxLerpSpeed * num7, num8 * num8);
			}
			else
			{
				num9 = vehicleSettings.steerReturnLerpSpeed * num7;
			}
			if (car.GetSteerModInput())
			{
				num9 *= 1.5f;
			}
			SteerAngle = Mathf.MoveTowards(SteerAngle, num6, dt * num9);
		}
	}

	private float GetWheelForceDistance(WheelCollider col)
	{
		return rBody.centerOfMass.y - transform.InverseTransformPoint(col.transform.position).y + col.radius + (1f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
	}

	private void UpdateSuspension(ServerWheelData wd)
	{
		wd.isGrounded = wd.wheelCollider.GetGroundHit(out wd.hit);
		wd.origin = wd.wheelColliderTransform.TransformPoint(wd.wheelCollider.center);
		RaycastHit hitInfo;
		if (wd.isGrounded && GamePhysics.Trace(new Ray(wd.origin, -wd.wheelColliderTransform.up), 0f, out hitInfo, wd.wheelCollider.suspensionDistance + wd.wheelCollider.radius, 1235321089, QueryTriggerInteraction.Ignore))
		{
			wd.hit.point = hitInfo.point;
			wd.hit.normal = hitInfo.normal;
		}
		if (wd.isGrounded)
		{
			if (wd.hit.force < 0f)
			{
				wd.hit.force = 0f;
			}
			wd.downforce = wd.hit.force;
		}
		else
		{
			wd.downforce = 0f;
		}
	}

	private void AdjustHitForces(int groundedWheels, float neutralForcePerWheel)
	{
		float num = neutralForcePerWheel * 0.25f;
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			if (!serverWheelData.isGrounded || !(serverWheelData.downforce < num))
			{
				continue;
			}
			if (groundedWheels == 1)
			{
				serverWheelData.downforce = num;
				continue;
			}
			float a = (num - serverWheelData.downforce) / (float)(groundedWheels - 1);
			serverWheelData.downforce = num;
			for (int j = 0; j < wheelData.Length; j++)
			{
				ServerWheelData serverWheelData2 = wheelData[j];
				if (serverWheelData2.isGrounded && serverWheelData2.downforce > num)
				{
					float num2 = Mathf.Min(a, serverWheelData2.downforce - num);
					serverWheelData2.downforce -= num2;
				}
			}
		}
	}

	private void UpdateLocalFrame(ServerWheelData wd, float dt)
	{
		if (!wd.isGrounded)
		{
			wd.hit.point = wd.origin - wd.wheelColliderTransform.up * (wd.wheelCollider.suspensionDistance + wd.wheelCollider.radius);
			wd.hit.normal = wd.wheelColliderTransform.up;
			wd.hit.collider = null;
		}
		Vector3 pointVelocity = rBody.GetPointVelocity(wd.hit.point);
		wd.velocity = pointVelocity - Vector3.Project(pointVelocity, wd.hit.normal);
		wd.localVelocity.y = Vector3.Dot(wd.hit.forwardDir, wd.velocity);
		wd.localVelocity.x = Vector3.Dot(wd.hit.sidewaysDir, wd.velocity);
		if (!wd.isGrounded)
		{
			wd.localRigForce = Vector2.zero;
			return;
		}
		float num = Mathf.InverseLerp(1f, 0.25f, wd.velocity.sqrMagnitude);
		Vector2 zero = default(Vector2);
		if (num > 0f)
		{
			float num2 = Vector3.Dot(Vector3.up, wd.hit.normal);
			Vector3 rhs;
			if (num2 > 1E-06f)
			{
				Vector3 vector = Vector3.up * wd.downforce / num2;
				rhs = vector - Vector3.Project(vector, wd.hit.normal);
			}
			else
			{
				rhs = Vector3.up * 100000f;
			}
			zero.y = Vector3.Dot(wd.hit.forwardDir, rhs);
			zero.x = Vector3.Dot(wd.hit.sidewaysDir, rhs);
			zero *= num;
		}
		else
		{
			zero = Vector2.zero;
		}
		Vector2 vector2 = (0f - Mathf.Clamp(wd.downforce / (0f - Physics.gravity.y), 0f, wd.wheelCollider.sprungMass) * 0.5f) * wd.localVelocity / dt;
		wd.localRigForce = vector2 + zero;
	}

	private void ComputeTyreForces(ServerWheelData wd, float speed, float maxDriveForce, float maxSpeed, float throttleInput, float steerInput, float brakeInput, float driveForceMultiplier)
	{
		float absSpeed = Mathf.Abs(speed);
		if (vehicleSettings.tankSteering && brakeInput == 0f)
		{
			float tankSteerInvert = GetTankSteerInvert(throttleInput, speed);
			if (wd.isLeftWheel)
			{
				if (throttleInput == 0f)
				{
					throttleInput = 0f - steerInput;
				}
				else if (steerInput > 0f)
				{
					throttleInput = Mathf.Lerp(throttleInput, -1f * tankSteerInvert, steerInput);
				}
				else if (steerInput < 0f)
				{
					throttleInput = Mathf.Lerp(throttleInput, 1f * tankSteerInvert, 0f - steerInput);
				}
			}
			else if (throttleInput == 0f)
			{
				throttleInput = steerInput;
			}
			else if (steerInput > 0f)
			{
				throttleInput = Mathf.Lerp(throttleInput, 1f * tankSteerInvert, steerInput);
			}
			else if (steerInput < 0f)
			{
				throttleInput = Mathf.Lerp(throttleInput, -1f * tankSteerInvert, 0f - steerInput);
			}
		}
		float num = (wd.wheel.powerWheel ? throttleInput : 0f);
		wd.hasThrottleInput = num != 0f;
		float num2 = vehicleSettings.maxDriveSlip;
		if (Mathf.Sign(num) != Mathf.Sign(wd.localVelocity.y))
		{
			num2 -= wd.localVelocity.y * Mathf.Sign(num);
		}
		float num3 = Mathf.Abs(num);
		float num4 = 0f - vehicleSettings.rollingResistance + num3 * (1f + vehicleSettings.rollingResistance) - brakeInput * (1f - vehicleSettings.rollingResistance);
		if (InSlowSpeedExitMode || num4 < 0f || maxDriveForce == 0f)
		{
			num4 *= -1f;
			wd.isBraking = true;
		}
		else
		{
			num4 *= Mathf.Sign(num);
			wd.isBraking = false;
		}
		float num6;
		if (wd.isBraking)
		{
			float num5 = Mathf.Clamp(car.GetMaxForwardSpeed() * vehicleSettings.brakeForceMultiplier, 10f * vehicleSettings.brakeForceMultiplier, 50f * vehicleSettings.brakeForceMultiplier);
			num5 += rBody.mass * 1.5f;
			num6 = num4 * num5;
		}
		else
		{
			num6 = ComputeDriveForce(speed, absSpeed, num4 * maxDriveForce, maxDriveForce, maxSpeed, driveForceMultiplier);
		}
		if (wd.isGrounded)
		{
			wd.tyreSlip.x = wd.localVelocity.x;
			wd.tyreSlip.y = wd.localVelocity.y - wd.angularVelocity * wd.wheelCollider.radius;
			float num7;
			switch (car.OnSurface)
			{
			case VehicleTerrainHandler.Surface.Road:
				num7 = 1f;
				break;
			case VehicleTerrainHandler.Surface.Ice:
				num7 = 0.25f;
				break;
			case VehicleTerrainHandler.Surface.Frictionless:
				num7 = 0f;
				break;
			default:
				num7 = 0.75f;
				break;
			}
			float num8 = wd.wheel.tyreFriction * wd.downforce * num7;
			float num9 = 0f;
			if (!wd.isBraking)
			{
				num9 = Mathf.Min(Mathf.Abs(num6 * wd.tyreSlip.x) / num8, num2);
				if (num6 != 0f && num9 < 0.1f)
				{
					num9 = 0.1f;
				}
			}
			if (Mathf.Abs(wd.tyreSlip.y) < num9)
			{
				wd.tyreSlip.y = num9 * Mathf.Sign(wd.tyreSlip.y);
			}
			Vector2 vector = (0f - num8) * wd.tyreSlip.normalized;
			vector.x = Mathf.Abs(vector.x) * 1.5f;
			vector.y = Mathf.Abs(vector.y);
			wd.tyreForce.x = Mathf.Clamp(wd.localRigForce.x, 0f - vector.x, vector.x);
			if (wd.isBraking)
			{
				float num10 = Mathf.Min(vector.y, num6);
				wd.tyreForce.y = Mathf.Clamp(wd.localRigForce.y, 0f - num10, num10);
			}
			else
			{
				wd.tyreForce.y = Mathf.Clamp(num6, 0f - vector.y, vector.y);
			}
		}
		else
		{
			wd.tyreSlip = Vector2.zero;
			wd.tyreForce = Vector2.zero;
		}
		if (wd.isGrounded)
		{
			float num11;
			if (wd.isBraking)
			{
				num11 = 0f;
			}
			else
			{
				float driveForceToMaxSlip = vehicleSettings.driveForceToMaxSlip;
				num11 = Mathf.Clamp01((Mathf.Abs(num6) - Mathf.Abs(wd.tyreForce.y)) / driveForceToMaxSlip) * num2 * Mathf.Sign(num6);
			}
			wd.angularVelocity = (wd.localVelocity.y + num11) / wd.wheelCollider.radius;
			return;
		}
		float num12 = 50f;
		float num13 = 10f;
		if (num > 0f)
		{
			wd.angularVelocity += num12 * num;
		}
		else
		{
			wd.angularVelocity -= num13;
		}
		wd.angularVelocity -= num12 * brakeInput;
		wd.angularVelocity = Mathf.Clamp(wd.angularVelocity, 0f, maxSpeed / wd.wheelCollider.radius);
	}

	private float ComputeDriveForce(float speed, float absSpeed, float demandedForce, float maxForce, float maxForwardSpeed, float driveForceMultiplier)
	{
		float num = ((speed >= 0f) ? maxForwardSpeed : (maxForwardSpeed * vehicleSettings.reversePercentSpeed));
		if (absSpeed < num)
		{
			if ((speed >= 0f || demandedForce <= 0f) && (speed <= 0f || demandedForce >= 0f))
			{
				maxForce = car.GetAdjustedDriveForce(absSpeed, maxForwardSpeed) * driveForceMultiplier;
			}
			return Mathf.Clamp(demandedForce, 0f - maxForce, maxForce);
		}
		float num2 = maxForce * Mathf.Max(1f - absSpeed / num, -1f) * Mathf.Sign(speed);
		if ((speed < 0f && demandedForce > 0f) || (speed > 0f && demandedForce < 0f))
		{
			num2 = Mathf.Clamp(num2 + demandedForce, 0f - maxForce, maxForce);
		}
		return num2;
	}

	private void ComputeOverallForces()
	{
		DriveWheelVelocity = 0f;
		DriveWheelSlip = 0f;
		int num = 0;
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			if (serverWheelData.wheel.powerWheel)
			{
				DriveWheelVelocity += serverWheelData.angularVelocity;
				if (serverWheelData.isGrounded)
				{
					float num2 = ComputeCombinedSlip(serverWheelData.localVelocity, serverWheelData.tyreSlip);
					DriveWheelSlip += num2;
				}
				num++;
			}
		}
		if (num > 0)
		{
			DriveWheelVelocity /= num;
			DriveWheelSlip /= num;
		}
	}

	private static float ComputeCombinedSlip(Vector2 localVelocity, Vector2 tyreSlip)
	{
		float magnitude = localVelocity.magnitude;
		if (magnitude > 0.01f)
		{
			float num = tyreSlip.x * localVelocity.x / magnitude;
			float y = tyreSlip.y;
			return Mathf.Sqrt(num * num + y * y);
		}
		return tyreSlip.magnitude;
	}

	private void ApplyTyreForces(ServerWheelData wd, float throttleInput, float steerInput, float speed)
	{
		if (wd.isGrounded)
		{
			Vector3 force = wd.hit.forwardDir * wd.tyreForce.y;
			Vector3 force2 = wd.hit.sidewaysDir * wd.tyreForce.x;
			Vector3 sidewaysForceAppPoint = GetSidewaysForceAppPoint(wd, wd.hit.point);
			rBody.AddForceAtPosition(force, wd.hit.point, ForceMode.Force);
			rBody.AddForceAtPosition(force2, sidewaysForceAppPoint, ForceMode.Force);
		}
	}

	private Vector3 GetSidewaysForceAppPoint(ServerWheelData wd, Vector3 contactPoint)
	{
		Vector3 result = contactPoint + wd.wheelColliderTransform.up * vehicleSettings.antiRoll * wd.forceDistance;
		float num = (wd.wheel.steerWheel ? SteerAngle : 0f);
		if (num != 0f && Mathf.Sign(num) != Mathf.Sign(wd.tyreSlip.x))
		{
			result += wd.wheelColliderTransform.forward * midWheelPos * (vehicleSettings.handlingBias - 0.5f);
		}
		return result;
	}

	private float GetTankSteerInvert(float throttleInput, float speed)
	{
		float result = 1f;
		if (throttleInput < 0f && speed < 1.75f)
		{
			result = -1f;
		}
		else if (throttleInput == 0f && speed < -1f)
		{
			result = -1f;
		}
		else if (speed < -1f)
		{
			result = -1f;
		}
		return result;
	}
}
