using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class FloatParameter : ParameterOverride<float>
	{
		public override void Interp(float from, float to, float t)
		{
			value = from + (to - from) * t;
		}
	}
}
