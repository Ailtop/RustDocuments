using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Instancing;

public class MeshGridManager
{
	private struct MeshGridKey
	{
		public int SpatialId;

		public bool IsShadow;
	}

	private class GridAllocationInfo
	{
		public List<int> Cells = new List<int>();
	}

	public NativeArray<GridJobData> Grids;

	private float HalfWorldSize;

	private float GridSize;

	private const int GridCount = 32;

	private const int normalGridCount = 1024;

	private const int shadowGridCount = 1024;

	private const int outOfBoundsGrid = 2048;

	private const int lastGridId = 2048;

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
		Grids = new NativeArray<GridJobData>(2049, Allocator.Persistent);
	}

	private void FreeNativeMemory()
	{
		NativeArrayEx.SafeDispose(ref Grids);
	}

	public int GetPartitionKey(float3 position, bool hasShadow)
	{
		int num = GetGridId(position);
		if (hasShadow && num != 2048)
		{
			num += 1024;
		}
		return num;
	}

	public void SetWorldSize(float worldSize)
	{
		GridSize = worldSize / 32f;
		HalfWorldSize = worldSize / 2f;
		UpdateGridBounds();
	}

	private void UpdateGridBounds()
	{
		for (int i = 0; i < Grids.Length; i++)
		{
			GridJobData gridJobData = Grids[i];
			gridJobData.GridId = i;
			if (i < 1024)
			{
				gridJobData.CanBeFrustumCulled = true;
			}
			if (i < 2048)
			{
				Bounds gridBounds = GetGridBounds(i);
				gridJobData.CanBeDistanceCulled = true;
				gridJobData.MinBounds = gridBounds.min;
				gridJobData.MaxBounds = gridBounds.max;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetGridId(float3 point)
	{
		int num = (int)((point.x + HalfWorldSize) / GridSize);
		int num2 = (int)((point.z + HalfWorldSize) / GridSize);
		if (num < 0 || num2 < 0 || num >= 32 || num2 >= 32)
		{
			return 2048;
		}
		return num + num2 * 32;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float3 GetGridCenter(int gridId)
	{
		if (gridId >= 1024 && gridId < 2048)
		{
			gridId -= 1024;
		}
		float num = (float)(gridId % 32) * GridSize - HalfWorldSize;
		return new float3(z: (float)(gridId / 32) * GridSize - HalfWorldSize + GridSize / 2f, x: num + GridSize / 2f, y: 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Bounds GetGridBounds(int gridId)
	{
		float3 gridCenter = GetGridCenter(gridId);
		return new Bounds(gridCenter, new Vector3(GridSize, 1000f, GridSize));
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		builder.AppendLine("### GridManager ###");
		builder.MemoryUsage("Grids", Grids, Grids.Length);
	}
}
