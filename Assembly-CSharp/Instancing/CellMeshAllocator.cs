using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConVar;
using Facepunch;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Instancing;

public class CellMeshAllocator
{
	private class CellPartition : Facepunch.Pool.IPooled
	{
		public List<CellId> PackedCells;

		public List<CellId> CellsWithSpace;

		public bool IsEmpty()
		{
			if (CollectionEx.IsNullOrEmpty(PackedCells))
			{
				return CollectionEx.IsNullOrEmpty(CellsWithSpace);
			}
			return false;
		}

		public void AddPackedCell(CellId cell)
		{
			if (PackedCells == null)
			{
				PackedCells = Facepunch.Pool.GetList<CellId>();
			}
			PackedCells.Add(cell);
		}

		public void RemovePackedCell(CellId cell)
		{
			if (PackedCells != null)
			{
				PackedCells.Remove(cell);
				if (PackedCells.Count == 0)
				{
					Facepunch.Pool.FreeList(ref PackedCells);
				}
			}
		}

		public void AddCellWithSpace(CellId cell)
		{
			if (CellsWithSpace == null)
			{
				CellsWithSpace = Facepunch.Pool.GetList<CellId>();
			}
			CellsWithSpace.Add(cell);
		}

		public void RemoveCellWithSpace(CellId cell)
		{
			if (CellsWithSpace != null)
			{
				CellsWithSpace.Remove(cell);
				if (CellsWithSpace.Count == 0)
				{
					Facepunch.Pool.FreeList(ref CellsWithSpace);
				}
			}
		}

		public void EnterPool()
		{
			if (PackedCells != null)
			{
				Facepunch.Pool.FreeList(ref PackedCells);
			}
			if (CellsWithSpace != null)
			{
				Facepunch.Pool.FreeList(ref CellsWithSpace);
			}
		}

		public void LeavePool()
		{
		}
	}

	public const int CellCapacity = 32;

	private const int initialCellCount = 8192;

	public const int InitialCapacity = 262144;

	private Dictionary<int, CellPartition> partitions = new Dictionary<int, CellPartition>();

	private List<CellId> recycledCells = new List<CellId>();

	private Dictionary<long, int> meshLookup = new Dictionary<long, int>();

	public Dictionary<long, int> sliceIndexLookup = new Dictionary<long, int>();

	private Dictionary<int, List<long>> sliceLists = new Dictionary<int, List<long>>();

	public NativeArray<CellHeader> Cells;

	public NativeArray<InstancedCullData> CullData;

	public NativeArray<float4x4> PositionData;

	public NativeArray<MeshOverrideData> OverrideArray;

	public GPUBuffer<float4x4> PositionBuffer;

	public GPUBuffer<InstancedCullData> CullingDataBuffer;

	public GPUBuffer<MeshOverrideData> OverrideBuffer;

	private bool dirty;

	public int CellCount { get; private set; }

	public void Initialize()
	{
		AllocateNativeMemory();
		meshLookup = new Dictionary<long, int>();
		sliceIndexLookup = new Dictionary<long, int>();
		sliceLists = new Dictionary<int, List<long>>();
		partitions = new Dictionary<int, CellPartition>();
		recycledCells = new List<CellId>();
		CellCount = 0;
	}

	public void OnDestroy()
	{
		FreeNativeMemory();
		dirty = false;
	}

	private void AllocateNativeMemory()
	{
		Cells = new NativeArray<CellHeader>(8192, Allocator.Persistent);
		int length = Cells.Length * 32;
		CullData = new NativeArray<InstancedCullData>(length, Allocator.Persistent);
		PositionData = new NativeArray<float4x4>(length, Allocator.Persistent);
		OverrideArray = new NativeArray<MeshOverrideData>(length, Allocator.Persistent);
		PositionBuffer = new GPUBuffer<float4x4>(length, GPUBuffer.Target.Structured);
		CullingDataBuffer = new GPUBuffer<InstancedCullData>(length, GPUBuffer.Target.Structured);
		OverrideBuffer = new GPUBuffer<MeshOverrideData>(length, GPUBuffer.Target.Structured);
	}

	private void FreeNativeMemory()
	{
		NativeArrayEx.SafeDispose(ref Cells);
		NativeArrayEx.SafeDispose(ref CullData);
		NativeArrayEx.SafeDispose(ref PositionData);
		PositionBuffer?.Dispose();
		PositionBuffer = null;
		CullingDataBuffer?.Dispose();
		CullingDataBuffer = null;
	}

