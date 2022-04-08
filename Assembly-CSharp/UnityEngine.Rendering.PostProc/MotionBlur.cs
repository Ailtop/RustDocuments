using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(MotionBlurRenderer), "Unity/Motion Blur", false)]
public sealed class MotionBlur : PostProcessEffectSettings
{
	[Range(0f, 360f)]
	[Tooltip("The angle of rotary shutter. Larger values give longer exposure.")]
	public FloatParameter shutterAngle = new FloatParameter
	{
		value = 270f
	};

	[Range(4f, 32f)]
	[Tooltip("The amount of sample points. This affects quality and performance.")]
	public IntParameter sampleCount = new IntParameter
	{
		value = 10
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value && shutterAngle.value > 0f && SystemInfo.supportsMotionVectors && RenderTextureFormat.RGHalf.IsSupported())
		{
			return !RuntimeUtilities.isVREnabled;
		}
		return false;
	}
}
