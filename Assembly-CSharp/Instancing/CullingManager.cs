using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Instancing;

public class CullingManager
{
	public NativeArray<RenderSlice> RenderSlicesArray;

	public GPUBuffer<RenderSlice> RenderSlicesBuffer;

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
		int length = 512;
		RenderSlicesArray = new NativeArray<RenderSlice>(length, Allocator.Persistent);
		RenderSlicesBuffer = new GPUBuffer<RenderSlice>(length, GPUBuffer.Target.Structured);
	}

	private void FreeNativeMemory()
	{
		NativeArrayEx.SafeDispose(ref RenderSlicesArray);
		RenderSlicesBuffer?.Dispose();
		RenderSlicesBuffer = null;
	}

	public void EnsureCapacity(int rendererCount)
	{
		if (RenderSlicesArray.Length < rendererCount + 1)
		{
			int newCapacity = Mathf.ClosestPowerOfTwo(rendererCount) * 2;
			NativeArrayEx.Expand(ref RenderSlicesArray, newCapacity);
			RenderSlicesBuffer.Expand(newCapacity);
			RenderSlicesBuffer.SetData(RenderSlicesArray);
		}
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		builder.AppendLine("### CullingManager ###");
		builder.MemoryUsage("PostCullInstanceCounts", RenderSlicesArray);
	}

	public void UpdateComputeBuffers()
	{
		RenderSlicesBuffer.SetData(RenderSlicesArray);
	}
}
