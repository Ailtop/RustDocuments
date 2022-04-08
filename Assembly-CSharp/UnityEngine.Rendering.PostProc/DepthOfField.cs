using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DepthOfFieldRenderer), "Unity/Depth of Field", false)]
public sealed class DepthOfField : PostProcessEffectSettings
{
	[Min(0.1f)]
	[Tooltip("Distance to the point of focus.")]
	public FloatParameter focusDistance = new FloatParameter
	{
		value = 10f
	};

	[Range(0.05f, 32f)]
	[Tooltip("Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.")]
	public FloatParameter aperture = new FloatParameter
	{
		value = 5.6f
	};

	[Range(1f, 300f)]
	[Tooltip("Distance between the lens and the film. The larger the value is, the shallower the depth of field is.")]
	public FloatParameter focalLength = new FloatParameter
	{
		value = 50f
	};

	[DisplayName("Max Blur Size")]
	[Tooltip("Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects performances (the larger the kernel is, the longer the GPU time is required).")]
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
