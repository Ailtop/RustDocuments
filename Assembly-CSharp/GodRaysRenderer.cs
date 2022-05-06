using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class GodRaysRenderer : PostProcessEffectRenderer<GodRays>
{
	private const int PASS_SCREEN = 0;

	private const int PASS_ADD = 1;

	public Shader GodRayShader;

	public Shader ScreenClearShader;

	public Shader SkyMaskShader;

	public override void Init()
	{
		if (!GodRayShader)
		{
			GodRayShader = Shader.Find("Hidden/PostProcessing/GodRays");
		}
		if (!ScreenClearShader)
		{
			ScreenClearShader = Shader.Find("Hidden/PostProcessing/ScreenClear");
		}
		if (!SkyMaskShader)
		{
			SkyMaskShader = Shader.Find("Hidden/PostProcessing/SkyMask");
		}
	}

	private void DrawBorder(PostProcessRenderContext context, RenderTargetIdentifier buffer1)
	{
		PropertySheet propertySheet = context.propertySheets.Get(ScreenClearShader);
		Rect value = new Rect(0f, context.height - 1, context.width, 1f);
		Rect value2 = new Rect(0f, 0f, context.width, 1f);
		Rect value3 = new Rect(0f, 0f, 1f, context.height);
		Rect value4 = new Rect(context.width - 1, 0f, 1f, context.height);
		RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, buffer1, propertySheet, 0, clear: false, value);
		RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, buffer1, propertySheet, 0, clear: false, value2);
		RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, buffer1, propertySheet, 0, clear: false, value3);
		RuntimeUtilities.BlitFullscreenTriangle(context.command, BuiltinRenderTextureType.None, buffer1, propertySheet, 0, clear: false, value4);
	}

	private int GetSkyMask(PostProcessRenderContext context, ResolutionType resolution, Vector3 lightPos, int blurIterations, float blurRadius, float maxRadius)
	{
		CommandBuffer command = context.command;
		Camera camera = context.camera;
		PropertySheet propertySheet = context.propertySheets.Get(SkyMaskShader);
		command.BeginSample("GodRays");
		int width;
		int height;
		int depthBuffer;
		switch (resolution)
		{
		case ResolutionType.High:
			width = context.screenWidth;
			height = context.screenHeight;
			depthBuffer = 0;
			break;
		case ResolutionType.Normal:
			width = context.screenWidth / 2;
			height = context.screenHeight / 2;
			depthBuffer = 0;
			break;
		default:
			width = context.screenWidth / 4;
			height = context.screenHeight / 4;
			depthBuffer = 0;
			break;
		}
		int num = Shader.PropertyToID("buffer1");
		int num2 = Shader.PropertyToID("buffer2");
		command.GetTemporaryRT(num, width, height, depthBuffer);
		propertySheet.properties.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * blurRadius);
		propertySheet.properties.SetVector("_LightPosition", new Vector4(lightPos.x, lightPos.y, lightPos.z, maxRadius));
		if ((camera.depthTextureMode & DepthTextureMode.Depth) != 0)
		{
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, num, propertySheet, 1);
		}
		else
		{
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, num, propertySheet, 2);
		}
		if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
		{
			DrawBorder(context, num);
		}
		float num3 = blurRadius * 0.00130208337f;
		propertySheet.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		propertySheet.properties.SetVector("_LightPosition", new Vector4(lightPos.x, lightPos.y, lightPos.z, maxRadius));
		for (int i = 0; i < blurIterations; i++)
		{
			command.GetTemporaryRT(num2, width, height, depthBuffer);
			RuntimeUtilities.BlitFullscreenTriangle(command, num, num2, propertySheet, 0);
			command.ReleaseTemporaryRT(num);
			num3 = blurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			propertySheet.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
			command.GetTemporaryRT(num, width, height, depthBuffer);
			RuntimeUtilities.BlitFullscreenTriangle(command, num2, num, propertySheet, 0);
			command.ReleaseTemporaryRT(num2);
			num3 = blurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			propertySheet.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		}
		command.EndSample("GodRays");
		return num;
	}

	public override void Render(PostProcessRenderContext context)
	{
		Camera camera = context.camera;
		TOD_Sky instance = TOD_Sky.Instance;
		if (!(instance == null))
		{
			Vector3 lightPos = camera.WorldToViewportPoint(instance.Components.LightTransform.position);
			CommandBuffer command = context.command;
			PropertySheet propertySheet = context.propertySheets.Get(GodRayShader);
			int skyMask = GetSkyMask(context, base.settings.Resolution.value, lightPos, base.settings.BlurIterations.value, base.settings.BlurRadius.value, base.settings.MaxRadius.value);
			Color value = Color.black;
			if ((double)lightPos.z >= 0.0)
			{
				value = ((!instance.IsDay) ? (base.settings.Intensity.value * instance.MoonVisibility * instance.MoonRayColor) : (base.settings.Intensity.value * instance.SunVisibility * instance.SunRayColor));
			}
			propertySheet.properties.SetColor("_LightColor", value);
			command.SetGlobalTexture("_SkyMask", skyMask);
			if (base.settings.BlendMode.value == BlendModeType.Screen)
			{
				RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, propertySheet, 0);
			}
			else
			{
				RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, propertySheet, 1);
			}
			command.ReleaseTemporaryRT(skyMask);
		}
	}
}
