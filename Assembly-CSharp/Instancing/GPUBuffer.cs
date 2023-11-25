using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace Instancing;

public class GPUBuffer
{
	public enum Target
	{
		Structured = 0,
		IndirectArgs = 1,
		Vertex = 2,
		Index = 3,
		Raw = 4
	}
}
public class GPUBuffer<T> : GPUBuffer, IDisposable where T : unmanaged
{
	private GraphicsBuffer.Target _type;

	public int BufferVersion { get; private set; }

	public GraphicsBuffer Buffer { get; private set; }

	public Target Type { get; private set; }

	public int count { get; private set; }

	public int stride { get; private set; }

	public int ByteLength => count * stride;

	public GPUBuffer(int length, Target target)
	{
		count = length;
		stride = Marshal.SizeOf<T>();
		Type = target;
		switch (target)
		{
		case Target.Structured:
			_type = GraphicsBuffer.Target.Structured;
			break;
		case Target.IndirectArgs:
			_type = GraphicsBuffer.Target.IndirectArguments;
			break;
		case Target.Vertex:
			_type = GraphicsBuffer.Target.Vertex;
			break;
		case Target.Index:
			_type = GraphicsBuffer.Target.Index;
			break;
		case Target.Raw:
			_type = GraphicsBuffer.Target.Raw;
			break;
		default:
			throw new NotImplementedException($"GPUBuffer Target '{target}'");
		}
		Buffer = new GraphicsBuffer(_type, length, stride);
		ClearData();
	}

	public void SetData(List<T> data)
	{
		Buffer.SetData(data);
	}

	public void SetData(List<int> data, int nativeArrayIndex, int computeBufferIndex, int length)
	{
		Buffer.SetData(data, nativeArrayIndex, computeBufferIndex, length);
	}

	public void SetData(T[] data)
	{
		Buffer.SetData(data);
	}

	public void SetData(T[] data, int nativeArrayIndex, int computeBufferIndex, int length)
	{
		Buffer.SetData(data, nativeArrayIndex, computeBufferIndex, length);
	}

	public void SetData(NativeArray<T> data)
	{
		Buffer.SetData(data);
	}

	public void SetData(NativeArray<T> data, int nativeArrayIndex, int computeBufferIndex, int length)
	{
		Buffer.SetData(data, nativeArrayIndex, computeBufferIndex, length);
	}

	public void ClearData()
	{
		using NativeArray<T> data = new NativeArray<T>(count, Allocator.Temp);
		Buffer.SetData(data);
	}

	public void Expand(int newCapacity, bool preserveData = false)
	{
		GraphicsBuffer graphicsBuffer = new GraphicsBuffer(_type, newCapacity, stride);
		BufferVersion++;
		if (preserveData)
		{
			T[] data = new T[newCapacity];
			Buffer.GetData(data, 0, 0, count);
			graphicsBuffer.SetData(data);
		}
		Dispose();
		Buffer = graphicsBuffer;
		count = newCapacity;
		if (!preserveData)
		{
			ClearData();
		}
	}

	public void EnsureCapacity(int size, bool preserveData = false, float expandRatio = 2f)
	{
		if (Buffer.count < size)
		{
			int newCapacity = (int)((float)size * expandRatio);
			Expand(newCapacity, preserveData);
		}
	}

	public void Dispose()
	{
		Buffer?.Dispose();
		Buffer = null;
	}
}
