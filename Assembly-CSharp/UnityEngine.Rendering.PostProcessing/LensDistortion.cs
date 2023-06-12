using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.LensDistortionRenderer), "Unity/Lens Distortion", true)]
public sealed class LensDistortion : PostProcessEffectSettings
{
	[Range(-100f, 100f)]
	[Tooltip("Total distortion amount.")]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Intensity multiplier on the x-axis. Set it to 0 to disable distortion on this axis.")]
	[DisplayName("X Multiplier")]
	[Range(0f, 1f)]
	public FloatParameter intensityX = new FloatParameter
	{
		value = 1f
	};

	[Range(0f, 1f)]
	[DisplayName("Y Multiplier")]
	[Tooltip("Intensity multiplier on the y-axis. Set it to 0 to disable distortion on this axis.")]
	public FloatParameter intensityY = new FloatParameter
	{
		value = 1f
	};

	[Space]
	[Range(-1f, 1f)]
	[Tooltip("Distortion center point (x-axis).")]
	public FloatParameter centerX = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Distortion center point (y-axis).")]
	[Range(-1f, 1f)]
	public FloatParameter centerY = new FloatParameter
	{
		value = 0f
	};

	[Space]
	[Range(0.01f, 5f)]
	[Tooltip("Global screen scaling.")]
	public FloatParameter scale = new FloatParameter
	{
		value = 1f
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value && !Mathf.Approximately(intensity, 0f) && ((float)intensityX > 0f || (float)intensityY > 0f))
		{
			return !RuntimeUtilities.isVREnabled;
		}
		return false;
	}
}
