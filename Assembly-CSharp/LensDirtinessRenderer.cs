using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class LensDirtinessRenderer : PostProcessEffectRenderer<LensDirtinessEffect>
{
	private enum Pass
	{
		Threshold = 0,
		Kawase = 1,
		Compose = 2
	}

	private int dataProperty = Shader.PropertyToID("_data");

	private Shader lensDirtinessShader;

	public override void Init()
	{
		base.Init();
		lensDirtinessShader = Shader.Find("Hidden/PostProcessing/LensDirtiness");
	}

	public override void Render(PostProcessRenderContext context)
	{
		float value = base.settings.bloomSize.value;
		float value2 = base.settings.gain.value;
		float value3 = base.settings.threshold.value;
		float value4 = base.settings.dirtiness.value;
		Color value5 = base.settings.bloomColor.value;
		Texture value6 = base.settings.dirtinessTexture.value;
		bool value7 = base.settings.sceneTintsBloom.value;
		CommandBuffer command = context.command;
		command.BeginSample("LensDirtinessEffect");
		if (value7)
		{
			command.EnableShaderKeyword("_SCENE_TINTS_BLOOM");
		}
		PropertySheet propertySheet = context.propertySheets.Get(lensDirtinessShader);
		RenderTargetIdentifier source = context.source;
		RenderTargetIdentifier destination = context.destination;
		int width = context.width;
		int height = context.height;
		int num = Shader.PropertyToID("_RTT_BloomThreshold");
		int num2 = Shader.PropertyToID("_RTT_1");
		int num3 = Shader.PropertyToID("_RTT_2");
		int num4 = Shader.PropertyToID("_RTT_3");
		int num5 = Shader.PropertyToID("_RTT_4");
		int num6 = Shader.PropertyToID("_RTT_Bloom_1");
		int num7 = Shader.PropertyToID("_RTT_Bloom_2");
		propertySheet.properties.SetFloat("_Gain", value2);
		propertySheet.properties.SetFloat("_Threshold", value3);
		command.GetTemporaryRT(num, width / 2, height / 2, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, source, num, propertySheet, 0);
		propertySheet.properties.SetVector("_Offset", new Vector4(1f / (float)width, 1f / (float)height, 0f, 0f) * 2f);
		command.GetTemporaryRT(num2, width / 2, height / 2, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num, num2, propertySheet, 1);
		command.ReleaseTemporaryRT(num);
		command.GetTemporaryRT(num3, width / 4, height / 4, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num2, num3, propertySheet, 1);
		command.ReleaseTemporaryRT(num2);
		command.GetTemporaryRT(num4, width / 8, height / 8, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num3, num4, propertySheet, 1);
		command.ReleaseTemporaryRT(num3);
		command.GetTemporaryRT(num5, width / 16, height / 16, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num4, num5, propertySheet, 1);
		command.ReleaseTemporaryRT(num4);
		command.GetTemporaryRT(num6, width / 16, height / 16, 0, FilterMode.Bilinear, context.sourceFormat);
		command.GetTemporaryRT(num7, width / 16, height / 16, 0, FilterMode.Bilinear, context.sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num5, num6);
		command.ReleaseTemporaryRT(num5);
		for (int i = 1; i <= 8; i++)
		{
			float x = value * (float)i / (float)width;
			float y = value * (float)i / (float)height;
			propertySheet.properties.SetVector("_Offset", new Vector4(x, y, 0f, 0f));
			RuntimeUtilities.BlitFullscreenTriangle(command, num6, num7, propertySheet, 1);
			RuntimeUtilities.BlitFullscreenTriangle(command, num7, num6, propertySheet, 1);
		}
		command.SetGlobalTexture("_Bloom", num7);
		propertySheet.properties.SetFloat("_Dirtiness", value4);
		propertySheet.properties.SetColor("_BloomColor", value5);
		propertySheet.properties.SetTexture("_DirtinessTexture", value6);
		RuntimeUtilities.BlitFullscreenTriangle(command, source, destination, propertySheet, 2);
		command.ReleaseTemporaryRT(num6);
		command.ReleaseTemporaryRT(num7);
		command.EndSample("LensDirtinessEffect");
	}
}
