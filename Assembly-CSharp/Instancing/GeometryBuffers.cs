using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

public class GeometryBuffers
{
	[StructLayout(LayoutKind.Explicit, Size = 96)]
	public struct VertexData
	{
		[FieldOffset(0)]
		public float4 Position;

		[FieldOffset(16)]
		public float4 UV01;

		[FieldOffset(32)]
		public float4 UV23;

		[FieldOffset(48)]
		public float4 Normal;

		[FieldOffset(64)]
		public float4 Tangent;

		[FieldOffset(80)]
		public float4 Color;
	}

	private int _meshCopyMode;

	public GPUBuffer<VertexData> VertexBuffer;

	public GPUBuffer<int> TriangleBuffer;

	private int VertexIndex;

	private int TriangleIndex;

	private Dictionary<Mesh, MultidrawMeshInfo[]> _meshes = new Dictionary<Mesh, MultidrawMeshInfo[]>();

	public bool IsDirty { get; set; }

	public void Initialize(int meshCopyMode)
	{
		_meshCopyMode = meshCopyMode;
		AllocateNativeMemory();
		ResetStreamPosition();
	}

	private void ResetStreamPosition()
	{
		TriangleIndex = 0;
		VertexIndex = 0;
	}

	public void Destroy()
	{
		FreeNativeMemory();
		_meshes.Clear();
	}

	private void AllocateNativeMemory()
	{
		VertexBuffer = new GPUBuffer<VertexData>(800000, GPUBuffer.Target.Structured);
		TriangleBuffer = new GPUBuffer<int>(3000000, GPUBuffer.Target.Structured);
	}

	private void FreeNativeMemory()
	{
		VertexBuffer?.Dispose();
		VertexBuffer = null;
		TriangleBuffer?.Dispose();
		TriangleBuffer = null;
	}

	public MultidrawMeshInfo[] CopyMesh(Mesh mesh)
	{
		if (_meshes.TryGetValue(mesh, out var value))
		{
			return value;
		}
		value = CalculateSubmeshInfo(mesh);
		_meshes.Add(mesh, value);
		if (_meshCopyMode == 0)
		{
			CopyMeshViaCPU(mesh);
		}
		else
		{
			CopyMeshViaShader(mesh);
		}
		IsDirty = true;
		return value;
	}

