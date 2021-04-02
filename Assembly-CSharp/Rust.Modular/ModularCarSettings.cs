using System;
using UnityEngine;

namespace Rust.Modular
{
	[Serializable]
	public class ModularCarSettings
	{
		[Range(0f, 3f)]
		[Header("Vehicle Setup")]
		public float tireFriction = 1f;

		[Range(0f, 1f)]
		public float rollingResistance = 0.05f;

		[Range(0f, 50f)]
		[Header("Wheels")]
		public float maxSteerAngle = 35f;

		public bool steeringAssist = true;

		[Range(0f, 1f)]
		public float steeringAssistRatio = 0.5f;

		public bool steeringLimit;

		[Range(0f, 50f)]
		public float minSteerLimitAngle = 6f;

		[Range(20f, 50f)]
		public float minSteerLimitSpeed = 30f;

		[Header("Motor")]
		public float engineStartupTime = 0.5f;

		public float maxDriveSlip = 4f;

		public float driveForceToMaxSlip = 1000f;

		public float reversePercentSpeed = 0.3f;

		[Header("Brakes")]
		public float brakeForceMultiplier = 1000f;

		[Header("Front/Rear Vehicle Balance")]
		[Range(0f, 1f)]
		public float handlingBias = 0.5f;
	}
}
