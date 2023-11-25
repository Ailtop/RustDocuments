using System;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(UnityEngine.Rendering.PostProcessing.VignetteRenderer), "Unity/Vignette", true)]
public sealed class Vignette : PostProcessEffectSettings
{
	[Tooltip("Use the \"Classic\" mode for parametric controls. Use the \"Masked\" mode to use your own texture mask.")]
	public VignetteModeParameter mode = new VignetteModeParameter
	{
		value = VignetteMode.Classic
	};

	[Tooltip("Vignette color.")]
	public ColorParameter color = new ColorParameter
	{
		value = new Color(0f, 0f, 0f, 1f)
	};

	[Tooltip("Sets the vignette center point (screen center is [0.5, 0.5]).")]
	public Vector2Parameter center = new Vector2Parameter
	{
		value = new Vector2(0.5f, 0.5f)
	};

	[Tooltip("Amount of vignetting on screen.")]
	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Smoothness of the vignette borders.")]
	[Range(0.01f, 1f)]
	public FloatParameter smoothness = new FloatParameter
	{
		value = 0.2f
	};

	[Tooltip("Lower values will make a square-ish vignette.")]
	[Range(0f, 1f)]
	public FloatParameter roundness = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Set to true to mark the vignette to be perfectly round. False will make its shape dependent on the current aspect ratio.")]
	public BoolParameter rounded = new BoolParameter
	{
		value = false
	};

	[Tooltip("A black and white mask to use as a vignette.")]
	public TextureParameter mask = new TextureParameter
	{
		value = null
	};

	[Range(0f, 1f)]
	[Tooltip("Mask opacity.")]
	public FloatParameter opacity = new FloatParameter
	{
		value = 1f
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value)
		{
			if (mode.value != 0 || !(intensity.value > 0f))
			{
				if (mode.value == VignetteMode.Masked && opacity.value > 0f)
				{
					return mask.value != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
