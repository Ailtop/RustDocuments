using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[Preserve]
public sealed class TemporalAntialiasing
{
	private enum Pass
	{
		SolverDilate = 0,
		SolverNoDilate = 1
	}

	[Range(0.1f, 1f)]
	[Tooltip("The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable, but blurrier, output.")]
	public float jitterSpread = 0.75f;

	[Range(0f, 3f)]
	[Tooltip("Controls the amount of sharpening applied to the color buffer. High values may introduce dark-border artifacts.")]
	public float sharpness = 0.25f;

	[Tooltip("The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color.")]
	[Range(0f, 0.99f)]
	public float stationaryBlending = 0.95f;

	[Tooltip("The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color.")]
	[Range(0f, 0.99f)]
	public float motionBlending = 0.85f;

	public Func<Camera, Vector2, Matrix4x4> jitteredMatrixFunc;

	private readonly RenderTargetIdentifier[] m_Mrt = new RenderTargetIdentifier[2];

	private bool m_ResetHistory = true;

	private const int k_NumEyes = 2;

	private const int k_NumHistoryTextures = 2;

	private readonly RenderTexture[][] m_HistoryTextures = new RenderTexture[2][];

	private readonly int[] m_HistoryPingPong = new int[2];

	public Vector2 jitter { get; private set; }

	public Vector2 jitterRaw { get; private set; }

	public int sampleIndex { get; private set; }

	public int sampleCount { get; set; }

	public bool IsSupported()
	{
		if (SystemInfo.supportedRenderTargetCount >= 2 && SystemInfo.supportsMotionVectors)
		{
			return SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
		}
		return false;
	}

	internal DepthTextureMode GetCameraFlags()
	{
		return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
	}

	internal void ResetHistory()
	{
		m_ResetHistory = true;
	}

	private Vector2 GenerateRandomOffset()
	{
		Vector2 result = new Vector2(HaltonSeq.Get((sampleIndex & 0x3FF) + 1, 2) - 0.5f, HaltonSeq.Get((sampleIndex & 0x3FF) + 1, 3) - 0.5f);
		if (++sampleIndex >= sampleCount)
		{
			sampleIndex = 0;
		}
		return result;
	}

	public Matrix4x4 GetJitteredProjectionMatrix(Camera camera)
	{
		jitter = GenerateRandomOffset();
		jitter *= jitterSpread;
		Matrix4x4 result = ((jitteredMatrixFunc == null) ? (camera.orthographic ? RuntimeUtilities.GetJitteredOrthographicProjectionMatrix(camera, jitter) : RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(camera, jitter)) : jitteredMatrixFunc(camera, jitter));
		jitterRaw = jitter;
		jitter = new Vector2(jitter.x / (float)camera.pixelWidth, jitter.y / (float)camera.pixelHeight);
		return result;
	}

	public void ConfigureJitteredProjectionMatrix(PostProcessRenderContext context)
	{
		Camera camera = context.camera;
		camera.nonJitteredProjectionMatrix = camera.projectionMatrix;
		camera.projectionMatrix = GetJitteredProjectionMatrix(camera);
		camera.useJitteredProjectionMatrixForTransparentRendering = true;
	}

	public void ConfigureStereoJitteredProjectionMatrices(PostProcessRenderContext context)
	{
		Camera camera = context.camera;
		jitter = GenerateRandomOffset();
		jitter *= jitterSpread;
		for (Camera.StereoscopicEye stereoscopicEye = Camera.StereoscopicEye.Left; stereoscopicEye <= Camera.StereoscopicEye.Right; stereoscopicEye++)
		{
			context.camera.CopyStereoDeviceProjectionMatrixToNonJittered(stereoscopicEye);
			Matrix4x4 stereoNonJitteredProjectionMatrix = context.camera.GetStereoNonJitteredProjectionMatrix(stereoscopicEye);
			Matrix4x4 matrix = RuntimeUtilities.GenerateJitteredProjectionMatrixFromOriginal(context, stereoNonJitteredProjectionMatrix, jitter);
			context.camera.SetStereoProjectionMatrix(stereoscopicEye, matrix);
		}
		jitter = new Vector2(jitter.x / (float)context.screenWidth, jitter.y / (float)context.screenHeight);
		camera.useJitteredProjectionMatrixForTransparentRendering = true;
	}