	private void CopyMeshViaShader(Mesh mesh)
	{
		mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		int index = 0;
		GraphicsBuffer vertexBuffer = mesh.GetVertexBuffer(index);
		GraphicsBuffer indexBuffer = mesh.GetIndexBuffer();
		ComputeShader copyMeshShader = SingletonComponent<InstancedScheduler>.Instance.CopyMeshShader;
		int kernelIndex = copyMeshShader.FindKernel("CopyMeshKernel");
		int vertexCount = mesh.vertexCount;
		int num = 0;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			num += (int)mesh.GetIndexCount(i);
		}
		copyMeshShader.SetInt("_Offset_Vertex", mesh.GetVertexAttributeOffset(VertexAttribute.Position));
		copyMeshShader.SetInt("_Offset_UV0", mesh.GetVertexAttributeOffset(VertexAttribute.TexCoord0));
		copyMeshShader.SetInt("_Offset_UV1", mesh.GetVertexAttributeOffset(VertexAttribute.TexCoord1));
		copyMeshShader.SetInt("_Offset_UV2", mesh.GetVertexAttributeOffset(VertexAttribute.TexCoord2));
		copyMeshShader.SetInt("_Offset_UV3", mesh.GetVertexAttributeOffset(VertexAttribute.TexCoord3));
		copyMeshShader.SetInt("_Offset_Normal", mesh.GetVertexAttributeOffset(VertexAttribute.Normal));
		copyMeshShader.SetInt("_Offset_Tangent", mesh.GetVertexAttributeOffset(VertexAttribute.Tangent));
		copyMeshShader.SetInt("_Offset_Color", mesh.GetVertexAttributeOffset(VertexAttribute.Color));
		copyMeshShader.SetBuffer(kernelIndex, "_Verts", mesh.GetVertexBuffer(index));
		copyMeshShader.SetBuffer(kernelIndex, "_Triangles", mesh.GetIndexBuffer());
		copyMeshShader.SetInt("_TriangleCount", num);
		copyMeshShader.SetInt("_VertexCount", vertexCount);
		copyMeshShader.SetInt("_VertexStride", mesh.GetVertexBufferStride(0));
		copyMeshShader.SetInt("_TriangleStride", (mesh.indexFormat == IndexFormat.UInt16) ? 2 : 4);
		copyMeshShader.SetInt("_OutputVertexIndex", VertexIndex);
		copyMeshShader.SetInt("_OutputTriangleIndex", TriangleIndex);
		copyMeshShader.SetBuffer(kernelIndex, "_Output", VertexBuffer.Buffer);
		copyMeshShader.SetBuffer(kernelIndex, "_OutputTriangles", TriangleBuffer.Buffer);
		VertexIndex += vertexCount;
		TriangleIndex += num;
		if (VertexBuffer.count < VertexIndex + 1 || TriangleBuffer.count < TriangleIndex + 1)
		{
			Debug.Log("Resizing multidraw geometry buffer");
			VertexBuffer.EnsureCapacity(VertexIndex + 1, preserveData: true);
			TriangleBuffer.EnsureCapacity(TriangleIndex + 1, preserveData: true);
			IsDirty = true;
		}
		int iterationCount = Instancing.InstancingUtil.GetIterationCount(Mathf.Max(num, vertexCount), 1024);
		copyMeshShader.Dispatch(kernelIndex, iterationCount, 1, 1);
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
	}

	public void Rebuild()
	{
		ResetStreamPosition();
		foreach (Mesh key in _meshes.Keys)
		{
			CopyMeshViaShader(key);
		}
	}

	private MultidrawMeshInfo[] CalculateSubmeshInfo(Mesh mesh)
	{
		MultidrawMeshInfo[] array = new MultidrawMeshInfo[mesh.subMeshCount];
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			SubMeshDescriptor subMesh = mesh.GetSubMesh(i);
			MultidrawMeshInfo multidrawMeshInfo = default(MultidrawMeshInfo);
			multidrawMeshInfo.IndexStart = TriangleIndex + subMesh.indexStart;
			multidrawMeshInfo.VertexStart = VertexIndex + subMesh.baseVertex;
			multidrawMeshInfo.VertexCount = subMesh.vertexCount;
			MultidrawMeshInfo multidrawMeshInfo2 = multidrawMeshInfo;
			array[i] = multidrawMeshInfo2;
		}
		return array;
	}

	private void CopyMeshViaCPU(Mesh mesh)
	{
		MeshCache.Data data = MeshCache.Get(mesh);
		NativeArray<VertexData> data2 = new NativeArray<VertexData>(data.vertices.Length, Allocator.Temp);
		for (int i = 0; i < data2.Length; i++)
		{
			VertexData value = default(VertexData);
			Vector3 vector = data.vertices[i];
			Vector2 vector2 = (mesh.HasVertexAttribute(VertexAttribute.TexCoord0) ? data.uv[i] : Vector2.zero);
			Vector2 vector3 = (mesh.HasVertexAttribute(VertexAttribute.TexCoord1) ? data.uv2[i] : Vector2.zero);
			Vector2 vector4 = (mesh.HasVertexAttribute(VertexAttribute.TexCoord2) ? data.uv3[i] : Vector2.zero);
			Vector2 vector5 = (mesh.HasVertexAttribute(VertexAttribute.TexCoord3) ? data.uv4[i] : Vector2.zero);
			Vector3 vector6 = (mesh.HasVertexAttribute(VertexAttribute.Normal) ? data.normals[i] : Vector3.zero);
			Vector4 vector7 = (mesh.HasVertexAttribute(VertexAttribute.Tangent) ? data.tangents[i] : Vector4.zero);
			Color32 color = (mesh.HasVertexAttribute(VertexAttribute.Color) ? data.colors32[i] : new Color32(0, 0, 0, 0));
			value.Position = new float4(vector.x, vector.y, vector.z, 1f);
			value.UV01 = new float4(vector2.x, vector2.y, vector3.x, vector3.y);
			value.UV23 = new float4(vector4.x, vector4.y, vector5.x, vector5.y);
			value.Normal = new float4(vector6.x, vector6.y, vector6.z, 1f);
			value.Tangent = new float4(vector7.x, vector7.y, vector7.z, vector7.w);
			value.Color = new float4((float)(int)color.r / 255f, (float)(int)color.g / 255f, (float)(int)color.b / 255f, (float)(int)color.a / 255f);
			data2[i] = value;
		}
		VertexBuffer.EnsureCapacity(VertexIndex + data2.Length + 1);
		VertexBuffer.SetData(data2, 0, VertexIndex, data2.Length);
		VertexIndex += data2.Length;
		int[] triangles = data.triangles;
		TriangleBuffer.EnsureCapacity(TriangleIndex + triangles.Length + 1);
		TriangleBuffer.SetData(triangles, 0, TriangleIndex, triangles.Length);
		TriangleIndex += triangles.Length;
		data2.Dispose();
		IsDirty = true;
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		builder.AppendLine($"Vertex Buffer: {VertexIndex} / {VertexBuffer.count}");
		builder.AppendLine($"Triangle Buffer: {TriangleIndex} / {TriangleBuffer.count}");
		builder.AppendLine($"Meshes: {_meshes.Count}");
	}
}
