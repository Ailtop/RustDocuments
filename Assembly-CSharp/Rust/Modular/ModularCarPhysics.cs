using UnityEngine;

namespace Rust.Modular
{
	public class ModularCarPhysics
	{
		public class ServerWheelData
		{
			public ModularCar.Wheel wheel;

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

			public Vector2 tireForce;

			public Vector2 tireSlip;

			public Vector3 velocity;

			public bool isBraking;

			public bool hasThrottleInput;
		}

		public readonly ServerWheelData[] wheelData;

		public readonly ModularCar modularCar;

		public readonly Transform transform;

		public readonly Rigidbody rBody;

		public readonly ModularCarSettings vehicleSettings;

		public float speedAngle;

		public bool wasSleeping = true;

		public bool hasDriver;

		public bool hadDriver;

		public float lastMovingTime = float.MinValue;

		public WheelFrictionCurve zeroFriction = new WheelFrictionCurve
		{
			stiffness = 0f
		};

		public Vector3 prevLocalCOM;

		public readonly float midWheelPos;

		private const int NUM_WHEELS = 4;

		private const bool WHEEL_HIT_CORRECTION = true;

		private const float SLEEP_SPEED = 0.25f;

		private const float SLEEP_DELAY = 10f;

		private const float AIR_DRAG = 0.25f;

		private const float DEFAULT_GROUND_GRIP = 0.75f;

		private const float ROAD_GROUND_GRIP = 1f;

		private const float ICE_GROUND_GRIP = 0.25f;

		public bool slowSpeedExitFlag;

		private const float SLOW_SPEED_EXIT_SPEED = 4f;

		public float dragMod;

		public float dragModDuration;

		public TimeSince timeSinceDragModSet;

		public TimeSince timeSinceWaterCheck;

		public float DriveWheelVelocity { get; set; }

		public float DriveWheelSlip { get; set; }

		public float SteerAngle { get; set; }

		public bool InSlowSpeedExitMode
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

