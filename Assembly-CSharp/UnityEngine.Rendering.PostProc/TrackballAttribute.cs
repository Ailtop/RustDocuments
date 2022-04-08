using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TrackballAttribute : Attribute
	{
		public enum Mode
		{
			None = 0,
			Lift = 1,
			Gamma = 2,
			Gain = 3
		}

		public readonly Mode mode;

		public TrackballAttribute(Mode mode)
		{
			this.mode = mode;
		}
	}
}
