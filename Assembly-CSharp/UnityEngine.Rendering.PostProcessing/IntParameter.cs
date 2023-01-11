using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
public sealed class IntParameter : ParameterOverride<int>
{
	public override void Interp(int from, int to, float t)
	{
		value = (int)((float)from + (float)(to - from) * t);
	}
}