		public ModularCarPhysics(ModularCar modularCar, Transform transform, Rigidbody rBody, ModularCarSettings vehicleSettings)
		{
			this.modularCar = modularCar;
			this.transform = transform;
			this.rBody = rBody;
			this.vehicleSettings = vehicleSettings;
			timeSinceWaterCheck = default(TimeSince);
			timeSinceWaterCheck = float.MaxValue;
			timeSinceDragModSet = default(TimeSince);
			timeSinceDragModSet = float.MaxValue;
			prevLocalCOM = rBody.centerOfMass;
			wheelData = new ServerWheelData[4];
			wheelData[0] = _003C_002Ector_003Eg__AddWheel_007C42_0(modularCar.wheelFL);
			wheelData[1] = _003C_002Ector_003Eg__AddWheel_007C42_0(modularCar.wheelFR);
			wheelData[2] = _003C_002Ector_003Eg__AddWheel_007C42_0(modularCar.wheelRL);
			wheelData[3] = _003C_002Ector_003Eg__AddWheel_007C42_0(modularCar.wheelRR);
			midWheelPos = wheelData[0].wheelColliderTransform.position.z - wheelData[2].wheelColliderTransform.position.z;
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
			hasDriver = modularCar.HasDriver();
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
			if (hasDriver && rBody.IsSleeping())
			{
				rBody.WakeUp();
			}
			if (!rBody.IsSleeping())
			{
				if ((wasSleeping && !rBody.isKinematic) || num > 0.25f || Mathf.Abs(rBody.angularVelocity.magnitude) > 0.25f)
				{
					lastMovingTime = Time.time;
				}
				if (!hasDriver && Time.time > lastMovingTime + 10f)
				{
					for (int i = 0; i < 4; i++)
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
					float maxDriveForce = modularCar.GetMaxDriveForce();
					float maxForwardSpeed = modularCar.GetMaxForwardSpeed();
					float num2 = (modularCar.IsOn() ? modularCar.GetThrottleInput() : 0f);
					float brakeInput = (InSlowSpeedExitMode ? 1f : modularCar.GetBrakeInput());
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
					if (modularCar.IsOn())
					{
						ComputeSteerAngle(dt, speed);
					}
					if ((float)timeSinceWaterCheck > 0.25f)
					{
						float a = modularCar.WaterFactor();
						float b = 0f;
						TriggerVehicleDrag result;
						if (modularCar.FindTrigger<TriggerVehicleDrag>(out result))
						{
							b = result.vehicleDrag;
						}
						float a2 = ((num2 != 0f) ? 0f : 0.25f);
						float a3 = Mathf.Max(a, b);
						a3 = Mathf.Max(a3, GetModifiedDrag());
						rBody.drag = Mathf.Max(a2, a3);
						rBody.angularDrag = a3 * 0.5f;
						timeSinceWaterCheck = 0f;
					}
					int num4 = 0;
					float num5 = 0f;
					bool flag = !hasDriver && rBody.velocity.magnitude < 2.5f && (float)modularCar.timeSinceLastPush > 2f;
					for (int j = 0; j < 4; j++)
					{
						ServerWheelData serverWheelData = wheelData[j];
						serverWheelData.wheelCollider.motorTorque = 1E-05f;
						if (flag && modularCar.OnSurface != VehicleTerrainHandler.Surface.Frictionless)
						{
							serverWheelData.wheelCollider.brakeTorque = 10000f;
						}
						else
						{
							serverWheelData.wheelCollider.brakeTorque = 0f;
						}
						if (serverWheelData.wheel.steerWheel)
						{
							serverWheelData.wheel.wheelCollider.steerAngle = SteerAngle;
						}
						UpdateSuspension(serverWheelData);
						if (serverWheelData.isGrounded)
						{
							num4++;
							num5 += wheelData[j].downforce;
						}
					}
					AdjustHitForces(num4, num5 / (float)num4);
					for (int k = 0; k < 4; k++)
					{
						ServerWheelData wd = wheelData[k];
						UpdateLocalFrame(wd, dt);
						ComputeTireForces(wd, speed, maxDriveForce, maxForwardSpeed, num2, brakeInput, num3);
						ApplyTireForces(wd);
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

		public void SetTempDrag(float drag, float duration)
		{
			dragMod = Mathf.Clamp(drag, 0.25f, 1000f);
			timeSinceDragModSet = 0f;
			dragModDuration = duration;
		}

		public float GetModifiedDrag()
		{
			return (1f - Mathf.InverseLerp(0f, dragModDuration, timeSinceDragModSet)) * dragMod;
		}

		public void COMChanged()
		{
			for (int i = 0; i < 4; i++)
			{
				ServerWheelData serverWheelData = wheelData[i];
				serverWheelData.forceDistance = GetWheelForceDistance(serverWheelData.wheel.wheelCollider);
			}
			prevLocalCOM = rBody.centerOfMass;
		}

		public void ComputeSteerAngle(float dt, float speed)
		{
			float num = vehicleSettings.maxSteerAngle * modularCar.GetSteerInput();
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
					num9 = Mathf.Lerp(75f * num7, 150f * num7, num8 * num8);
				}
				else
				{
					num9 = 200f * num7;
				}
				if (modularCar.GetSteerModInput())
				{
					num9 *= 1.5f;
				}
				SteerAngle = Mathf.MoveTowards(SteerAngle, num6, dt * num9);
			}
		}

		public float GetWheelForceDistance(WheelCollider col)
		{
			return rBody.centerOfMass.y - transform.InverseTransformPoint(col.transform.position).y + col.radius + (1f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
		}

		public void UpdateSuspension(ServerWheelData wd)
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

		public void AdjustHitForces(int groundedWheels, float neutralForcePerWheel)
		{
			float num = neutralForcePerWheel * 0.25f;
			for (int i = 0; i < 4; i++)
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
				for (int j = 0; j < 4; j++)
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

		public void UpdateLocalFrame(ServerWheelData wd, float dt)
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

		public void ComputeTireForces(ServerWheelData wd, float speed, float maxDriveForce, float maxSpeed, float throttleInput, float brakeInput, float driveForceMultiplier)
		{
			float absSpeed = Mathf.Abs(speed);
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
				float num5 = Mathf.Clamp(modularCar.GetMaxForwardSpeed() * vehicleSettings.brakeForceMultiplier, 10f * vehicleSettings.brakeForceMultiplier, 50f * vehicleSettings.brakeForceMultiplier);
				num5 += rBody.mass * 1.5f;
				num6 = num4 * num5;
			}
			else
			{
				num6 = ComputeDriveForce(speed, absSpeed, num4 * maxDriveForce, maxDriveForce, maxSpeed, driveForceMultiplier);
			}
			if (wd.isGrounded)
			{
				wd.tireSlip.x = wd.localVelocity.x;
				wd.tireSlip.y = wd.localVelocity.y - wd.angularVelocity * wd.wheelCollider.radius;
				float num7;
				switch (modularCar.OnSurface)
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
				float num8 = vehicleSettings.tireFriction * wd.downforce * num7;
				float num9 = 0f;
				if (!wd.isBraking)
				{
					num9 = Mathf.Min(Mathf.Abs(num6 * wd.tireSlip.x) / num8, num2);
					if (num6 != 0f && num9 < 0.1f)
					{
						num9 = 0.1f;
					}
				}
				if (Mathf.Abs(wd.tireSlip.y) < num9)
				{
					wd.tireSlip.y = num9 * Mathf.Sign(wd.tireSlip.y);
				}
				Vector2 vector = (0f - num8) * wd.tireSlip.normalized;
				vector.x = Mathf.Abs(vector.x) * 1.5f;
				vector.y = Mathf.Abs(vector.y);
				wd.tireForce.x = Mathf.Clamp(wd.localRigForce.x, 0f - vector.x, vector.x);
				if (wd.isBraking)
				{
					float num10 = Mathf.Min(vector.y, num6);
					wd.tireForce.y = Mathf.Clamp(wd.localRigForce.y, 0f - num10, num10);
				}
				else
				{
					wd.tireForce.y = Mathf.Clamp(num6, 0f - vector.y, vector.y);
				}
			}
			else
			{
				wd.tireSlip = Vector2.zero;
				wd.tireForce = Vector2.zero;
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
					num11 = Mathf.Clamp01((Mathf.Abs(num6) - Mathf.Abs(wd.tireForce.y)) / driveForceToMaxSlip) * num2 * Mathf.Sign(num6);
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

		public float ComputeDriveForce(float speed, float absSpeed, float demandedForce, float maxForce, float maxForwardSpeed, float driveForceMultiplier)
		{
			float num = ((speed >= 0f) ? maxForwardSpeed : (maxForwardSpeed * vehicleSettings.reversePercentSpeed));
			if (absSpeed < num)
			{
				if ((speed >= 0f || demandedForce <= 0f) && (speed <= 0f || demandedForce >= 0f))
				{
					maxForce = modularCar.GetAdjustedDriveForce(absSpeed, maxForwardSpeed) * driveForceMultiplier;
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

		public void ComputeOverallForces()
		{
			DriveWheelVelocity = 0f;
			DriveWheelSlip = 0f;
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				ServerWheelData serverWheelData = wheelData[i];
				if (serverWheelData.wheel.powerWheel)
				{
					DriveWheelVelocity += serverWheelData.angularVelocity;
					if (serverWheelData.isGrounded)
					{
						float num2 = ComputeCombinedSlip(serverWheelData.localVelocity, serverWheelData.tireSlip);
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

		public static float ComputeCombinedSlip(Vector2 localVelocity, Vector2 tireSlip)
		{
			float magnitude = localVelocity.magnitude;
			if (magnitude > 0.01f)
			{
				float num = tireSlip.x * localVelocity.x / magnitude;
				float y = tireSlip.y;
				return Mathf.Sqrt(num * num + y * y);
			}
			return tireSlip.magnitude;
		}

		public void ApplyTireForces(ServerWheelData wd)
		{
			if (wd.isGrounded)
			{
				Vector3 force = wd.hit.forwardDir * wd.tireForce.y;
				Vector3 force2 = wd.hit.sidewaysDir * wd.tireForce.x;
				Vector3 sidewaysForceAppPoint = GetSidewaysForceAppPoint(wd, wd.hit.point);
				rBody.AddForceAtPosition(force, wd.hit.point, ForceMode.Force);
				rBody.AddForceAtPosition(force2, sidewaysForceAppPoint, ForceMode.Force);
			}
		}

		public Vector3 GetSidewaysForceAppPoint(ServerWheelData wd, Vector3 contactPoint)
		{
			Vector3 result = contactPoint + wd.wheelColliderTransform.up * wd.forceDistance;
			float num = (wd.wheel.steerWheel ? SteerAngle : 0f);
			if (num != 0f && Mathf.Sign(num) != Mathf.Sign(wd.tireSlip.x))
			{
				result += wd.wheelColliderTransform.forward * midWheelPos * (vehicleSettings.handlingBias - 0.5f);
			}
			return result;
		}
	}
}
