using System;
using ConVar;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class DepthOfFieldEffectRenderer : PostProcessEffectRenderer<DepthOfFieldEffect>
{
	private float focalDistance01 = 10f;

	private float internalBlurWidth = 1f;

	private Shader dofShader;

	public override void Init()
	{
		dofShader = Shader.Find("Hidden/PostProcessing/DepthOfFieldEffect");
	}

	private float FocalDistance01(Camera cam, float worldDist)
	{
		return cam.WorldToViewportPoint((worldDist - cam.nearClipPlane) * cam.transform.forward + cam.transform.position).z / (cam.farClipPlane - cam.nearClipPlane);
	}

	private void WriteCoc(PostProcessRenderContext context, PropertySheet sheet)
	{
		CommandBuffer command = context.command;
		RenderTargetIdentifier source = context.source;
		RenderTextureFormat sourceFormat = context.sourceFormat;
		float num = 1f;
		int width = context.width / 2;
		int height = context.height / 2;
		int num2 = Shader.PropertyToID("DOFtemp1");
		int num3 = Shader.PropertyToID("DOFtemp2");
		command.GetTemporaryRT(num3, width, height, 0, FilterMode.Bilinear, sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, source, num3, sheet, 1);
		float num4 = internalBlurWidth * num;
		sheet.properties.SetVector("_Offsets", new Vector4(0f, num4, 0f, num4));
		command.GetTemporaryRT(num2, width, height, 0, FilterMode.Bilinear, sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num3, num2, sheet, 0);
		command.ReleaseTemporaryRT(num3);
		sheet.properties.SetVector("_Offsets", new Vector4(num4, 0f, 0f, num4));
		command.GetTemporaryRT(num3, width, height, 0, FilterMode.Bilinear, sourceFormat);
		RuntimeUtilities.BlitFullscreenTriangle(command, num2, num3, sheet, 0);
		command.ReleaseTemporaryRT(num2);
		command.SetGlobalTexture("_FgOverlap", num3);
		RuntimeUtilities.BlitFullscreenTriangle(command, source, source, sheet, 3, RenderBufferLoadAction.Load);
		command.ReleaseTemporaryRT(num3);
	}

	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(dofShader);
		CommandBuffer command = context.command;
		int width = context.width;
		int height = context.height;
		RenderTextureFormat sourceFormat = context.sourceFormat;
		bool value = base.settings.highResolution.value;
		DOFBlurSampleCountParameter blurSampleCount = base.settings.blurSampleCount;
		float value2 = base.settings.focalSize.value;
		float value3 = base.settings.focalLength.value;
		float value4 = base.settings.aperture.value;
		float value5 = base.settings.maxBlurSize.value;
		int nameID = Shader.PropertyToID("DOFrtLow");
		int nameID2 = Shader.PropertyToID("DOFrtLow2");
		value4 = Math.Max(value4, 0f);
		value5 = Math.Max(value5, 0.1f);
		value2 = Mathf.Clamp(value2, 0f, 2f);
		internalBlurWidth = Mathf.Max(value5, 0f);
		focalDistance01 = FocalDistance01(context.camera, value3);
		propertySheet.properties.SetVector("_CurveParams", new Vector4(1f, value2, value4 / 10f, focalDistance01));
		propertySheet.properties.SetVector("_DistortionParams", new Vector4(base.settings.anamorphicSqueeze, (float)base.settings.anamorphicBarrel * 2f, 0f, 0f));
		if (value)
		{
			internalBlurWidth *= 2f;
		}
		WriteCoc(context, propertySheet);
		if (ConVar.Graphics.dof_debug)
		{
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 5);
			return;
		}
		command.GetTemporaryRT(nameID, width >> 1, height >> 1, 0, FilterMode.Bilinear, sourceFormat);
		command.GetTemporaryRT(nameID2, width >> 1, height >> 1, 0, FilterMode.Bilinear, sourceFormat);
		int pass = 2;
		if ((float)base.settings.anamorphicSqueeze > 0f || (float)base.settings.anamorphicBarrel > 0f)
		{
			command.EnableShaderKeyword("ANAMORPHIC_BOKEH");
		}
		else
		{
			command.DisableShaderKeyword("ANAMORPHIC_BOKEH");
		}
		propertySheet.properties.SetVector("_Offsets", new Vector4(0f, internalBlurWidth, 0.025f, internalBlurWidth));
		propertySheet.properties.SetInt("_BlurCountMode", (int)blurSampleCount.value);
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, pass);
		command.ReleaseTemporaryRT(nameID);
		command.ReleaseTemporaryRT(nameID2);
	}
}
