using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Modular
{
	public class ModularCarChassisVisuals : MonoBehaviour
	{
		[Serializable]
		public class Steering
		{
			public Transform steerL;

			public Transform steerR;

			public LookAtTarget steerRodL;

			public LookAtTarget steerRodR;

			public LookAtTarget steeringArm;
		}

		[Serializable]
		public class LookAtTarget
		{
			public Transform aim;

			public Transform target;

			public Vector3 angleAdjust;
		}

		public ModularCar modularCar;

		public Transform frontAxle;

		public Transform rearAxle;

		public Steering steering;

		public LookAtTarget transmission;

		public PhysicMaterial defaultGroundMaterial;

		public PhysicMaterial snowGroundMaterial;

		public PhysicMaterial grassGroundMaterial;

		public PhysicMaterial sandGroundMaterial;

		public List<PhysicMaterial> dirtGroundMaterials;

		public List<PhysicMaterial> stoneyGroundMaterials;

		[ReadOnly]
		[Tooltip("Copied automatically from the WheelColliders")]
		public float wheelRadius;

		[Tooltip("Copied automatically from the WheelColliders")]
		[ReadOnly]
		public float suspensionDistance;

		[Tooltip("Copied automatically from the WheelColliders")]
		[ReadOnly]
		public float springTargetPosition;
	}
}