	public CellId AddMesh(InstancedCullData data, int partitionKey, float4x4 localToWorld)
	{
		if (!partitions.TryGetValue(partitionKey, out var value))
		{
			value = Facepunch.Pool.Get<CellPartition>();
			partitions[partitionKey] = value;
		}
		if (CollectionEx.IsNullOrEmpty(value.CellsWithSpace))
		{
			value.AddCellWithSpace(CreateCell(partitionKey));
		}
		CellId cellId = value.CellsWithSpace[value.CellsWithSpace.Count - 1];
		CellHeader value2 = Cells[cellId.Index];
		int num = value2.StartIndex + value2.Count;
		value2.Count++;
		int count = value2.Count;
		Cells[cellId.Index] = value2;
		if (!sliceLists.TryGetValue(data.RendererId, out var value3))
		{
			value3 = new List<long>();
			sliceLists[data.RendererId] = value3;
		}
		data.SliceIndex = value3.Count;
		value3.Add(data.VirtualMeshId);
		CullData[num] = data;
		PositionData[num] = localToWorld;
		OverrideArray[num] = default(MeshOverrideData);
		if (Render.computebuffer_setdata_immediate)
		{
			CullingDataBuffer.SetData(CullData, num, num, 1);
			PositionBuffer.SetData(PositionData, num, num, 1);
			OverrideBuffer.SetData(OverrideArray, num, num, 1);
		}
		else
		{
			dirty = true;
		}
		meshLookup.Add(data.VirtualMeshId, num);
		if (count == 32)
		{
			value.RemoveCellWithSpace(cellId);
			value.AddPackedCell(cellId);
		}
		else if (count > 32)
		{
			Debug.LogError($"AddMesh() fucked up: >{32} elements in cell {cellId}");
		}
		return cellId;
	}

	public bool TryRemoveMesh(long virtualMeshId, out InstancedCullData removedData)
	{
		if (!meshLookup.TryGetValue(virtualMeshId, out var value))
		{
			removedData = default(InstancedCullData);
			return false;
		}
		removedData = CullData[value];
		CellId cellId = GetCellId(value);
		CellHeader value2 = Cells[cellId.Index];
		int num = value2.StartIndex + value2.Count - 1;
		int count = value2.Count;
		int partitionKey = value2.PartitionKey;
		int num2 = --value2.Count;
		Cells[cellId.Index] = value2;
		if (value != num)
		{
			InstancedCullData value3 = CullData[num];
			CullData[value] = value3;
			PositionData[value] = PositionData[num];
			OverrideArray[value] = OverrideArray[num];
			meshLookup[value3.VirtualMeshId] = value;
			if (Render.computebuffer_setdata_immediate)
			{
				CullingDataBuffer.SetData(CullData, value, value, 1);
				PositionBuffer.SetData(PositionData, value, value, 1);
				OverrideBuffer.SetData(OverrideArray, value, value, 1);
			}
			else
			{
				dirty = true;
			}
		}
		CullData[num] = default(InstancedCullData);
		if (Render.computebuffer_setdata_immediate)
		{
			CullingDataBuffer.SetData(CullData, num, num, 1);
		}
		else
		{
			dirty = true;
		}
		List<long> list = sliceLists[removedData.RendererId];
		long num3 = list[list.Count - 1];
		if (removedData.VirtualMeshId != num3)
		{
			int num4 = meshLookup[num3];
			InstancedCullData value4 = CullData[num4];
			value4.SliceIndex = removedData.SliceIndex;
			CullData[num4] = value4;
			if (Render.computebuffer_setdata_immediate)
			{
				CullingDataBuffer.SetData(CullData, num4, num4, 1);
			}
			else
			{
				dirty = true;
			}
			list[removedData.SliceIndex] = num3;
		}
		list.RemoveAt(list.Count - 1);
		CellPartition obj = partitions[partitionKey];
		if (count == 32)
		{
			obj.RemovePackedCell(cellId);
			obj.AddCellWithSpace(cellId);
		}
		else if (num2 == 0)
		{
			obj.RemoveCellWithSpace(cellId);
			if (obj.IsEmpty())
			{
				partitions.Remove(partitionKey);
				Facepunch.Pool.Free(ref obj);
			}
			RecycleCell(cellId);
		}
		meshLookup.Remove(virtualMeshId);
		return true;
	}

