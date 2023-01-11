using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[Preserve]
internal sealed class ScalableAO : UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod
{
	private enum Pass
	{
		OcclusionEstimationForward = 0,
		OcclusionEstimationDeferred = 1,
		HorizontalBlurForward = 2,
		HorizontalBlurDeferred = 3,
		VerticalBlur = 4,
		CompositionForward = 5,
		CompositionDeferred = 6,
		DebugOverlay = 7
	}

	private RenderTexture m_Result;

	private PropertySheet m_PropertySheet;

	private AmbientOcclusion m_Settings;

	private readonly RenderTargetIdentifier[] m_MRT = new RenderTargetIdentifier[2]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.CameraTarget
	};

	private readonly int[] m_SampleCount = new int[5] { 4, 6, 10, 8, 12 };

	public ScalableAO(AmbientOcclusion settings)
	{
		m_Settings = settings;
	}

	public DepthTextureMode GetCameraFlags()
	{
		return DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
	}

	private void DoLazyInitialization(PostProcessRenderContext context)
	{
		m_PropertySheet = context.propertySheets.Get(context.resources.shaders.scalableAO);
		bool flag = false;
		if (m_Result == null || !m_Result.IsCreated())
		{
			m_Result = context.GetScreenSpaceTemporaryRT(0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			m_Result.hideFlags = HideFlags.DontSave;
			m_Result.filterMode = FilterMode.Bilinear;
			flag = true;
		}
		else if (m_Result.width != context.width || m_Result.height != context.height)
		{
			m_Result.Release();
			m_Result.width = context.width;
			m_Result.height = context.height;
			flag = true;
		}
		if (flag)
		{
			m_Result.Create();
		}
	}

	private void Render(PostProcessRenderContext context, CommandBuffer cmd, int occlusionSource)
	{
		DoLazyInitialization(context);
		m_Settings.radius.value = Mathf.Max(m_Settings.radius.value, 0.0001f);
		bool num = m_Settings.quality.value < AmbientOcclusionQuality.High;
		float value = m_Settings.intensity.value;
		float value2 = m_Settings.radius.value;
		float z = (num ? 0.5f : 1f);
		float w = m_SampleCount[(int)m_Settings.quality.value];
		PropertySheet propertySheet = m_PropertySheet;
		propertySheet.ClearKeywords();
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.AOParams, new Vector4(value, value2, z, w));
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.AOColor, Color.white - m_Settings.color.value);
		if (context.camera.actualRenderingPath == RenderingPath.Forward && RenderSettings.fog)
		{
			propertySheet.EnableKeyword("APPLY_FORWARD_FOG");
			propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.FogParams, new Vector3(RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance));
		}
		int num2 = ((!num) ? 1 : 2);
		int occlusionTexture = UnityEngine.Rendering.PostProcessing.ShaderIDs.OcclusionTexture1;
		int widthOverride = context.width / num2;
		int heightOverride = context.height / num2;
		context.GetScreenSpaceTemporaryRT(cmd, occlusionTexture, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, FilterMode.Bilinear, widthOverride, heightOverride);
		RuntimeUtilities.BlitFullscreenTriangle(cmd, BuiltinRenderTextureType.None, occlusionTexture, propertySheet, occlusionSource);
		int occlusionTexture2 = UnityEngine.Rendering.PostProcessing.ShaderIDs.OcclusionTexture2;
		context.GetScreenSpaceTemporaryRT(cmd, occlusionTexture2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		RuntimeUtilities.BlitFullscreenTriangle(cmd, occlusionTexture, occlusionTexture2, propertySheet, 2 + occlusionSource);
		cmd.ReleaseTemporaryRT(occlusionTexture);
		RuntimeUtilities.BlitFullscreenTriangle(cmd, occlusionTexture2, m_Result, propertySheet, 4);
		cmd.ReleaseTemporaryRT(occlusionTexture2);
		if (context.IsDebugOverlayEnabled(DebugOverlay.AmbientOcclusion))
		{
			context.PushDebugOverlay(cmd, m_Result, propertySheet, 7);
		}
	}

	public void RenderAfterOpaque(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion");
		Render(context, command, 0);
		command.SetGlobalTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.SAOcclusionTexture, m_Result);
		RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget, m_PropertySheet, 5, RenderBufferLoadAction.Load);
		command.EndSample("Ambient Occlusion");
	}

	public void RenderAmbientOnly(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion Render");
		Render(context, command, 1);
		command.EndSample("Ambient Occlusion Render");
	}

	public void CompositeAmbientOnly(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion Composite");
		command.SetGlobalTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.SAOcclusionTexture, m_Result);
		RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, m_MRT, BuiltinRenderTextureType.CameraTarget, m_PropertySheet, 6);
		command.EndSample("Ambient Occlusion Composite");
	}

	public void Release()
	{
		RuntimeUtilities.Destroy(m_Result);
		m_Result = null;
	}
}
