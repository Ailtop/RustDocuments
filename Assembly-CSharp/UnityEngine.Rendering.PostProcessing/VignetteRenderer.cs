using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class VignetteRenderer : PostProcessEffectRenderer<Vignette>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet uberSheet = context.uberSheet;
		uberSheet.EnableKeyword("VIGNETTE");
		uberSheet.properties.SetColor(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Color, base.settings.color.value);
		if ((VignetteMode)base.settings.mode == VignetteMode.Classic)
		{
			uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Mode, 0f);
			uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Center, base.settings.center.value);
			float z = (1f - base.settings.roundness.value) * 6f + base.settings.roundness.value;
			uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Settings, new Vector4(base.settings.intensity.value * 3f, base.settings.smoothness.value * 5f, z, base.settings.rounded.value ? 1f : 0f));
		}
		else
		{
			uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Mode, 1f);
			uberSheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Mask, base.settings.mask.value);
			uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.Vignette_Opacity, Mathf.Clamp01(base.settings.opacity.value));
		}
	}
}
