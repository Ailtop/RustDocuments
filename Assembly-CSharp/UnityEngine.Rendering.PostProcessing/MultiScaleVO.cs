using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Serializable]
[Preserve]
internal sealed class MultiScaleVO : UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod
{
	internal enum MipLevel
	{
		Original = 0,
		L1 = 1,
		L2 = 2,
		L3 = 3,
		L4 = 4,
		L5 = 5,
		L6 = 6
	}

	private enum Pass
	{
		DepthCopy = 0,
		CompositionDeferred = 1,
		CompositionForward = 2,
		DebugOverlay = 3
	}

	private readonly float[] m_SampleThickness = new float[12]
	{
		Mathf.Sqrt(0.96f),
		Mathf.Sqrt(0.84f),
		Mathf.Sqrt(0.64f),
		Mathf.Sqrt(0.35999995f),
		Mathf.Sqrt(0.91999996f),
		Mathf.Sqrt(0.79999995f),
		Mathf.Sqrt(0.59999996f),
		Mathf.Sqrt(0.31999993f),
		Mathf.Sqrt(0.67999995f),
		Mathf.Sqrt(0.47999996f),
		Mathf.Sqrt(0.19999993f),
		Mathf.Sqrt(0.27999997f)
	};

	private readonly float[] m_InvThicknessTable = new float[12];

	private readonly float[] m_SampleWeightTable = new float[12];

	private readonly int[] m_Widths = new int[7];

	private readonly int[] m_Heights = new int[7];

	private AmbientOcclusion m_Settings;

	private PropertySheet m_PropertySheet;

	private PostProcessResources m_Resources;

	private RenderTexture m_AmbientOnlyAO;