	public InstancedMeshData? TryGetMeshData(long virtualMeshId)
	{
		if (!meshLookup.TryGetValue(virtualMeshId, out var value))
		{
			return null;
		}
		InstancedMeshData value2 = default(InstancedMeshData);
		value2.CullData = CullData[value];
		value2.LocalToWorld = PositionData[value];
		return value2;
	}

	public void SetMeshVisible(long virtualMeshId, bool visible)
	{
		if (!meshLookup.TryGetValue(virtualMeshId, out var value))
		{
			Debug.LogError($"Trying to remove mesh {virtualMeshId} that doesn't exist");
			return;
		}
		InstancedCullData value2 = CullData[value];
		if (value2.IsVisible != visible)
		{
			value2.IsVisible = visible;
			CullData[value] = value2;
			if (Render.computebuffer_setdata_immediate)
			{
				CullingDataBuffer.SetData(CullData, value, value, 1);
			}
			else
			{
				dirty = true;
			}
		}
	}

	public void SetOverride(long virtualMeshId, MeshOverrideData newData)
	{
		if (!meshLookup.TryGetValue(virtualMeshId, out var value))
		{
			Debug.LogError($"Trying to set override of mesh {virtualMeshId} that doesn't exist");
		}
		else if (OverrideArray[value] != newData)
		{
			OverrideArray[value] = newData;
			if (Render.computebuffer_setdata_immediate)
			{
				OverrideBuffer.SetData(OverrideArray, value, value, 1);
			}
			else
			{
				dirty = true;
			}
		}
	}

	private CellId CreateCell(int sortingKey)
	{
		CellId result;
		if (recycledCells.Count > 0)
		{
			result = recycledCells[recycledCells.Count - 1];
			recycledCells.RemoveAt(recycledCells.Count - 1);
		}
		else
		{
			result = new CellId(CellCount);
			CellCount++;
		}
		if (Cells.Length <= CellCount)
		{
			ExpandData();
		}
		Cells[result.Index] = new CellHeader
		{
			Count = 0,
			PartitionKey = sortingKey,
			StartIndex = result.Index * 32
		};
		return result;
	}

	public void ExpandData()
	{
		NativeArrayEx.Expand(ref Cells, Cells.Length * 2);
		int newCapacity = Cells.Length * 32;
		NativeArrayEx.Expand(ref CullData, newCapacity);
		NativeArrayEx.Expand(ref PositionData, newCapacity);
		NativeArrayEx.Expand(ref OverrideArray, newCapacity);
		CullingDataBuffer.Expand(newCapacity);
		CullingDataBuffer.SetData(CullData);
		PositionBuffer.Expand(newCapacity);
		PositionBuffer.SetData(PositionData);
		OverrideBuffer.Expand(newCapacity);
		OverrideBuffer.SetData(OverrideArray);
	}

	private void RecycleCell(CellId cellId)
	{
		recycledCells.Add(cellId);
	}

	private CellId GetCellId(int index)
	{
		return new CellId(index / 32);
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		int num = Cells.Take(CellCount).Sum((CellHeader x) => 32 - x.Count);
		int num2 = CellCount * 32;
		builder.AppendLine("### CellAllocator ###");
		builder.AppendLine($"Cells: {CellCount}");
		builder.AppendLine($"Empty Space In Cells: {num} / {num2} ({Math.Round((double)num / (double)num2, 1)}%)");
		builder.MemoryUsage("Cell Headers", Cells);
		builder.MemoryUsage("Data Array", CullData);
		builder.MemoryUsage("MeshLookup", meshLookup);
		builder.MemoryUsage("Recycled Cells", recycledCells);
		builder.MemoryUsage("Partitions", partitions);
		builder.AppendLine("# Allocation Summary #");
		var array = (from x in Cells.Take(CellCount)
			group x by x.Count into x
			select new
			{
				amountInCell = x.Key,
				count = x.Count()
			} into x
			orderby x.amountInCell
			select x).ToArray();
		foreach (var anon in array)
		{
			builder.AppendLine($"{anon.amountInCell}/{32} Cells: {anon.count}");
		}
	}

	public void FlushComputeBuffers()
	{
		if (dirty)
		{
			dirty = false;
			CullingDataBuffer.SetData(CullData);
			PositionBuffer.SetData(PositionData);
			OverrideBuffer.SetData(OverrideArray);
		}
	}
}
