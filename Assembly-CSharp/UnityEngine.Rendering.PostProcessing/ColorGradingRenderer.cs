using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class ColorGradingRenderer : PostProcessEffectRenderer<ColorGrading>
{
	private enum Pass
	{
		LutGenLDRFromScratch = 0,
		LutGenLDR = 1,
		LutGenHDR2D = 2
	}

	private Texture2D m_GradingCurves;

	private readonly Color[] m_Pixels = new Color[256];

	private RenderTexture m_InternalLdrLut;

	private RenderTexture m_InternalLogLut;

	private const int k_Lut2DSize = 32;

	private const int k_Lut3DSize = 33;

	private readonly HableCurve m_HableCurve = new HableCurve();

	public override void Render(PostProcessRenderContext context)
	{
		GradingMode value = base.settings.gradingMode.value;
		bool flag = SystemInfo.supports3DRenderTextures && SystemInfo.supportsComputeShaders && context.resources.computeShaders.lut3DBaker != null && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3;
		if (value == GradingMode.External)
		{
			RenderExternalPipeline3D(context);
		}
		else if (value == GradingMode.HighDefinitionRange && flag)
		{
			RenderHDRPipeline3D(context);
		}
		else if (value == GradingMode.HighDefinitionRange)
		{
			RenderHDRPipeline2D(context);
		}
		else
		{
			RenderLDRPipeline2D(context);
		}
	}

	private void RenderExternalPipeline3D(PostProcessRenderContext context)
	{
		Texture value = base.settings.externalLut.value;
		if (!(value == null))
		{
			PropertySheet uberSheet = context.uberSheet;
			uberSheet.EnableKeyword("COLOR_GRADING_HDR_3D");
			uberSheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut3D, value);
			uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut3D_Params, new Vector2(1f / (float)value.width, (float)value.width - 1f));
			uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.PostExposure, RuntimeUtilities.Exp2(base.settings.postExposure.value));
			context.logLut = value;
		}
	}

	private void RenderHDRPipeline3D(PostProcessRenderContext context)
	{
		CheckInternalLogLut();
		ComputeShader lut3DBaker = context.resources.computeShaders.lut3DBaker;
		int kernelIndex = 0;
		switch (base.settings.tonemapper.value)
		{
		case Tonemapper.None:
			kernelIndex = lut3DBaker.FindKernel("KGenLut3D_NoTonemap");
			break;
		case Tonemapper.Neutral:
			kernelIndex = lut3DBaker.FindKernel("KGenLut3D_NeutralTonemap");
			break;
		case Tonemapper.ACES:
			kernelIndex = lut3DBaker.FindKernel("KGenLut3D_AcesTonemap");
			break;
		case Tonemapper.Custom:
			kernelIndex = lut3DBaker.FindKernel("KGenLut3D_CustomTonemap");
			break;
		}
		CommandBuffer command = context.command;
		command.SetComputeTextureParam(lut3DBaker, kernelIndex, "_Output", m_InternalLogLut);
		command.SetComputeVectorParam(lut3DBaker, "_Size", new Vector4(33f, 1f / 32f, 0f, 0f));
		Vector3 vector = ColorUtilities.ComputeColorBalance(base.settings.temperature.value, base.settings.tint.value);
		command.SetComputeVectorParam(lut3DBaker, "_ColorBalance", vector);
		command.SetComputeVectorParam(lut3DBaker, "_ColorFilter", base.settings.colorFilter.value);
		float x = base.settings.hueShift.value / 360f;
		float y = base.settings.saturation.value / 100f + 1f;
		float z = base.settings.contrast.value / 100f + 1f;
		command.SetComputeVectorParam(lut3DBaker, "_HueSatCon", new Vector4(x, y, z, 0f));
		Vector4 vector2 = new Vector4(base.settings.mixerRedOutRedIn, base.settings.mixerRedOutGreenIn, base.settings.mixerRedOutBlueIn, 0f);
		Vector4 vector3 = new Vector4(base.settings.mixerGreenOutRedIn, base.settings.mixerGreenOutGreenIn, base.settings.mixerGreenOutBlueIn, 0f);
		Vector4 vector4 = new Vector4(base.settings.mixerBlueOutRedIn, base.settings.mixerBlueOutGreenIn, base.settings.mixerBlueOutBlueIn, 0f);
		command.SetComputeVectorParam(lut3DBaker, "_ChannelMixerRed", vector2 / 100f);
		command.SetComputeVectorParam(lut3DBaker, "_ChannelMixerGreen", vector3 / 100f);
		command.SetComputeVectorParam(lut3DBaker, "_ChannelMixerBlue", vector4 / 100f);
		Vector3 vector5 = ColorUtilities.ColorToLift(base.settings.lift.value * 0.2f);
		Vector3 vector6 = ColorUtilities.ColorToGain(base.settings.gain.value * 0.8f);
		Vector3 vector7 = ColorUtilities.ColorToInverseGamma(base.settings.gamma.value * 0.8f);
		command.SetComputeVectorParam(lut3DBaker, "_Lift", new Vector4(vector5.x, vector5.y, vector5.z, 0f));
		command.SetComputeVectorParam(lut3DBaker, "_InvGamma", new Vector4(vector7.x, vector7.y, vector7.z, 0f));
		command.SetComputeVectorParam(lut3DBaker, "_Gain", new Vector4(vector6.x, vector6.y, vector6.z, 0f));
		command.SetComputeTextureParam(lut3DBaker, kernelIndex, "_Curves", GetCurveTexture(hdr: true));
		if (base.settings.tonemapper.value == Tonemapper.Custom)
		{
			m_HableCurve.Init(base.settings.toneCurveToeStrength.value, base.settings.toneCurveToeLength.value, base.settings.toneCurveShoulderStrength.value, base.settings.toneCurveShoulderLength.value, base.settings.toneCurveShoulderAngle.value, base.settings.toneCurveGamma.value);
			command.SetComputeVectorParam(lut3DBaker, "_CustomToneCurve", m_HableCurve.uniforms.curve);
			command.SetComputeVectorParam(lut3DBaker, "_ToeSegmentA", m_HableCurve.uniforms.toeSegmentA);
			command.SetComputeVectorParam(lut3DBaker, "_ToeSegmentB", m_HableCurve.uniforms.toeSegmentB);
			command.SetComputeVectorParam(lut3DBaker, "_MidSegmentA", m_HableCurve.uniforms.midSegmentA);
			command.SetComputeVectorParam(lut3DBaker, "_MidSegmentB", m_HableCurve.uniforms.midSegmentB);
			command.SetComputeVectorParam(lut3DBaker, "_ShoSegmentA", m_HableCurve.uniforms.shoSegmentA);
			command.SetComputeVectorParam(lut3DBaker, "_ShoSegmentB", m_HableCurve.uniforms.shoSegmentB);
		}
		context.command.BeginSample("HdrColorGradingLut3D");
		int num = Mathf.CeilToInt(8.25f);
		command.DispatchCompute(lut3DBaker, kernelIndex, num, num, num);
		context.command.EndSample("HdrColorGradingLut3D");
		RenderTexture internalLogLut = m_InternalLogLut;
		PropertySheet uberSheet = context.uberSheet;
		uberSheet.EnableKeyword("COLOR_GRADING_HDR_3D");
		uberSheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut3D, internalLogLut);
		uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut3D_Params, new Vector2(1f / (float)internalLogLut.width, (float)internalLogLut.width - 1f));
		uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.PostExposure, RuntimeUtilities.Exp2(base.settings.postExposure.value));
		context.logLut = internalLogLut;
	}

	private void RenderHDRPipeline2D(PostProcessRenderContext context)
	{
		CheckInternalStripLut();
		PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.lut2DBaker);
		propertySheet.ClearKeywords();
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D_Params, new Vector4(32f, 0.00048828125f, 1f / 64f, 1.032258f));
		Vector3 vector = ColorUtilities.ComputeColorBalance(base.settings.temperature.value, base.settings.tint.value);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ColorBalance, vector);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ColorFilter, base.settings.colorFilter.value);
		float x = base.settings.hueShift.value / 360f;
		float y = base.settings.saturation.value / 100f + 1f;
		float z = base.settings.contrast.value / 100f + 1f;
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.HueSatCon, new Vector3(x, y, z));
		Vector3 vector2 = new Vector3(base.settings.mixerRedOutRedIn, base.settings.mixerRedOutGreenIn, base.settings.mixerRedOutBlueIn);
		Vector3 vector3 = new Vector3(base.settings.mixerGreenOutRedIn, base.settings.mixerGreenOutGreenIn, base.settings.mixerGreenOutBlueIn);
		Vector3 vector4 = new Vector3(base.settings.mixerBlueOutRedIn, base.settings.mixerBlueOutGreenIn, base.settings.mixerBlueOutBlueIn);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerRed, vector2 / 100f);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerGreen, vector3 / 100f);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerBlue, vector4 / 100f);
		Vector3 vector5 = ColorUtilities.ColorToLift(base.settings.lift.value * 0.2f);
		Vector3 vector6 = ColorUtilities.ColorToGain(base.settings.gain.value * 0.8f);
		Vector3 vector7 = ColorUtilities.ColorToInverseGamma(base.settings.gamma.value * 0.8f);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lift, vector5);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.InvGamma, vector7);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Gain, vector6);
		propertySheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Curves, GetCurveTexture(hdr: true));
		switch (base.settings.tonemapper.value)
		{
		case Tonemapper.Custom:
			propertySheet.EnableKeyword("TONEMAPPING_CUSTOM");
			m_HableCurve.Init(base.settings.toneCurveToeStrength.value, base.settings.toneCurveToeLength.value, base.settings.toneCurveShoulderStrength.value, base.settings.toneCurveShoulderLength.value, base.settings.toneCurveShoulderAngle.value, base.settings.toneCurveGamma.value);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.CustomToneCurve, m_HableCurve.uniforms.curve);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ToeSegmentA, m_HableCurve.uniforms.toeSegmentA);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ToeSegmentB, m_HableCurve.uniforms.toeSegmentB);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.MidSegmentA, m_HableCurve.uniforms.midSegmentA);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.MidSegmentB, m_HableCurve.uniforms.midSegmentB);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ShoSegmentA, m_HableCurve.uniforms.shoSegmentA);
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ShoSegmentB, m_HableCurve.uniforms.shoSegmentB);
			break;
		case Tonemapper.ACES:
			propertySheet.EnableKeyword("TONEMAPPING_ACES");
			break;
		case Tonemapper.Neutral:
			propertySheet.EnableKeyword("TONEMAPPING_NEUTRAL");
			break;
		}
		context.command.BeginSample("HdrColorGradingLut2D");
		RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, m_InternalLdrLut, propertySheet, 2);
		context.command.EndSample("HdrColorGradingLut2D");
		RenderTexture internalLdrLut = m_InternalLdrLut;
		PropertySheet uberSheet = context.uberSheet;
		uberSheet.EnableKeyword("COLOR_GRADING_HDR_2D");
		uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D_Params, new Vector3(1f / (float)internalLdrLut.width, 1f / (float)internalLdrLut.height, (float)internalLdrLut.height - 1f));
		uberSheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D, internalLdrLut);
		uberSheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.PostExposure, RuntimeUtilities.Exp2(base.settings.postExposure.value));
	}

	private void RenderLDRPipeline2D(PostProcessRenderContext context)
	{
		CheckInternalStripLut();
		PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.lut2DBaker);
		propertySheet.ClearKeywords();
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D_Params, new Vector4(32f, 0.00048828125f, 1f / 64f, 1.032258f));
		Vector3 vector = ColorUtilities.ComputeColorBalance(base.settings.temperature.value, base.settings.tint.value);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ColorBalance, vector);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ColorFilter, base.settings.colorFilter.value);
		float x = base.settings.hueShift.value / 360f;
		float y = base.settings.saturation.value / 100f + 1f;
		float z = base.settings.contrast.value / 100f + 1f;
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.HueSatCon, new Vector3(x, y, z));
		Vector3 vector2 = new Vector3(base.settings.mixerRedOutRedIn, base.settings.mixerRedOutGreenIn, base.settings.mixerRedOutBlueIn);
		Vector3 vector3 = new Vector3(base.settings.mixerGreenOutRedIn, base.settings.mixerGreenOutGreenIn, base.settings.mixerGreenOutBlueIn);
		Vector3 vector4 = new Vector3(base.settings.mixerBlueOutRedIn, base.settings.mixerBlueOutGreenIn, base.settings.mixerBlueOutBlueIn);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerRed, vector2 / 100f);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerGreen, vector3 / 100f);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.ChannelMixerBlue, vector4 / 100f);
		Vector3 vector5 = ColorUtilities.ColorToLift(base.settings.lift.value);
		Vector3 vector6 = ColorUtilities.ColorToGain(base.settings.gain.value);
		Vector3 vector7 = ColorUtilities.ColorToInverseGamma(base.settings.gamma.value);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lift, vector5);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.InvGamma, vector7);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Gain, vector6);
		propertySheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.Brightness, (base.settings.brightness.value + 100f) / 100f);
		propertySheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Curves, GetCurveTexture(hdr: false));
		context.command.BeginSample("LdrColorGradingLut2D");
		Texture value = base.settings.ldrLut.value;
		if (value == null || value.width != value.height * value.height)
		{
			RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, m_InternalLdrLut, propertySheet, 0);
		}
		else
		{
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.UserLut2D_Params, new Vector4(1f / (float)value.width, 1f / (float)value.height, (float)value.height - 1f, base.settings.ldrLutContribution));
			RuntimeUtilities.BlitFullscreenTriangle(context.command, value, m_InternalLdrLut, propertySheet, 1);
		}
		context.command.EndSample("LdrColorGradingLut2D");
		RenderTexture internalLdrLut = m_InternalLdrLut;
		PropertySheet uberSheet = context.uberSheet;
		uberSheet.EnableKeyword("COLOR_GRADING_LDR_2D");
		uberSheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D_Params, new Vector3(1f / (float)internalLdrLut.width, 1f / (float)internalLdrLut.height, (float)internalLdrLut.height - 1f));
		uberSheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.Lut2D, internalLdrLut);
	}

	private void CheckInternalLogLut()
	{
		if (m_InternalLogLut == null || !m_InternalLogLut.IsCreated())
		{
			RuntimeUtilities.Destroy(m_InternalLogLut);
			RenderTextureFormat lutFormat = GetLutFormat();
			m_InternalLogLut = new RenderTexture(33, 33, 0, lutFormat, RenderTextureReadWrite.Linear)
			{
				name = "Color Grading Log Lut",
				dimension = TextureDimension.Tex3D,
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				anisoLevel = 0,
				enableRandomWrite = true,
				volumeDepth = 33,
				autoGenerateMips = false,
				useMipMap = false
			};
			m_InternalLogLut.Create();
		}
	}

	private void CheckInternalStripLut()
	{
		if (m_InternalLdrLut == null || !m_InternalLdrLut.IsCreated())
		{
			RuntimeUtilities.Destroy(m_InternalLdrLut);
			RenderTextureFormat lutFormat = GetLutFormat();
			m_InternalLdrLut = new RenderTexture(1024, 32, 0, lutFormat, RenderTextureReadWrite.Linear)
			{
				name = "Color Grading Strip Lut",
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				anisoLevel = 0,
				autoGenerateMips = false,
				useMipMap = false
			};
			m_InternalLdrLut.Create();
		}
	}

	private Texture2D GetCurveTexture(bool hdr)
	{
		if (m_GradingCurves == null)
		{
			TextureFormat curveFormat = GetCurveFormat();
			m_GradingCurves = new Texture2D(128, 2, curveFormat, mipChain: false, linear: true)
			{
				name = "Internal Curves Texture",
				hideFlags = HideFlags.DontSave,
				anisoLevel = 0,
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Bilinear
			};
		}
		Spline value = base.settings.hueVsHueCurve.value;
		Spline value2 = base.settings.hueVsSatCurve.value;
		Spline value3 = base.settings.satVsSatCurve.value;
		Spline value4 = base.settings.lumVsSatCurve.value;
		Spline value5 = base.settings.masterCurve.value;
		Spline value6 = base.settings.redCurve.value;
		Spline value7 = base.settings.greenCurve.value;
		Spline value8 = base.settings.blueCurve.value;
		Color[] pixels = m_Pixels;
		for (int i = 0; i < 128; i++)
		{
			float r = value.cachedData[i];
			float g = value2.cachedData[i];
			float b = value3.cachedData[i];
			float a = value4.cachedData[i];
			pixels[i] = new Color(r, g, b, a);
			if (!hdr)
			{
				float a2 = value5.cachedData[i];
				float r2 = value6.cachedData[i];
				float g2 = value7.cachedData[i];
				float b2 = value8.cachedData[i];
				pixels[i + 128] = new Color(r2, g2, b2, a2);
			}
		}
		m_GradingCurves.SetPixels(pixels);
		m_GradingCurves.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		return m_GradingCurves;
	}

	private static bool IsRenderTextureFormatSupportedForLinearFiltering(RenderTextureFormat format)
	{
		return SystemInfo.IsFormatSupported(GraphicsFormatUtility.GetGraphicsFormat(format, RenderTextureReadWrite.Linear), FormatUsage.Linear);
	}

	private static RenderTextureFormat GetLutFormat()
	{
		RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;
		if (!IsRenderTextureFormatSupportedForLinearFiltering(renderTextureFormat))
		{
			renderTextureFormat = RenderTextureFormat.ARGB2101010;
			if (!IsRenderTextureFormatSupportedForLinearFiltering(renderTextureFormat))
			{
				renderTextureFormat = RenderTextureFormat.ARGB32;
			}
		}
		return renderTextureFormat;
	}

	private static TextureFormat GetCurveFormat()
	{
		TextureFormat textureFormat = TextureFormat.RGBAHalf;
		if (!SystemInfo.SupportsTextureFormat(textureFormat))
		{
			textureFormat = TextureFormat.ARGB32;
		}
		return textureFormat;
	}

	public override void Release()
	{
		RuntimeUtilities.Destroy(m_InternalLdrLut);
		m_InternalLdrLut = null;
		RuntimeUtilities.Destroy(m_InternalLogLut);
		m_InternalLogLut = null;
		RuntimeUtilities.Destroy(m_GradingCurves);
		m_GradingCurves = null;
	}
}