	private void GenerateHistoryName(RenderTexture rt, int id, PostProcessRenderContext context)
	{
		rt.name = "Temporal Anti-aliasing History id #" + id;
		if (context.stereoActive)
		{
			rt.name = rt.name + " for eye " + context.xrActiveEye;
		}
	}

	private RenderTexture CheckHistory(int id, PostProcessRenderContext context)
	{
		int xrActiveEye = context.xrActiveEye;
		if (m_HistoryTextures[xrActiveEye] == null)
		{
			m_HistoryTextures[xrActiveEye] = new RenderTexture[2];
		}
		RenderTexture renderTexture = m_HistoryTextures[xrActiveEye][id];
		if (m_ResetHistory || renderTexture == null || !renderTexture.IsCreated())
		{
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = context.GetScreenSpaceTemporaryRT(0, context.sourceFormat);
			GenerateHistoryName(renderTexture, id, context);
			renderTexture.filterMode = FilterMode.Bilinear;
			m_HistoryTextures[xrActiveEye][id] = renderTexture;
			RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, renderTexture);
		}
		else if (renderTexture.width != context.width || renderTexture.height != context.height)
		{
			RenderTexture screenSpaceTemporaryRT = context.GetScreenSpaceTemporaryRT(0, context.sourceFormat);
			GenerateHistoryName(screenSpaceTemporaryRT, id, context);
			screenSpaceTemporaryRT.filterMode = FilterMode.Bilinear;
			m_HistoryTextures[xrActiveEye][id] = screenSpaceTemporaryRT;
			RuntimeUtilities.BlitFullscreenTriangle(context.command, renderTexture, screenSpaceTemporaryRT);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		return m_HistoryTextures[xrActiveEye][id];
	}

	internal void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.temporalAntialiasing);
		CommandBuffer command = context.command;
		command.BeginSample("TemporalAntialiasing");
		int num = m_HistoryPingPong[context.xrActiveEye];
		RenderTexture value = CheckHistory(++num % 2, context);
		RenderTexture renderTexture = CheckHistory(++num % 2, context);
		m_HistoryPingPong[context.xrActiveEye] = ++num % 2;
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.Jitter, jitter);
		propertySheet.properties.SetFloat(UnityEngine.Rendering.PostProcessing.ShaderIDs.Sharpness, sharpness);
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.FinalBlendParameters, new Vector4(stationaryBlending, motionBlending, 6000f, 0f));
		propertySheet.properties.SetTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.HistoryTex, value);
		int pass = (context.camera.orthographic ? 1 : 0);
		m_Mrt[0] = context.destination;
		m_Mrt[1] = renderTexture;
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, m_Mrt, context.source, propertySheet, pass);
		command.EndSample("TemporalAntialiasing");
		m_ResetHistory = false;
	}

	internal void Release()
	{
		if (m_HistoryTextures != null)
		{
			for (int i = 0; i < m_HistoryTextures.Length; i++)
			{
				if (m_HistoryTextures[i] != null)
				{
					for (int j = 0; j < m_HistoryTextures[i].Length; j++)
					{
						RenderTexture.ReleaseTemporary(m_HistoryTextures[i][j]);
						m_HistoryTextures[i][j] = null;
					}
					m_HistoryTextures[i] = null;
				}
			}
		}
		sampleIndex = 0;
		m_HistoryPingPong[0] = 0;
		m_HistoryPingPong[1] = 0;
		ResetHistory();
	}
}
