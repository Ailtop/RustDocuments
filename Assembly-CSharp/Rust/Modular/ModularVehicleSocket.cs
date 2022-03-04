using System;
using UnityEngine;

namespace Rust.Modular
{
	[Serializable]
	public class ModularVehicleSocket
	{
		public enum SocketWheelType
		{
			NoWheel = 0,
			ForwardWheel = 1,
			BackWheel = 2
		}

		public enum SocketLocationType
		{
			Middle = 0,
			Front = 1,
			Back = 2
		}

		[SerializeField]
		private Transform socketTransform;

		[SerializeField]
		private SocketWheelType wheelType;

		[SerializeField]
		private SocketLocationType locationType;

		public Vector3 WorldPosition => socketTransform.position;

		public Quaternion WorldRotation => socketTransform.rotation;

		public SocketWheelType WheelType => wheelType;

		public SocketLocationType LocationType => locationType;

		public bool ShouldBeActive(ConditionalSocketSettings modelSettings)
		{
			bool flag = true;
			if (modelSettings.restrictOnLocation)
			{
				ConditionalSocketSettings.LocationCondition locationRestriction = modelSettings.locationRestriction;
				switch (LocationType)
				{
				case SocketLocationType.Back:
					flag = locationRestriction == ConditionalSocketSettings.LocationCondition.Back || locationRestriction == ConditionalSocketSettings.LocationCondition.NotFront || locationRestriction == ConditionalSocketSettings.LocationCondition.NotMiddle;
					break;
				case SocketLocationType.Front:
					flag = locationRestriction == ConditionalSocketSettings.LocationCondition.Front || locationRestriction == ConditionalSocketSettings.LocationCondition.NotBack || locationRestriction == ConditionalSocketSettings.LocationCondition.NotMiddle;
					break;
				case SocketLocationType.Middle:
					flag = locationRestriction == ConditionalSocketSettings.LocationCondition.Middle || locationRestriction == ConditionalSocketSettings.LocationCondition.NotFront || locationRestriction == ConditionalSocketSettings.LocationCondition.NotBack;
					break;
				}
			}
			if (flag && modelSettings.restrictOnWheel)
			{
				flag = WheelType == modelSettings.wheelRestriction;
			}
			return flag;
		}
	}
}
