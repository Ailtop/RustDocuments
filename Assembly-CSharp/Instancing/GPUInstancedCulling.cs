using System;
using ConVar;
using Unity.Mathematics;
using UnityEngine;

namespace Instancing;

public class GPUInstancedCulling
{
	public struct CullingParameters
	{
		public ComputeShader cullShader;

		public ComputeShader postCullShader;

		public int shadowCameras;

		public Vector3 cameraPosition;

		public float maxDistance;

		public float distanceScale;

		public GPUBuffer<float4> frustumPlanes;

		public bool frustumCulling;

		public GPUBuffer<InstancedCullData> inputBuffer;

		public int inputLength;

		public GPUBuffer<DrawCallJobData> drawCalls;

		public int drawCallCount;

		public GPUBuffer<RenderSlice> renderSlices;

		public int sliceCount;

		public GPUBuffer<uint> renderBuffer;

		public GPUBuffer<uint> indirectArgs;

		public GPUBuffer<uint> indirectExtraArgs;
	}

	private static readonly int Param_InputBuffer = Shader.PropertyToID("_InputBuffer");

	private static readonly int Param_OutputBuffer = Shader.PropertyToID("_OutputBuffer");

	private static readonly int Param_ComputeBuffer = Shader.PropertyToID("_ComputeBuffer");

	private static readonly int Param_IndirectArgs = Shader.PropertyToID("_IndirectArgs");

	private static readonly int Param_IndirectExtraArgs = Shader.PropertyToID("_IndirectExtraArgs");

	private static readonly int Param_RenderSlices = Shader.PropertyToID("_RenderSlices");

	private static readonly int Param_SliceCounts = Shader.PropertyToID("_SliceCounts");

	private static readonly int Param_FrustumPlanes = Shader.PropertyToID("_FrustumPlanes");

	private static readonly int Param_ShadowFrustumCount = Shader.PropertyToID("_ShadowFrustumCount");

	private static readonly int Param_CameraPosition = Shader.PropertyToID("_CameraPosition");

	private static readonly int Param_MaxDistance = Shader.PropertyToID("_MaxDistance");

	private static readonly int Param_DistanceScale = Shader.PropertyToID("_DistanceScale");

	private static readonly int Param_SliceCount = Shader.PropertyToID("_SliceCount");

	private static readonly int Param_Length = Shader.PropertyToID("_Length");

	private static readonly int Param_DrawCalls = Shader.PropertyToID("_DrawCalls");

	private static readonly int Param_DrawCallCount = Shader.PropertyToID("_DrawCallCount");

	private static readonly int Param_Multidraw_Enabled = Shader.PropertyToID("_Multidraw_Enabled");

	private static readonly int Param_FrustumCullingEnabled = Shader.PropertyToID("_FrustumCullingEnabled");

	private GPUBuffer<uint> tempBuffer;

	private GPUBuffer<uint> postCullMeshCounts;

	public void Initialize()
	{
		AllocateNativeMemory();
	}

	public void OnDestroy()
	{
		FreeNativeMemory();
	}

	private void AllocateNativeMemory()
	{
		tempBuffer = new GPUBuffer<uint>(1024, GPUBuffer.Target.Structured);
		postCullMeshCounts = new GPUBuffer<uint>(1024, GPUBuffer.Target.Structured);
	}

	private void FreeNativeMemory()
	{
		tempBuffer?.Dispose();
		tempBuffer = null;
		postCullMeshCounts?.Dispose();
		postCullMeshCounts = null;
	}

	public void SimpleCulling(CullingParameters options)
	{
		if (options.renderSlices.count < options.sliceCount + 1)
		{
			throw new ArgumentException("SliceIndexes must be at least as large as slice count + 1!");
		}
		if (tempBuffer.count < options.inputBuffer.count)
		{
			tempBuffer.Expand(options.inputBuffer.count);
		}
		if (postCullMeshCounts.count < options.renderSlices.count)
		{
			postCullMeshCounts.Expand(options.renderSlices.count);
		}
		ClearBuffer(SingletonComponent<InstancedScheduler>.Instance.ClearBufferShader, tempBuffer);
		ClearBuffer(SingletonComponent<InstancedScheduler>.Instance.ClearBufferShader, postCullMeshCounts);
		ClearBuffer(SingletonComponent<InstancedScheduler>.Instance.ClearBufferShader, options.renderBuffer);
		ClearBuffer(SingletonComponent<InstancedScheduler>.Instance.ClearBufferShader, options.indirectArgs);
		ClearBuffer(SingletonComponent<InstancedScheduler>.Instance.ClearBufferShader, options.indirectExtraArgs);
		CullingShader(options.cullShader, options.inputBuffer, options.inputLength, tempBuffer, options.renderSlices, options.frustumPlanes, options.shadowCameras, options.cameraPosition, options.maxDistance, options.distanceScale, options.frustumCulling);
		SimplePostCull(options.postCullShader, tempBuffer, options.renderBuffer, options.renderSlices, options.sliceCount, postCullMeshCounts);
		WriteIndirectArgs(SingletonComponent<InstancedScheduler>.Instance.WriteIndirectArgsShader, postCullMeshCounts, options.renderSlices, options.drawCalls, options.drawCallCount, options.indirectArgs, options.indirectExtraArgs);
	}

