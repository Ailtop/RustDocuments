using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class CathodeRenderer : PostProcessEffectRenderer<Cathode>
{
	private Texture2D noiseTex;

	private RenderTexture temporalRT;

	private Shader grayShader = Shader.Find("Hidden/Shader/Gray");

	private Shader primaryShader = Shader.Find("Hidden/Shader/PrimaryTransform");

	private Shader tvShader = Shader.Find("Hidden/Shader/TV");

	private Shader postTVShader = Shader.Find("Hidden/Shader/PostTV");

	private Shader trailShader = Shader.Find("Hidden/Shader/Trail");

	private readonly int _CathodeRT1 = Shader.PropertyToID("CathodeRT1");

	private readonly int _CathodeRT2 = Shader.PropertyToID("CathodeRT2");

	private readonly int _Intensity = Shader.PropertyToID("_Intensity");

	private readonly int _SizeX = Shader.PropertyToID("_SizeX");

	private readonly int _SizeY = Shader.PropertyToID("_SizeY");

	private readonly int _ChromaSubsampling = Shader.PropertyToID("_ChromaSubsampling");

	private readonly int _Sharpen = Shader.PropertyToID("_Sharpen");

	private readonly int _SharpenRadius = Shader.PropertyToID("_SharpenRadius");

	private readonly int _ColorNoise = Shader.PropertyToID("_ColorNoise");

	private readonly int _RestlessFoot = Shader.PropertyToID("_RestlessFoot");

	private readonly int _FootAmplitude = Shader.PropertyToID("_FootAmplitude");

	private readonly int _ChromaOffset = Shader.PropertyToID("_ChromaOffset");

	private readonly int _ChromaIntensity = Shader.PropertyToID("_ChromaIntensity");

	private readonly int _ChromaInstability = Shader.PropertyToID("_ChromaInstability");

	private readonly int _BurnIn = Shader.PropertyToID("_BurnIn");

	private readonly int _TapeDust = Shader.PropertyToID("_TapeDust");

	private readonly int _TrailTex = Shader.PropertyToID("_TrailTex");

	private readonly int _NoiseTex = Shader.PropertyToID("_NoiseTex");

	private readonly int _Gamma = Shader.PropertyToID("_Gamma");

	private readonly int _ResponseCurve = Shader.PropertyToID("_ResponseCurve");

	private readonly int _Saturation = Shader.PropertyToID("_Saturation");

	private readonly int _Wobble = Shader.PropertyToID("_Wobble");

	private readonly int _Black = Shader.PropertyToID("_Black");

	private readonly int _White = Shader.PropertyToID("_White");

	private readonly int _DynamicRangeMin = Shader.PropertyToID("_DynamicRangeMin");

	private readonly int _DynamicRangeMax = Shader.PropertyToID("_DynamicRangeMax");

	private readonly int _ScreenWhiteBal = Shader.PropertyToID("_ScreenWhiteBal");

	private readonly int _Trailing = Shader.PropertyToID("_Trailing");

	public override void Init()
	{
		base.Init();
		grayShader = Shader.Find("Hidden/Shader/Gray");
		primaryShader = Shader.Find("Hidden/Shader/PrimaryTransform");
		tvShader = Shader.Find("Hidden/Shader/TV");
		postTVShader = Shader.Find("Hidden/Shader/PostTV");
		trailShader = Shader.Find("Hidden/Shader/Trail");
		noiseTex = Resources.Load<Texture2D>("Noise");
	}

	public override void Release()
	{
		if (noiseTex != null)
		{
			Resources.UnloadAsset(noiseTex);
			noiseTex = null;
		}
		if (temporalRT != null)
		{
			Object.DestroyImmediate(temporalRT);
			temporalRT = null;
		}
		base.Release();
	}

	public override void Render(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("CathodeAnalogueVideo");
		int num = context.width / (int)base.settings.downscaleTemporal;
		int num2 = context.height / (int)base.settings.downscaleTemporal;
		if (temporalRT == null || temporalRT.width != num || temporalRT.height != num2)
		{
			if (temporalRT != null)
			{
				Object.DestroyImmediate(temporalRT);
			}
			temporalRT = new RenderTexture(num, num2, 0, RenderTextureFormat.DefaultHDR);
		}
		if ((float)base.settings.intensity > 0f)
		{
			PropertySheet propertySheet = context.propertySheets.Get(grayShader);
			PropertySheet propertySheet2 = context.propertySheets.Get(primaryShader);
			PropertySheet propertySheet3 = context.propertySheets.Get(tvShader);
			PropertySheet propertySheet4 = context.propertySheets.Get(postTVShader);
			PropertySheet propertySheet5 = context.propertySheets.Get(trailShader);
			propertySheet.properties.Clear();
			propertySheet2.properties.Clear();
			propertySheet3.properties.Clear();
			propertySheet4.properties.Clear();
			propertySheet5.properties.Clear();
			propertySheet.properties.SetFloat(_Intensity, base.settings.intensity);
			propertySheet.properties.SetFloat(_SizeX, base.settings.horizontalBlur);
			propertySheet.properties.SetFloat(_SizeY, base.settings.verticalBlur);
			propertySheet2.properties.SetFloat(_Intensity, base.settings.intensity);
			propertySheet2.properties.SetFloat(_ChromaSubsampling, (float)base.settings.chromaSubsampling * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_Sharpen, (float)base.settings.sharpen * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_SharpenRadius, (float)base.settings.sharpenRadius * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_ColorNoise, (float)base.settings.colorNoise * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_RestlessFoot, (float)base.settings.restlessFoot * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_FootAmplitude, (float)base.settings.footAmplitude * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_ChromaOffset, (float)base.settings.chromaOffset * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_ChromaIntensity, Mathf.Lerp(1f, base.settings.chromaIntensity, base.settings.intensity));
			propertySheet2.properties.SetFloat(_ChromaInstability, (float)base.settings.chromaInstability * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_BurnIn, (float)base.settings.burnIn * (float)base.settings.intensity);
			propertySheet2.properties.SetFloat(_TapeDust, 1f - (float)base.settings.tapeDust * (float)base.settings.intensity);
			propertySheet2.properties.SetTexture(_TrailTex, temporalRT);
			propertySheet2.properties.SetTexture(_NoiseTex, noiseTex);
			propertySheet3.properties.SetFloat(_Intensity, base.settings.intensity);
			propertySheet3.properties.SetFloat(_Gamma, 1f);
			propertySheet4.properties.SetFloat(_Intensity, base.settings.intensity);
			propertySheet4.properties.SetFloat(_ResponseCurve, (float)base.settings.responseCurve * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_Saturation, Mathf.Lerp(1f, base.settings.saturation, base.settings.intensity));
			propertySheet4.properties.SetFloat(_Wobble, (float)base.settings.wobble * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_Black, base.settings.blackWhiteLevels.value.x * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_White, 1f - (1f - base.settings.blackWhiteLevels.value.y) * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_DynamicRangeMin, base.settings.dynamicRange.value.x * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_DynamicRangeMax, 1f - (1f - base.settings.dynamicRange.value.y) * (float)base.settings.intensity);
			propertySheet4.properties.SetFloat(_ScreenWhiteBal, (float)base.settings.whiteBallance * (float)base.settings.intensity);
			propertySheet5.properties.SetFloat(_Trailing, 1f - (float)base.settings.cometTrailing * (float)base.settings.intensity);
			RenderTextureDescriptor renderTextureDescriptor = default(RenderTextureDescriptor);
			renderTextureDescriptor.dimension = TextureDimension.Tex2D;
			renderTextureDescriptor.width = context.width / (int)base.settings.downscale;
			renderTextureDescriptor.height = context.height / (int)base.settings.downscale;
			renderTextureDescriptor.depthBufferBits = 0;
			renderTextureDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
			renderTextureDescriptor.useMipMap = true;
			renderTextureDescriptor.autoGenerateMips = true;
			renderTextureDescriptor.msaaSamples = 1;
			RenderTextureDescriptor desc = renderTextureDescriptor;
			command.GetTemporaryRT(_CathodeRT1, desc, FilterMode.Trilinear);
			command.GetTemporaryRT(_CathodeRT2, desc, FilterMode.Trilinear);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, _CathodeRT1, propertySheet, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, _CathodeRT1, _CathodeRT2, propertySheet2, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, _CathodeRT1, temporalRT, propertySheet5, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, _CathodeRT2, _CathodeRT1, propertySheet4, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, _CathodeRT1, context.destination, propertySheet3, 0);
			command.ReleaseTemporaryRT(_CathodeRT1);
			command.ReleaseTemporaryRT(_CathodeRT2);
		}
		else
		{
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination);
		}
		command.EndSample("CathodeAnalogueVideo");
	}
}
