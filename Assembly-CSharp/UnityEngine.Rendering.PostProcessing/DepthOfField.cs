using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.DepthOfFieldRenderer), "Unity/Depth of Field", false)]
public sealed class DepthOfField : PostProcessEffectSettings
{
	[Tooltip("Distance to the point of focus.")]
	[Min(0.1f)]
	public FloatParameter focusDistance = new FloatParameter
	{
		value = 10f
	};

	[Tooltip("Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.")]
	[Range(0.05f, 32f)]
	public FloatParameter aperture = new FloatParameter
	{
		value = 5.6f
	};

	[Tooltip("Distance between the lens and the film. The larger the value is, the shallower the depth of field is.")]
	[Range(1f, 300f)]
	public FloatParameter focalLength = new FloatParameter
	{
		value = 50f
	};

	[Tooltip("Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects performances (the larger the kernel is, the longer the GPU time is required).")]
	[DisplayName("Max Blur Size")]
	public KernelSizeParameter kernelSize = new KernelSizeParameter
	{
		value = KernelSize.Medium
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value)
		{
			return SystemInfo.graphicsShaderLevel >= 35;
		}
		return false;
	}
}
