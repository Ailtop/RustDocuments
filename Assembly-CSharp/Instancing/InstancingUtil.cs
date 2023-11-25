using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

internal static class InstancingUtil
{
	public static readonly int PositionBufferProperty = Shader.PropertyToID("_PositionBuffer");

	public static readonly int RenderBufferProperty = Shader.PropertyToID("_PostCullBuffer");

	public static readonly int IndirectExtraArgProperty = Shader.PropertyToID("_IndirectExtraArgsBuffer");

	public static readonly int Param_MeshOverrideBuffer = Shader.PropertyToID("_MeshOverrideBuffer");

	public static readonly int Param_RenderSliceIndexes = Shader.PropertyToID("_RenderSliceIndexes");

	public static readonly int DrawCallIndexProperty = Shader.PropertyToID("_DrawCallIndex");

	public static readonly int Param_RendererIndex = Shader.PropertyToID("_RendererIndex");

	public static readonly int Param_Verticies = Shader.PropertyToID("_Verticies");

	public static readonly int Param_Triangles = Shader.PropertyToID("_Triangles");

	public static readonly GlobalKeyword Keyword_Rust_Procedural_Rendering = GlobalKeyword.Create("RUST_PROCEDURAL_INSTANCING");

	public const int CullingGPUThreads = 1024;

	public static float MB(int bytes)
	{
		return (float)Math.Round((float)(bytes / 100000) / 10f, 1);
	}

	public static StringBuilder MemoryUsage(this StringBuilder builder, string name, ComputeBuffer buffer)
	{
		builder.AppendLine($"[ComputeBuffer] {name} {buffer.count} | {MB(buffer.count * buffer.stride)}MB");
		return builder;
	}

	public static StringBuilder MemoryUsage(this StringBuilder builder, string name, GraphicsBuffer buffer)
	{
		builder.AppendLine($"[ComputeBuffer] {name} {buffer.count} | {MB(buffer.count * buffer.stride)}MB");
		return builder;
	}

	public static StringBuilder MemoryUsage<T>(this StringBuilder builder, string name, NativeArray<T> array, int count = -1) where T : unmanaged
	{
		int num = Marshal.SizeOf<T>();
		builder.AppendLine(string.Format("[NativeArray] {0}{1} Capacity: {2} | {3}MB", name, (count >= 0) ? (" Count: " + count) : "", array.Length, MB(array.Length * num)));
		return builder;
	}

	public static StringBuilder MemoryUsage<T>(this StringBuilder builder, string name, ICollection<T> array)
	{
		Type type = (array.GetType().IsGenericType ? array.GetType() : array.GetType().GetGenericTypeDefinition());
		string arg = "Collection";
		if (type == typeof(Dictionary<, >))
		{
			arg = "Dictionary";
		}
		else if (type == typeof(List<>))
		{
			arg = "List";
		}
		else if (type == typeof(HashSet<>))
		{
			arg = "HashSet";
		}
		else if (type == typeof(Array))
		{
			arg = "Array";
		}
		int count = array.Count;
		builder.AppendLine($"[{arg}] {name} Size: {count}");
		return builder;
	}

	public static int GetIterationCount(int count, int threads)
	{
		return count / threads + ((count % threads != 0) ? 1 : 0);
	}
}
