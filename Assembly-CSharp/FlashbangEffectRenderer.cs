using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class FlashbangEffectRenderer : PostProcessEffectRenderer<FlashbangEffect>
{
	public static bool needsCapture;

	private Shader flashbangEffectShader;

	private RenderTexture screenRT;

	public override void Init()
	{
		base.Init();
		flashbangEffectShader = Shader.Find("Hidden/PostProcessing/FlashbangEffect");
	}

	public override void Render(PostProcessRenderContext context)
	{
		if (!Application.isPlaying)
		{
			RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination);
			return;
		}
		CommandBuffer command = context.command;
		CheckCreateRenderTexture(ref screenRT, "Flashbang", context.width, context.height, context.sourceFormat);
		command.BeginSample("FlashbangEffect");
		if (needsCapture)
		{
			command.CopyTexture(context.source, screenRT);
			needsCapture = false;
		}
		PropertySheet propertySheet = context.propertySheets.Get(flashbangEffectShader);
		propertySheet.properties.Clear();
		propertySheet.properties.SetFloat("_BurnIntensity", base.settings.burnIntensity.value);
		propertySheet.properties.SetFloat("_WhiteoutIntensity", base.settings.whiteoutIntensity.value);
		if ((bool)screenRT)
		{
			propertySheet.properties.SetTexture("_BurnOverlay", screenRT);
		}
		RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, propertySheet, 0);
		command.EndSample("FlashbangEffect");
	}

	public override void Release()
	{
		base.Release();
		SafeDestroyRenderTexture(ref screenRT);
	}

	private static void CheckCreateRenderTexture(ref RenderTexture rt, string name, int width, int height, RenderTextureFormat format)
	{
		if (rt == null || rt.width != width || rt.height != height)
		{
			SafeDestroyRenderTexture(ref rt);
			rt = new RenderTexture(width, height, 0, format)
			{
				hideFlags = HideFlags.DontSave
			};
			rt.name = name;
			rt.wrapMode = TextureWrapMode.Clamp;
			rt.Create();
		}
	}

	private static void SafeDestroyRenderTexture(ref RenderTexture rt)
	{
		if (rt != null)
		{
			rt.Release();
			Object.DestroyImmediate(rt);
			rt = null;
		}
	}
}
