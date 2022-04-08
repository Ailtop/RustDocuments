using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class GrainRenderer : PostProcessEffectRenderer<Grain>
	{
		private RenderTexture m_GrainLookupRT;

		private const int k_SampleCount = 1024;

		private int m_SampleIndex;

		public override void Render(PostProcessRenderContext context)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float z = HaltonSeq.Get(m_SampleIndex & 0x3FF, 2);
			float w = HaltonSeq.Get(m_SampleIndex & 0x3FF, 3);
			if (++m_SampleIndex >= 1024)
			{
				m_SampleIndex = 0;
			}
			if (m_GrainLookupRT == null || !m_GrainLookupRT.IsCreated())
			{
				RuntimeUtilities.Destroy(m_GrainLookupRT);
				m_GrainLookupRT = new RenderTexture(128, 128, 0, GetLookupFormat())
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Repeat,
					anisoLevel = 0,
					name = "Grain Lookup Texture"
				};
				m_GrainLookupRT.Create();
			}
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.grainBaker);
			propertySheet.properties.Clear();
			propertySheet.properties.SetFloat(ShaderIDs.Phase, realtimeSinceStartup % 10f);
			propertySheet.properties.SetVector(ShaderIDs.GrainNoiseParameters, new Vector3(12.9898f, 78.233f, 43758.5469f));
			context.command.BeginSample("GrainLookup");
			RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, m_GrainLookupRT, propertySheet, base.settings.colored.value ? 1 : 0);
			context.command.EndSample("GrainLookup");
			PropertySheet uberSheet = context.uberSheet;
			uberSheet.EnableKeyword("GRAIN");
			uberSheet.properties.SetTexture(ShaderIDs.GrainTex, m_GrainLookupRT);
			uberSheet.properties.SetVector(ShaderIDs.Grain_Params1, new Vector2(base.settings.lumContrib.value, base.settings.intensity.value * 20f));
			uberSheet.properties.SetVector(ShaderIDs.Grain_Params2, new Vector4((float)context.width / (float)m_GrainLookupRT.width / base.settings.size.value, (float)context.height / (float)m_GrainLookupRT.height / base.settings.size.value, z, w));
		}

		private RenderTextureFormat GetLookupFormat()
		{
			if (RenderTextureFormat.ARGBHalf.IsSupported())
			{
				return RenderTextureFormat.ARGBHalf;
			}
			return RenderTextureFormat.ARGB32;
		}

		public override void Release()
		{
			RuntimeUtilities.Destroy(m_GrainLookupRT);
			m_GrainLookupRT = null;
			m_SampleIndex = 0;
		}
	}
}
