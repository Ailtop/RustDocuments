using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class ColorParameter : ParameterOverride<Color>
	{
		public override void Interp(Color from, Color to, float t)
		{
			value.r = from.r + (to.r - from.r) * t;
			value.g = from.g + (to.g - from.g) * t;
			value.b = from.b + (to.b - from.b) * t;
			value.a = from.a + (to.a - from.a) * t;
		}

		public static implicit operator Vector4(ColorParameter prop)
		{
			return prop.value;
		}
	}
}
