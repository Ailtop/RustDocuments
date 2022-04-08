using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class BloomRenderer : PostProcessEffectRenderer<Bloom>
{
	private enum Pass
	{
		Prefilter13 = 0,
		Prefilter4 = 1,
		Downsample13 = 2,
		Downsample4 = 3,
		UpsampleTent = 4,
		UpsampleBox = 5,
		DebugOverlayThreshold = 6,
		DebugOverlayTent = 7,
		DebugOverlayBox = 8
	}

	private struct Level
	{
		internal int down;

		internal int up;
	}

	private Level[] m_Pyramid;

	private const int k_MaxPyramidSize = 16;

	public override void Init()
	{
		m_Pyramid = new Level[16];
		for (int i = 0; i < 16; i++)
		{
			m_Pyramid[i] = new Level
			{
				down = Shader.PropertyToID("_BloomMipDown" + i),
				up = Shader.PropertyToID("_BloomMipUp" + i)
			};
		}
	}

	public override void Render(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("BloomPyramid");
		PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.bloom);
		propertySheet.properties.SetTexture(ShaderIDs.AutoExposureTex, context.autoExposureTexture);
		float num = Mathf.Clamp(base.settings.anamorphicRatio, -1f, 1f);
		float num2 = ((num < 0f) ? (0f - num) : 0f);
		float num3 = ((num > 0f) ? num : 0f);
		int num4 = Mathf.FloorToInt((float)context.screenWidth / (2f - num2));
		int num5 = Mathf.FloorToInt((float)context.screenHeight / (2f - num3));
		bool flag = context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass && context.camera.stereoTargetEye == StereoTargetEyeMask.Both;
		int num6 = (flag ? (num4 * 2) : num4);
		float num7 = Mathf.Log(Mathf.Max(num4, num5), 2f) + Mathf.Min(base.settings.diffusion.value, 10f) - 10f;
		int num8 = Mathf.FloorToInt(num7);
		int num9 = Mathf.Clamp(num8, 1, 16);
		float num10 = 0.5f + num7 - (float)num8;
		propertySheet.properties.SetFloat(ShaderIDs.SampleScale, num10);
		float num11 = Mathf.GammaToLinearSpace(base.settings.threshold.value);
		float num12 = num11 * base.settings.softKnee.value + 1E-05f;
		Vector4 value = new Vector4(num11, num11 - num12, num12 * 2f, 0.25f / num12);
		propertySheet.properties.SetVector(ShaderIDs.Threshold, value);
		float x = Mathf.GammaToLinearSpace(base.settings.clamp.value);
		propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(x, 0f, 0f, 0f));
		int num13 = (base.settings.fastMode ? 1 : 0);
		RenderTargetIdentifier source = context.source;
		for (int i = 0; i < num9; i++)
		{
			int down = m_Pyramid[i].down;
			int up = m_Pyramid[i].up;
			int pass = ((i == 0) ? num13 : (2 + num13));
			context.GetScreenSpaceTemporaryRT(command, down, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, num6, num5);
			context.GetScreenSpaceTemporaryRT(command, up, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, num6, num5);
			RuntimeUtilities.BlitFullscreenTriangle(command, source, down, propertySheet, pass);
			source = down;
			num6 = ((flag && num6 / 2 % 2 > 0) ? (1 + num6 / 2) : (num6 / 2));
			num6 = Mathf.Max(num6, 1);
			num5 = Mathf.Max(num5 / 2, 1);
		}
		int num14 = m_Pyramid[num9 - 1].down;
		for (int num15 = num9 - 2; num15 >= 0; num15--)
		{
			int down2 = m_Pyramid[num15].down;
			int up2 = m_Pyramid[num15].up;
			command.SetGlobalTexture(ShaderIDs.BloomTex, down2);
			RuntimeUtilities.BlitFullscreenTriangle(command, num14, up2, propertySheet, 4 + num13);
			num14 = up2;
		}
		Color linear = base.settings.color.value.linear;
		float num16 = RuntimeUtilities.Exp2(base.settings.intensity.value / 10f) - 1f;
		Vector4 value2 = new Vector4(num10, num16, base.settings.dirtIntensity.value, num9);
		if (context.IsDebugOverlayEnabled(DebugOverlay.BloomThreshold))
		{
			context.PushDebugOverlay(command, context.source, propertySheet, 6);
		}
		else if (context.IsDebugOverlayEnabled(DebugOverlay.BloomBuffer))
		{
			propertySheet.properties.SetVector(ShaderIDs.ColorIntensity, new Vector4(linear.r, linear.g, linear.b, num16));
			context.PushDebugOverlay(command, m_Pyramid[0].up, propertySheet, 7 + num13);
		}
		Texture texture = ((base.settings.dirtTexture.value == null) ? RuntimeUtilities.blackTexture : base.settings.dirtTexture.value);
		float num17 = (float)texture.width / (float)texture.height;
		float num18 = (float)context.screenWidth / (float)context.screenHeight;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		if (num17 > num18)
		{
			value3.x = num18 / num17;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num18 > num17)
		{
			value3.y = num17 / num18;
			value3.w = (1f - value3.y) * 0.5f;
		}
		PropertySheet uberSheet = context.uberSheet;
		if ((bool)base.settings.fastMode)
		{
			uberSheet.EnableKeyword("BLOOM_LOW");
		}
		else
		{
			uberSheet.EnableKeyword("BLOOM");
		}
		uberSheet.properties.SetVector(ShaderIDs.Bloom_DirtTileOffset, value3);
		uberSheet.properties.SetVector(ShaderIDs.Bloom_Settings, value2);
		uberSheet.properties.SetColor(ShaderIDs.Bloom_Color, linear);
		uberSheet.properties.SetTexture(ShaderIDs.Bloom_DirtTex, texture);
		command.SetGlobalTexture(ShaderIDs.BloomTex, num14);
		for (int j = 0; j < num9; j++)
		{
			if (m_Pyramid[j].down != num14)
			{
				command.ReleaseTemporaryRT(m_Pyramid[j].down);
			}
			if (m_Pyramid[j].up != num14)
			{
				command.ReleaseTemporaryRT(m_Pyramid[j].up);
			}
		}
		command.EndSample("BloomPyramid");
		context.bloomBufferNameID = num14;
	}
}