	private static void CullingShader(ComputeShader shader, GPUBuffer<InstancedCullData> inputBuffer, int inputLength, GPUBuffer<uint> outputBuffer, GPUBuffer<RenderSlice> renderSlices, GPUBuffer<float4> frustumPlanes, int shadowCameras, Vector3 cameraPosition, float maxDistance, float distanceScale, bool frustumCulling)
	{
		if (outputBuffer.stride != 4)
		{
			throw new ArgumentException($"Cull output buffer must have a stride of 4 ({outputBuffer.stride})!");
		}
		int iterationCount = GetIterationCount(inputLength, 1024);
		if (iterationCount != 0)
		{
			int num = shader.FindKernel("CullingKernel");
			ComputeBufferEx.SetBuffer(shader, num, Param_InputBuffer, inputBuffer);
			ComputeBufferEx.SetBuffer(shader, num, Param_OutputBuffer, outputBuffer);
			ComputeBufferEx.SetBuffer(shader, num, Param_RenderSlices, renderSlices);
			ComputeBufferEx.SetBuffer(shader, num, Param_FrustumPlanes, frustumPlanes);
			shader.SetVector(Param_CameraPosition, cameraPosition);
			shader.SetFloat(Param_MaxDistance, maxDistance);
			shader.SetFloat(Param_DistanceScale, distanceScale);
			shader.SetFloat(Param_FrustumCullingEnabled, frustumCulling ? 1f : 0f);
			shader.SetInt(Param_ShadowFrustumCount, shadowCameras);
			shader.Dispatch(num, iterationCount, 1, 1);
		}
	}

	private static void SimplePostCull(ComputeShader shader, GPUBuffer<uint> inputBuffer, GPUBuffer<uint> outputBuffer, GPUBuffer<RenderSlice> renderSlices, int sliceCount, GPUBuffer<uint> sliceCounts)
	{
		int iterationCount = GetIterationCount(sliceCount, 1024);
		if (iterationCount != 0)
		{
			int num = shader.FindKernel("SimplePostCull");
			ComputeBufferEx.SetBuffer(shader, num, Param_InputBuffer, inputBuffer);
			ComputeBufferEx.SetBuffer(shader, num, Param_OutputBuffer, outputBuffer);
			ComputeBufferEx.SetBuffer(shader, num, Param_RenderSlices, renderSlices);
			ComputeBufferEx.SetBuffer(shader, num, Param_SliceCounts, sliceCounts);
			shader.SetInt(Param_SliceCount, (Render.max_renderers > 0) ? Render.max_renderers : sliceCount);
			shader.Dispatch(num, iterationCount, 1, 1);
		}
	}

	private static void WriteIndirectArgs(ComputeShader shader, GPUBuffer<uint> sliceCounts, GPUBuffer<RenderSlice> renderSlices, GPUBuffer<DrawCallJobData> drawCalls, int drawCallCount, GPUBuffer<uint> indirectArgs, GPUBuffer<uint> indirectExtraArgs)
	{
		int iterationCount = GetIterationCount(drawCallCount, 1024);
		if (iterationCount != 0)
		{
			int num = shader.FindKernel("WriteIndirectArgs");
			ComputeBufferEx.SetBuffer(shader, num, Param_SliceCounts, sliceCounts);
			ComputeBufferEx.SetBuffer(shader, num, Param_RenderSlices, renderSlices);
			ComputeBufferEx.SetBuffer(shader, num, Param_DrawCalls, drawCalls);
			shader.SetInt(Param_DrawCallCount, drawCallCount);
			ComputeBufferEx.SetBuffer(shader, num, Param_IndirectArgs, indirectArgs);
			ComputeBufferEx.SetBuffer(shader, num, Param_IndirectExtraArgs, indirectExtraArgs);
			shader.SetInt(Param_Multidraw_Enabled, Render.IsMultidrawEnabled ? 1 : 0);
			shader.Dispatch(num, iterationCount, 1, 1);
		}
	}

	private static void ClearBuffer(ComputeShader shader, GPUBuffer<uint> buffer)
	{
		int count = buffer.count;
		int iterationCount = GetIterationCount(count, 1024);
		int num = shader.FindKernel("ClearBufferKernel");
		ComputeBufferEx.SetBuffer(shader, num, Param_ComputeBuffer, buffer);
		shader.SetInt(Param_Length, count);
		shader.Dispatch(num, iterationCount, 1, 1);
	}

	private static int GetIterationCount(int count, int threads)
	{
		return count / threads + ((count % threads != 0) ? 1 : 0);
	}
}