	private readonly RenderTargetIdentifier[] m_MRT = new RenderTargetIdentifier[2]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.CameraTarget
	};

	public MultiScaleVO(AmbientOcclusion settings)
	{
		m_Settings = settings;
	}

	public DepthTextureMode GetCameraFlags()
	{
		return DepthTextureMode.Depth;
	}

	public void SetResources(PostProcessResources resources)
	{
		m_Resources = resources;
	}

	private void Alloc(CommandBuffer cmd, int id, MipLevel size, RenderTextureFormat format, bool uav)
	{
		cmd.GetTemporaryRT(id, new RenderTextureDescriptor
		{
			width = m_Widths[(int)size],
			height = m_Heights[(int)size],
			colorFormat = format,
			depthBufferBits = 0,
			volumeDepth = 1,
			autoGenerateMips = false,
			msaaSamples = 1,
			enableRandomWrite = uav,
			dimension = TextureDimension.Tex2D,
			sRGB = false
		}, FilterMode.Point);
	}

	private void AllocArray(CommandBuffer cmd, int id, MipLevel size, RenderTextureFormat format, bool uav)
	{
		cmd.GetTemporaryRT(id, new RenderTextureDescriptor
		{
			width = m_Widths[(int)size],
			height = m_Heights[(int)size],
			colorFormat = format,
			depthBufferBits = 0,
			volumeDepth = 16,
			autoGenerateMips = false,
			msaaSamples = 1,
			enableRandomWrite = uav,
			dimension = TextureDimension.Tex2DArray,
			sRGB = false
		}, FilterMode.Point);
	}

	private void Release(CommandBuffer cmd, int id)
	{
		cmd.ReleaseTemporaryRT(id);
	}

	private Vector4 CalculateZBufferParams(Camera camera)
	{
		float num = camera.farClipPlane / camera.nearClipPlane;
		if (SystemInfo.usesReversedZBuffer)
		{
			return new Vector4(num - 1f, 1f, 0f, 0f);
		}
		return new Vector4(1f - num, num, 0f, 0f);
	}

	private float CalculateTanHalfFovHeight(Camera camera)
	{
		return 1f / camera.projectionMatrix[0, 0];
	}

	private Vector2 GetSize(MipLevel mip)
	{
		return new Vector2(m_Widths[(int)mip], m_Heights[(int)mip]);
	}

	private Vector3 GetSizeArray(MipLevel mip)
	{
		return new Vector3(m_Widths[(int)mip], m_Heights[(int)mip], 16f);
	}

	public void GenerateAOMap(CommandBuffer cmd, Camera camera, RenderTargetIdentifier destination, RenderTargetIdentifier? depthMap, bool invert, bool isMSAA)
	{
		m_Widths[0] = camera.pixelWidth * ((!RuntimeUtilities.isSinglePassStereoEnabled) ? 1 : 2);
		m_Heights[0] = camera.pixelHeight;
		for (int i = 1; i < 7; i++)
		{
			int num = 1 << i;
			m_Widths[i] = (m_Widths[0] + (num - 1)) / num;
			m_Heights[i] = (m_Heights[0] + (num - 1)) / num;
		}
		PushAllocCommands(cmd, isMSAA);
		PushDownsampleCommands(cmd, camera, depthMap, isMSAA);
		float tanHalfFovH = CalculateTanHalfFovHeight(camera);
		PushRenderCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth1, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion1, GetSizeArray(MipLevel.L3), tanHalfFovH, isMSAA);
		PushRenderCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth2, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion2, GetSizeArray(MipLevel.L4), tanHalfFovH, isMSAA);
		PushRenderCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth3, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion3, GetSizeArray(MipLevel.L5), tanHalfFovH, isMSAA);
		PushRenderCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth4, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion4, GetSizeArray(MipLevel.L6), tanHalfFovH, isMSAA);
		PushUpsampleCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth4, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion4, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion3, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined3, GetSize(MipLevel.L4), GetSize(MipLevel.L3), isMSAA);
		PushUpsampleCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined3, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion2, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined2, GetSize(MipLevel.L3), GetSize(MipLevel.L2), isMSAA);
		PushUpsampleCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined2, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion1, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined1, GetSize(MipLevel.L2), GetSize(MipLevel.L1), isMSAA);
		PushUpsampleCommands(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined1, UnityEngine.Rendering.PostProcessing.ShaderIDs.LinearDepth, null, destination, GetSize(MipLevel.L1), GetSize(MipLevel.Original), isMSAA, invert);
		PushReleaseCommands(cmd);
	}

	private void PushAllocCommands(CommandBuffer cmd, bool isMSAA)
	{
		if (isMSAA)
		{
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LinearDepth, MipLevel.Original, RenderTextureFormat.RGHalf, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1, MipLevel.L1, RenderTextureFormat.RGFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2, MipLevel.L2, RenderTextureFormat.RGFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3, MipLevel.L3, RenderTextureFormat.RGFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth4, MipLevel.L4, RenderTextureFormat.RGFloat, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth1, MipLevel.L3, RenderTextureFormat.RGHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth2, MipLevel.L4, RenderTextureFormat.RGHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth3, MipLevel.L5, RenderTextureFormat.RGHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth4, MipLevel.L6, RenderTextureFormat.RGHalf, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion1, MipLevel.L1, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion2, MipLevel.L2, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion3, MipLevel.L3, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion4, MipLevel.L4, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined1, MipLevel.L1, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined2, MipLevel.L2, RenderTextureFormat.RG16, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined3, MipLevel.L3, RenderTextureFormat.RG16, uav: true);
		}
		else
		{
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LinearDepth, MipLevel.Original, RenderTextureFormat.RHalf, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1, MipLevel.L1, RenderTextureFormat.RFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2, MipLevel.L2, RenderTextureFormat.RFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3, MipLevel.L3, RenderTextureFormat.RFloat, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth4, MipLevel.L4, RenderTextureFormat.RFloat, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth1, MipLevel.L3, RenderTextureFormat.RHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth2, MipLevel.L4, RenderTextureFormat.RHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth3, MipLevel.L5, RenderTextureFormat.RHalf, uav: true);
			AllocArray(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth4, MipLevel.L6, RenderTextureFormat.RHalf, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion1, MipLevel.L1, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion2, MipLevel.L2, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion3, MipLevel.L3, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion4, MipLevel.L4, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined1, MipLevel.L1, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined2, MipLevel.L2, RenderTextureFormat.R8, uav: true);
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined3, MipLevel.L3, RenderTextureFormat.R8, uav: true);
		}
	}

	private void PushDownsampleCommands(CommandBuffer cmd, Camera camera, RenderTargetIdentifier? depthMap, bool isMSAA)
	{
		bool flag = false;
		RenderTargetIdentifier renderTargetIdentifier;
		if (depthMap.HasValue)
		{
			renderTargetIdentifier = depthMap.Value;
		}
		else if (!RuntimeUtilities.IsResolvedDepthAvailable(camera))
		{
			Alloc(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.DepthCopy, MipLevel.Original, RenderTextureFormat.RFloat, uav: false);
			renderTargetIdentifier = new RenderTargetIdentifier(UnityEngine.Rendering.PostProcessing.ShaderIDs.DepthCopy);
			RuntimeUtilities.BlitFullscreenTriangle(cmd, BuiltinRenderTextureType.None, renderTargetIdentifier, m_PropertySheet, 0);
			flag = true;
		}
		else
		{
			renderTargetIdentifier = BuiltinRenderTextureType.ResolvedDepth;
		}
		ComputeShader multiScaleAODownsample = m_Resources.computeShaders.multiScaleAODownsample1;
		int kernelIndex = multiScaleAODownsample.FindKernel(isMSAA ? "MultiScaleVODownsample1_MSAA" : "MultiScaleVODownsample1");
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "LinearZ", UnityEngine.Rendering.PostProcessing.ShaderIDs.LinearDepth);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS2x", UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS4x", UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS2xAtlas", UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth1);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS4xAtlas", UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth2);
		cmd.SetComputeVectorParam(multiScaleAODownsample, "ZBufferParams", CalculateZBufferParams(camera));
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "Depth", renderTargetIdentifier);
		cmd.DispatchCompute(multiScaleAODownsample, kernelIndex, m_Widths[4], m_Heights[4], 1);
		if (flag)
		{
			Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.DepthCopy);
		}
		multiScaleAODownsample = m_Resources.computeShaders.multiScaleAODownsample2;
		kernelIndex = (isMSAA ? multiScaleAODownsample.FindKernel("MultiScaleVODownsample2_MSAA") : multiScaleAODownsample.FindKernel("MultiScaleVODownsample2"));
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS4x", UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS8x", UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS16x", UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth4);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS8xAtlas", UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth3);
		cmd.SetComputeTextureParam(multiScaleAODownsample, kernelIndex, "DS16xAtlas", UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth4);
		cmd.DispatchCompute(multiScaleAODownsample, kernelIndex, m_Widths[6], m_Heights[6], 1);
	}

	private void PushRenderCommands(CommandBuffer cmd, int source, int destination, Vector3 sourceSize, float tanHalfFovH, bool isMSAA)
	{
		float num = 2f * tanHalfFovH * 10f / sourceSize.x;
		if (RuntimeUtilities.isSinglePassStereoEnabled)
		{
			num *= 2f;
		}
		float num2 = 1f / num;
		for (int i = 0; i < 12; i++)
		{
			m_InvThicknessTable[i] = num2 / m_SampleThickness[i];
		}
		m_SampleWeightTable[0] = 4f * m_SampleThickness[0];
		m_SampleWeightTable[1] = 4f * m_SampleThickness[1];
		m_SampleWeightTable[2] = 4f * m_SampleThickness[2];
		m_SampleWeightTable[3] = 4f * m_SampleThickness[3];
		m_SampleWeightTable[4] = 4f * m_SampleThickness[4];
		m_SampleWeightTable[5] = 8f * m_SampleThickness[5];
		m_SampleWeightTable[6] = 8f * m_SampleThickness[6];
		m_SampleWeightTable[7] = 8f * m_SampleThickness[7];
		m_SampleWeightTable[8] = 4f * m_SampleThickness[8];
		m_SampleWeightTable[9] = 8f * m_SampleThickness[9];
		m_SampleWeightTable[10] = 8f * m_SampleThickness[10];
		m_SampleWeightTable[11] = 4f * m_SampleThickness[11];
		m_SampleWeightTable[0] = 0f;
		m_SampleWeightTable[2] = 0f;
		m_SampleWeightTable[5] = 0f;
		m_SampleWeightTable[7] = 0f;
		m_SampleWeightTable[9] = 0f;
		float num3 = 0f;
		float[] sampleWeightTable = m_SampleWeightTable;
		foreach (float num4 in sampleWeightTable)
		{
			num3 += num4;
		}
		for (int k = 0; k < m_SampleWeightTable.Length; k++)
		{
			m_SampleWeightTable[k] /= num3;
		}
		ComputeShader multiScaleAORender = m_Resources.computeShaders.multiScaleAORender;
		int kernelIndex = (isMSAA ? multiScaleAORender.FindKernel("MultiScaleVORender_MSAA_interleaved") : multiScaleAORender.FindKernel("MultiScaleVORender_interleaved"));
		cmd.SetComputeFloatParams(multiScaleAORender, "gInvThicknessTable", m_InvThicknessTable);
		cmd.SetComputeFloatParams(multiScaleAORender, "gSampleWeightTable", m_SampleWeightTable);
		cmd.SetComputeVectorParam(multiScaleAORender, "gInvSliceDimension", new Vector2(1f / sourceSize.x, 1f / sourceSize.y));
		cmd.SetComputeVectorParam(multiScaleAORender, "AdditionalParams", new Vector2(-1f / m_Settings.thicknessModifier.value, m_Settings.intensity.value));
		cmd.SetComputeTextureParam(multiScaleAORender, kernelIndex, "DepthTex", source);
		cmd.SetComputeTextureParam(multiScaleAORender, kernelIndex, "Occlusion", destination);
		multiScaleAORender.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
		cmd.DispatchCompute(multiScaleAORender, kernelIndex, ((int)sourceSize.x + (int)x - 1) / (int)x, ((int)sourceSize.y + (int)y - 1) / (int)y, ((int)sourceSize.z + (int)z - 1) / (int)z);
	}

	private void PushUpsampleCommands(CommandBuffer cmd, int lowResDepth, int interleavedAO, int highResDepth, int? highResAO, RenderTargetIdentifier dest, Vector3 lowResDepthSize, Vector2 highResDepthSize, bool isMSAA, bool invert = false)
	{
		ComputeShader multiScaleAOUpsample = m_Resources.computeShaders.multiScaleAOUpsample;
		int num = 0;
		num = (isMSAA ? multiScaleAOUpsample.FindKernel(highResAO.HasValue ? "MultiScaleVOUpSample_MSAA_blendout" : (invert ? "MultiScaleVOUpSample_MSAA_invert" : "MultiScaleVOUpSample_MSAA")) : multiScaleAOUpsample.FindKernel(highResAO.HasValue ? "MultiScaleVOUpSample_blendout" : (invert ? "MultiScaleVOUpSample_invert" : "MultiScaleVOUpSample")));
		float num2 = 1920f / lowResDepthSize.x;
		float num3 = 1f - Mathf.Pow(10f, m_Settings.blurTolerance.value) * num2;
		num3 *= num3;
		float num4 = Mathf.Pow(10f, m_Settings.upsampleTolerance.value);
		float x = 1f / (Mathf.Pow(10f, m_Settings.noiseFilterTolerance.value) + num4);
		cmd.SetComputeVectorParam(multiScaleAOUpsample, "InvLowResolution", new Vector2(1f / lowResDepthSize.x, 1f / lowResDepthSize.y));
		cmd.SetComputeVectorParam(multiScaleAOUpsample, "InvHighResolution", new Vector2(1f / highResDepthSize.x, 1f / highResDepthSize.y));
		cmd.SetComputeVectorParam(multiScaleAOUpsample, "AdditionalParams", new Vector4(x, num2, num3, num4));
		cmd.SetComputeTextureParam(multiScaleAOUpsample, num, "LoResDB", lowResDepth);
		cmd.SetComputeTextureParam(multiScaleAOUpsample, num, "HiResDB", highResDepth);
		cmd.SetComputeTextureParam(multiScaleAOUpsample, num, "LoResAO1", interleavedAO);
		if (highResAO.HasValue)
		{
			cmd.SetComputeTextureParam(multiScaleAOUpsample, num, "HiResAO", highResAO.Value);
		}
		cmd.SetComputeTextureParam(multiScaleAOUpsample, num, "AoResult", dest);
		int threadGroupsX = ((int)highResDepthSize.x + 17) / 16;
		int threadGroupsY = ((int)highResDepthSize.y + 17) / 16;
		cmd.DispatchCompute(multiScaleAOUpsample, num, threadGroupsX, threadGroupsY, 1);
	}

	private void PushReleaseCommands(CommandBuffer cmd)
	{
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LinearDepth);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth1);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth2);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth3);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.LowDepth4);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth1);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth2);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth3);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.TiledDepth4);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion1);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion2);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion3);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Occlusion4);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined1);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined2);
		Release(cmd, UnityEngine.Rendering.PostProcessing.ShaderIDs.Combined3);
	}

	private void PreparePropertySheet(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(m_Resources.shaders.multiScaleAO);
		propertySheet.ClearKeywords();
		propertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.AOColor, Color.white - m_Settings.color.value);
		m_PropertySheet = propertySheet;
	}

	private void CheckAOTexture(PostProcessRenderContext context)
	{
		if (m_AmbientOnlyAO == null || !m_AmbientOnlyAO.IsCreated() || m_AmbientOnlyAO.width != context.width || m_AmbientOnlyAO.height != context.height)
		{
			RuntimeUtilities.Destroy(m_AmbientOnlyAO);
			m_AmbientOnlyAO = new RenderTexture(context.width, context.height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
			{
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Point,
				enableRandomWrite = true
			};
			m_AmbientOnlyAO.Create();
		}
	}

	private void PushDebug(PostProcessRenderContext context)
	{
		if (context.IsDebugOverlayEnabled(DebugOverlay.AmbientOcclusion))
		{
			context.PushDebugOverlay(context.command, m_AmbientOnlyAO, m_PropertySheet, 3);
		}
	}

	public void RenderAfterOpaque(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion");
		SetResources(context.resources);
		PreparePropertySheet(context);
		CheckAOTexture(context);
		if (context.camera.actualRenderingPath == RenderingPath.Forward && RenderSettings.fog)
		{
			m_PropertySheet.EnableKeyword("APPLY_FORWARD_FOG");
			m_PropertySheet.properties.SetVector(UnityEngine.Rendering.PostProcessing.ShaderIDs.FogParams, new Vector3(RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance));
		}
		GenerateAOMap(command, context.camera, m_AmbientOnlyAO, null, invert: false, isMSAA: false);
		PushDebug(context);
		command.SetGlobalTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.MSVOcclusionTexture, m_AmbientOnlyAO);
		RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget, m_PropertySheet, 2, RenderBufferLoadAction.Load);
		command.EndSample("Ambient Occlusion");
	}

	public void RenderAmbientOnly(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion Render");
		SetResources(context.resources);
		PreparePropertySheet(context);
		CheckAOTexture(context);
		GenerateAOMap(command, context.camera, m_AmbientOnlyAO, null, invert: false, isMSAA: false);
		PushDebug(context);
		command.EndSample("Ambient Occlusion Render");
	}

	public void CompositeAmbientOnly(PostProcessRenderContext context)
	{
		CommandBuffer command = context.command;
		command.BeginSample("Ambient Occlusion Composite");
		command.SetGlobalTexture(UnityEngine.Rendering.PostProcessing.ShaderIDs.MSVOcclusionTexture, m_AmbientOnlyAO);
		RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, m_MRT, BuiltinRenderTextureType.CameraTarget, m_PropertySheet, 1);
		command.EndSample("Ambient Occlusion Composite");
	}

	public void Release()
	{
		RuntimeUtilities.Destroy(m_AmbientOnlyAO);
		m_AmbientOnlyAO = null;
	}
}
