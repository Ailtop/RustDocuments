using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	[PostProcess(typeof(AutoExposureRenderer), "Unity/Auto Exposure", true)]
	public sealed class AutoExposure : PostProcessEffectSettings
	{
		[MinMax(1f, 99f)]
		[DisplayName("Filtering (%)")]
		[Tooltip("Filters the bright and dark parts of the histogram when computing the average luminance. This is to avoid very dark pixels and very bright pixels from contributing to the auto exposure. Unit is in percent.")]
		public Vector2Parameter filtering = new Vector2Parameter
		{
			value = new Vector2(50f, 95f)
		};

		[Range(-9f, 9f)]
		[DisplayName("Minimum (EV)")]
		[Tooltip("Minimum average luminance to consider for auto exposure. Unit is EV.")]
		public FloatParameter minLuminance = new FloatParameter
		{
			value = 0f
		};

		[Range(-9f, 9f)]
		[DisplayName("Maximum (EV)")]
		[Tooltip("Maximum average luminance to consider for auto exposure. Unit is EV.")]
		public FloatParameter maxLuminance = new FloatParameter
		{
			value = 0f
		};

		[Min(0f)]
		[DisplayName("Exposure Compensation")]
		[Tooltip("Use this to scale the global exposure of the scene.")]
		public FloatParameter keyValue = new FloatParameter
		{
			value = 1f
		};

		[DisplayName("Type")]
		[Tooltip("Use \"Progressive\" if you want auto exposure to be animated. Use \"Fixed\" otherwise.")]
		public EyeAdaptationParameter eyeAdaptation = new EyeAdaptationParameter
		{
			value = EyeAdaptation.Progressive
		};

		[Min(0f)]
		[Tooltip("Adaptation speed from a dark to a light environment.")]
		public FloatParameter speedUp = new FloatParameter
		{
			value = 2f
		};

		[Min(0f)]
		[Tooltip("Adaptation speed from a light to a dark environment.")]
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
}
