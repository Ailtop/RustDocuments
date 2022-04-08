using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ChromaticAberrationRenderer), "Unity/Chromatic Aberration", true)]
public sealed class ChromaticAberration : PostProcessEffectSettings
{
	[Tooltip("Shifts the hue of chromatic aberrations.")]
	public TextureParameter spectralLut = new TextureParameter
	{
		value = null
	};

	[Range(0f, 1f)]
	[Tooltip("Amount of tangential distortion.")]
	public FloatParameter intensity = new FloatParameter
	{
		value = 0f
	};

	[FormerlySerializedAs("mobileOptimized")]
	[Tooltip("Boost performances by lowering the effect quality. This settings is meant to be used on mobile and other low-end platforms but can also provide a nice performance boost on desktops and consoles.")]
	public BoolParameter fastMode = new BoolParameter
	{
		value = false
	};

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (enabled.value)
		{
			return intensity.value > 0f;
		}
		return false;
	}
}
