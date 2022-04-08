using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
public sealed class SplineParameter : ParameterOverride<Spline>
{
	protected internal override void OnEnable()
	{
		if (value != null)
		{
			value.Cache(int.MinValue);
		}
	}

	internal override void SetValue(ParameterOverride parameter)
	{
		base.SetValue(parameter);
		if (value != null)
		{
			value.Cache(Time.renderedFrameCount);
		}
	}

	public override void Interp(Spline from, Spline to, float t)
	{
		if (from == null || to == null)
		{
			base.Interp(from, to, t);
			return;
		}
		int renderedFrameCount = Time.renderedFrameCount;
		from.Cache(renderedFrameCount);
		to.Cache(renderedFrameCount);
		for (int i = 0; i < 128; i++)
		{
			float num = from.cachedData[i];
			float num2 = to.cachedData[i];
			value.cachedData[i] = num + (num2 - num) * t;
		}
	}
}
