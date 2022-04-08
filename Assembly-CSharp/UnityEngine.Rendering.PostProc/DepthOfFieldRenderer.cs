using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class DepthOfFieldRenderer : PostProcessEffectRenderer<DepthOfField>
	{
		private enum Pass
		{
			CoCCalculation = 0,
			CoCTemporalFilter = 1,
			DownsampleAndPrefilter = 2,
			BokehSmallKernel = 3,
			BokehMediumKernel = 4,
			BokehLargeKernel = 5,
			BokehVeryLargeKernel = 6,
			PostFilter = 7,
			Combine = 8,
			DebugOverlay = 9
		}

		private const int k_NumEyes = 2;

		private const int k_NumCoCHistoryTextures = 2;

		private readonly RenderTexture[][] m_CoCHistoryTextures = new RenderTexture[2][];

		private int[] m_HistoryPingPong = new int[2];

		private const float k_FilmHeight = 0.024f;

		public DepthOfFieldRenderer()
		{
			for (int i = 0; i < 2; i++)
			{
				m_CoCHistoryTextures[i] = new RenderTexture[2];
				m_HistoryPingPong[i] = 0;
			}
		}

		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		private RenderTextureFormat SelectFormat(RenderTextureFormat primary, RenderTextureFormat secondary)
		{
			if (primary.IsSupported())
			{
				return primary;
			}
			if (secondary.IsSupported())
			{
				return secondary;
			}
			return RenderTextureFormat.Default;
		}

		private float CalculateMaxCoCRadius(int screenHeight)
		{
			float num = (float)base.settings.kernelSize.value * 4f + 6f;
			return Mathf.Min(0.05f, num / (float)screenHeight);
		}

		private RenderTexture CheckHistory(int eye, int id, PostProcessRenderContext context, RenderTextureFormat format)
		{
			RenderTexture renderTexture = m_CoCHistoryTextures[eye][id];
			if (m_ResetHistory || renderTexture == null || !renderTexture.IsCreated() || renderTexture.width != context.width || renderTexture.height != context.height)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = context.GetScreenSpaceTemporaryRT(0, format, RenderTextureReadWrite.Linear);
				renderTexture.name = "CoC History, Eye: " + eye + ", ID: " + id;
				renderTexture.filterMode = FilterMode.Bilinear;
				renderTexture.Create();
				m_CoCHistoryTextures[eye][id] = renderTexture;
			}
			return renderTexture;
		}

		public override void Render(PostProcessRenderContext context)
		{
			RenderTextureFormat sourceFormat = context.sourceFormat;
			RenderTextureFormat renderTextureFormat = SelectFormat(RenderTextureFormat.R8, RenderTextureFormat.RHalf);
			float num = 0.024f * ((float)context.height / 1080f);
			float num2 = base.settings.focalLength.value / 1000f;
			float num3 = Mathf.Max(base.settings.focusDistance.value, num2);
			float num4 = (float)context.screenWidth / (float)context.screenHeight;
			float value = num2 * num2 / (base.settings.aperture.value * (num3 - num2) * num * 2f);
			float num5 = CalculateMaxCoCRadius(context.screenHeight);
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.depthOfField);
			propertySheet.properties.Clear();
			propertySheet.properties.SetFloat(ShaderIDs.Distance, num3);
			propertySheet.properties.SetFloat(ShaderIDs.LensCoeff, value);
			propertySheet.properties.SetFloat(ShaderIDs.MaxCoC, num5);
			propertySheet.properties.SetFloat(ShaderIDs.RcpMaxCoC, 1f / num5);
			propertySheet.properties.SetFloat(ShaderIDs.RcpAspect, 1f / num4);
			CommandBuffer command = context.command;
			command.BeginSample("DepthOfField");
			context.GetScreenSpaceTemporaryRT(command, ShaderIDs.CoCTex, 0, renderTextureFormat, RenderTextureReadWrite.Linear);
			RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, ShaderIDs.CoCTex, propertySheet, 0);
			if (context.IsTemporalAntialiasingActive() || context.dlssEnabled)
			{
				float motionBlending = context.temporalAntialiasing.motionBlending;
				float z = (m_ResetHistory ? 0f : motionBlending);
				Vector2 jitter = context.temporalAntialiasing.jitter;
				propertySheet.properties.SetVector(ShaderIDs.TaaParams, new Vector3(jitter.x, jitter.y, z));
				int num6 = m_HistoryPingPong[context.xrActiveEye];
				RenderTexture renderTexture = CheckHistory(context.xrActiveEye, ++num6 % 2, context, renderTextureFormat);
				RenderTexture renderTexture2 = CheckHistory(context.xrActiveEye, ++num6 % 2, context, renderTextureFormat);
				m_HistoryPingPong[context.xrActiveEye] = ++num6 % 2;
				RuntimeUtilities.BlitFullscreenTriangle(command, renderTexture, renderTexture2, propertySheet, 1);
				command.ReleaseTemporaryRT(ShaderIDs.CoCTex);
				command.SetGlobalTexture(ShaderIDs.CoCTex, renderTexture2);
			}
			context.GetScreenSpaceTemporaryRT(command, ShaderIDs.DepthOfFieldTex, 0, sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / 2, context.height / 2);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, ShaderIDs.DepthOfFieldTex, propertySheet, 2);
			context.GetScreenSpaceTemporaryRT(command, ShaderIDs.DepthOfFieldTemp, 0, sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / 2, context.height / 2);
			RuntimeUtilities.BlitFullscreenTriangle(command, ShaderIDs.DepthOfFieldTex, ShaderIDs.DepthOfFieldTemp, propertySheet, (int)(3 + base.settings.kernelSize.value));
			RuntimeUtilities.BlitFullscreenTriangle(command, ShaderIDs.DepthOfFieldTemp, ShaderIDs.DepthOfFieldTex, propertySheet, 7);
			command.ReleaseTemporaryRT(ShaderIDs.DepthOfFieldTemp);
			if (context.IsDebugOverlayEnabled(DebugOverlay.DepthOfField))
			{
				context.PushDebugOverlay(command, context.source, propertySheet, 9);
			}
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 8);
			command.ReleaseTemporaryRT(ShaderIDs.DepthOfFieldTex);
			if (!context.IsTemporalAntialiasingActive() || context.dlssEnabled)
			{
				command.ReleaseTemporaryRT(ShaderIDs.CoCTex);
			}
			command.EndSample("DepthOfField");
			m_ResetHistory = false;
		}

		public override void Release()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < m_CoCHistoryTextures[i].Length; j++)
				{
					RenderTexture.ReleaseTemporary(m_CoCHistoryTextures[i][j]);
					m_CoCHistoryTextures[i][j] = null;
				}
				m_HistoryPingPong[i] = 0;
			}
			ResetHistory();
		}
	}
}
