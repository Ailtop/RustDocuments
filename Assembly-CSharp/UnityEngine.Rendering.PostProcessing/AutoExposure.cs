using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.AutoExposureRenderer), "Unity/Auto Exposure", true)]
public sealed class AutoExposure : PostProcessEffectSettings
{
	[Tooltip("Filters the bright and dark parts of the histogram when computing the average luminance. This is to avoid very dark pixels and very bright pixels from contributing to the auto exposure. Unit is in percent.")]
	[DisplayName("Filtering (%)")]
	[MinMax(1f, 99f)]
	public Vector2Parameter filtering = new Vector2Parameter
	{
		value = new Vector2(50f, 95f)
	};

	[Tooltip("Minimum average luminance to consider for auto exposure. Unit is EV.")]
	[DisplayName("Minimum (EV)")]
	[Range(-9f, 9f)]
	public FloatParameter minLuminance = new FloatParameter
	{
		value = 0f
	};

	[Range(-9f, 9f)]
	[Tooltip("Maximum average luminance to consider for auto exposure. Unit is EV.")]
	[DisplayName("Maximum (EV)")]
	public FloatParameter maxLuminance = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Use this to scale the global exposure of the scene.")]
	[Min(0f)]
	[DisplayName("Exposure Compensation")]
	public FloatParameter keyValue = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Use \"Progressive\" if you want auto exposure to be animated. Use \"Fixed\" otherwise.")]
	[DisplayName("Type")]
	public EyeAdaptationParameter eyeAdaptation = new EyeAdaptationParameter
	{
		value = EyeAdaptation.Progressive
	};

	[Tooltip("Adaptation speed from a dark to a light environment.")]
	[Min(0f)]
	public FloatParameter speedUp = new FloatParameter
	{
		value = 2f
	};

	[Tooltip("Adaptation speed from a light to a dark environment.")]
	[Min(0f)]
	public FloatParameter speedDown = new FloatParameter
	{
		value = 1f
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value && SystemInfo.supportsComputeShaders && !RuntimeUtilities.isAndroidOpenGL && RenderTextureFormat.RFloat.IsSupported() && (bool)context.resources.computeShaders.autoExposure)
		{
			return context.resources.computeShaders.exposureHistogram;
		}
		return false;
	}
}
