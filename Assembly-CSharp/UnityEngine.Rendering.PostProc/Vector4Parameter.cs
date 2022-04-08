using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class Vector4Parameter : ParameterOverride<Vector4>
	{
		public override void Interp(Vector4 from, Vector4 to, float t)
		{
			value.x = from.x + (to.x - from.x) * t;
			value.y = from.y + (to.y - from.y) * t;
			value.z = from.z + (to.z - from.z) * t;
			value.w = from.w + (to.w - from.w) * t;
		}

		public static implicit operator Vector2(Vector4Parameter prop)
		{
			return prop.value;
		}

		public static implicit operator Vector3(Vector4Parameter prop)
		{
			return prop.value;
		}
	}
}
