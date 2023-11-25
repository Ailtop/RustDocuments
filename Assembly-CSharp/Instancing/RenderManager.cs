using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Instancing;

public class RenderManager
{
	public NativeArray<InstancedRendererJobData> RendererArray;

	public NativeArray<uint> MeshCountArray;

	private Dictionary<MeshRenderKey, InstancedMeshRenderer> rendererLookup = new Dictionary<MeshRenderKey, InstancedMeshRenderer>();

	public List<InstancedMeshRenderer> Renderers = new List<InstancedMeshRenderer>();

	public GeometryBuffers GeometryBuffers;

	private const int initialRendererCapacity = 512;

	private readonly DrawCallManager drawCallManager;

	private readonly MaterialCache _materialCache;

	public int TotalMeshCount { get; private set; }

	public int TotalDrawCallCount { get; private set; }

	public int RendererCount => Renderers.Count;

	public RenderManager(DrawCallManager drawCalls, GeometryBuffers geometryBuffers, MaterialCache materialCache)
	{
		drawCallManager = drawCalls;
		GeometryBuffers = geometryBuffers;
		_materialCache = materialCache;
	}

	public void Initialize()
	{
		Renderers = new List<InstancedMeshRenderer>();
		rendererLookup = new Dictionary<MeshRenderKey, InstancedMeshRenderer>();
		TotalMeshCount = 0;
		TotalDrawCallCount = 0;
		AllocateNativeMemory();
	}

	public void OnDestroy()
	{
		FreeNativeMemory();
	}

	private void AllocateNativeMemory()
	{
		RendererArray = new NativeArray<InstancedRendererJobData>(512, Allocator.Persistent);
		MeshCountArray = new NativeArray<uint>(512, Allocator.Persistent);
	}

	private void FreeNativeMemory()
	{
		NativeArrayEx.SafeDispose(ref RendererArray);
		NativeArrayEx.SafeDispose(ref MeshCountArray);
	}

	public void OnMeshAdded(int rendererId)
	{
		MeshCountArray[rendererId] += 1;
		TotalMeshCount++;
	}

	public void OnMeshRemoved(int rendererId)
	{
		MeshCountArray[rendererId] -= 1;
		TotalMeshCount--;
	}

	public Bounds CalculateMeshBounds(int meshId, Matrix4x4 localToWorld)
	{
		return GeometryUtility.CalculateBounds(Renderers[meshId].BoundsPoints, localToWorld);
	}

	public bool DoesRendererHaveShadow(int meshId)
	{
		return Renderers[meshId].HasShadow;
	}

	public bool DoesRendererHaveMesh(int rendererId)
	{
		return Renderers[rendererId].HasMesh;
	}

	public bool IsLastLOD(int rendererId)
	{
		return Renderers[rendererId].IsLastLod;
	}

	public int GetRendererId(InstancedLODState lod)
	{
		InstancedMeshRenderer instancedMeshRenderer = GetRenderer(lod);
		if (instancedMeshRenderer == null)
		{
			instancedMeshRenderer = RegisterRenderer(lod);
		}
		return instancedMeshRenderer.RendererId;
	}

	public float GetMinDistance(int rendererId)
	{
		return RendererArray[rendererId].MinDistance;
	}

	public float GetMaxDistance(int rendererId)
	{
		return RendererArray[rendererId].MaxDistance;
	}

	public Mesh GetMeshForRenderer(int rendererId)
	{
		return Renderers[rendererId].Mesh;
	}

	private InstancedMeshRenderer GetRenderer(InstancedLODState lod)
	{
		MeshRenderKey key = new MeshRenderKey(lod.Mesh, lod.Materials, lod.CastShadows, lod.RecieveShadows, lod.LightProbes);
		rendererLookup.TryGetValue(key, out var value);
		return value;
	}

	private InstancedMeshRenderer RegisterRenderer(InstancedLODState lod)
	{
		MeshRenderKey key = new MeshRenderKey(lod.Mesh, lod.Materials, lod.CastShadows, lod.RecieveShadows, lod.LightProbes);
		if (rendererLookup.TryGetValue(key, out var value))
		{
			Debug.LogWarning("Tried to register a renderer that already exists: skipping");
			return value;
		}
		int count = Renderers.Count;
		int drawCallCount = drawCallManager.DrawCallCount;
		Material[] multidrawMaterials = key.Materials.Select((Material x) => _materialCache.EnableProceduralInstancing(x)).ToArray();
		InstancedMeshRenderer instancedMeshRenderer = new InstancedMeshRenderer(count, drawCallCount, key, multidrawMaterials, lod.LodLevel, lod.TotalLodLevels, lod.MeshCategory, GeometryBuffers);
		for (int i = 0; i < instancedMeshRenderer.DrawCallCount; i++)
		{
			Mesh mesh = instancedMeshRenderer.Mesh;
			drawCallManager.AddDrawCall(instancedMeshRenderer, i, mesh.GetIndexCount(i), mesh.GetIndexStart(i), mesh.GetBaseVertex(i), instancedMeshRenderer.MultidrawSubmeshes[i]);
		}
		rendererLookup[key] = instancedMeshRenderer;
		Renderers.Add(instancedMeshRenderer);
		InstancedRendererJobData instancedRendererJobData = default(InstancedRendererJobData);
		instancedRendererJobData.Id = count;
		instancedRendererJobData.MinDistance = lod.MinimumDistance;
		instancedRendererJobData.MaxDistance = lod.MaximumDistance;
		instancedRendererJobData.ShadowMode = instancedMeshRenderer.CastShadows;
		instancedRendererJobData.DrawCallCount = instancedMeshRenderer.DrawCallCount;
		InstancedRendererJobData value2 = instancedRendererJobData;
		EnsureJobDataCapacity();
		RendererArray[count] = value2;
		TotalDrawCallCount += instancedMeshRenderer.DrawCallCount;
		return instancedMeshRenderer;
	}

	private void EnsureJobDataCapacity()
	{
		if (RendererArray.Length <= Renderers.Count)
		{
			int newCapacity = Mathf.ClosestPowerOfTwo(Renderers.Count) * 2;
			NativeArrayEx.Expand(ref RendererArray, newCapacity);
			NativeArrayEx.Expand(ref MeshCountArray, newCapacity);
		}
	}

	public void PrintMemoryUsage(StringBuilder builder)
	{
		builder.AppendLine("### RenderManager ###");
		builder.MemoryUsage("Renderers", Renderers);
		builder.MemoryUsage("RendererArray", RendererArray);
		builder.MemoryUsage("MeshCountArray", MeshCountArray);
	}
}
