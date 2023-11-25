using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Instancing;

[BurstCompile]
public struct PreCullingJob : IJob
{
	[ReadOnly]
	public NativeArray<InstancedRendererJobData> Meshes;

	[ReadOnly]
	public int RendererCount;

	[ReadOnly]
	public NativeArray<uint> CountPerMesh;

	[ReadOnly]
	public NativeArray<DrawCallJobData> DrawCalls;

	[ReadOnly]
	public int DrawCallCount;

	[WriteOnly]
	public NativeArray<RenderSlice> RenderSlices;

	public void Execute()
	{
		CalculateRenderSlices();
	}

	private void CalculateRenderSlices()
	{
		uint num = 0u;
		for (int i = 0; i < RendererCount; i++)
		{
			uint num2 = CountPerMesh[i];
			uint num3 = num2;
			if (num3 != 0)
			{
				RenderSlices[i] = new RenderSlice
				{
					StartIndex = num,
					Length = num2
				};
				num += num3;
			}
			else
			{
				RenderSlices[i] = default(RenderSlice);
			}
		}
	}
}
