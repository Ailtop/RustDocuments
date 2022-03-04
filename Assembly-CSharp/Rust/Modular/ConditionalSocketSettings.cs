using System;

namespace Rust.Modular
{
	[Serializable]
	public class ConditionalSocketSettings
	{
		public enum LocationCondition
		{
			Middle = 0,
			Front = 1,
			Back = 2,
			NotMiddle = 3,
			NotFront = 4,
			NotBack = 5
		}

		public bool restrictOnLocation;

		public LocationCondition locationRestriction;

		public bool restrictOnWheel;

		public ModularVehicleSocket.SocketWheelType wheelRestriction;

		public bool HasSocketRestrictions
		{
			get
			{
				if (!restrictOnLocation)
				{
					return restrictOnWheel;
				}
				return true;
			}
		}
	}
}
