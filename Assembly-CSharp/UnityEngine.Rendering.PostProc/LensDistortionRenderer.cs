using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class LensDistortionRenderer : PostProcessEffectRenderer<LensDistortion>
	{
		public override void Render(PostProcessRenderContext context)
		{
			PropertySheet uberSheet = context.uberSheet;
			float val = 1.6f * Math.Max(Mathf.Abs(base.settings.intensity.value), 1f);
			float num = (float)Math.PI / 180f * Math.Min(160f, val);
			float y = 2f * Mathf.Tan(num * 0.5f);
			Vector4 value = new Vector4(base.settings.centerX.value, base.settings.centerY.value, Mathf.Max(base.settings.intensityX.value, 0.0001f), Mathf.Max(base.settings.intensityY.value, 0.0001f));
			Vector4 value2 = new Vector4((base.settings.intensity.value >= 0f) ? num : (1f / num), y, 1f / base.settings.scale.value, base.settings.intensity.value);
			uberSheet.EnableKeyword("DISTORT");
			uberSheet.properties.SetVector(ShaderIDs.Distortion_CenterScale, value);
			uberSheet.properties.SetVector(ShaderIDs.Distortion_Amount, value2);
		}
	}
}
