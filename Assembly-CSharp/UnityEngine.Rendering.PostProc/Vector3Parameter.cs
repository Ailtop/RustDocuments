using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class Vector3Parameter : ParameterOverride<Vector3>
	{
		public override void Interp(Vector3 from, Vector3 to, float t)
		{
			value.x = from.x + (to.x - from.x) * t;
			value.y = from.y + (to.y - from.y) * t;
			value.z = from.z + (to.z - from.z) * t;
		}

		public static implicit operator Vector2(Vector3Parameter prop)
		{
			return prop.value;
		}

		public static implicit operator Vector4(Vector3Parameter prop)
		{
			return prop.value;
		}
	}
}
