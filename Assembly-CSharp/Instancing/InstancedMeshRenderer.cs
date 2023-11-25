using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

public class InstancedMeshRenderer
{
	public int RendererId { get; }

	public int DrawCallIndex { get; }

	public int DrawCallCount { get; }

	public string MeshName => Mesh.name;

	public Mesh Mesh { get; }

	public Material[] Materials { get; private set; }

	public Material[] MultidrawMaterials { get; private set; }

	public ShadowCastingMode CastShadows { get; }

	public bool RecieveShadows { get; }

	public LightProbeUsage LightProbes { get; }

	public int Verticies { get; }

	public int Triangles { get; }

	public int VertexStart { get; private set; }

	public int IndexStart { get; private set; }

	public int LodLevel { get; }

	public int TotalLodLevels { get; }

	public bool IsLastLod { get; }

	public InstancedMeshCategory MeshCategory { get; }

	public MultidrawMeshInfo[] MultidrawSubmeshes { get; }

	public bool HasShadow => CastShadows != ShadowCastingMode.Off;

	public bool HasMesh => CastShadows != ShadowCastingMode.ShadowsOnly;

	public Vector3[] BoundsPoints { get; }

	public InstancedMeshRenderer(int rendererIndex, int drawCallIndex, MeshRenderKey key, Material[] multidrawMaterials, int lodLevel, int lodLevels, InstancedMeshCategory meshCategory, GeometryBuffers buffers)
	{
		RendererId = rendererIndex;
		DrawCallIndex = drawCallIndex;
		Mesh = key.Mesh;
		Materials = key.Materials;
		MultidrawMaterials = multidrawMaterials;
		CastShadows = key.CastShadows;
		RecieveShadows = key.RecieveShadows;
		LightProbes = key.LightProbeUsages;
		Verticies = Mesh.vertexCount;
		LodLevel = lodLevel;
		TotalLodLevels = lodLevels;
		IsLastLod = lodLevel == lodLevels - 1;
		MeshCategory = meshCategory;
		DrawCallCount = Mathf.Min(Mesh.subMeshCount, Materials.Length);
		if (Materials.Length > Mesh.subMeshCount)
		{
			string name = Mesh.name;
			Debug.LogError("More submesh than material for mesh " + name);
		}
		if (Mesh.subMeshCount > Materials.Length)
		{
			string name2 = Mesh.name;
			Debug.LogWarning("More materials than submesh for mesh " + name2);
		}
		for (int i = 0; i < Mesh.subMeshCount; i++)
		{
			Triangles += (int)Mesh.GetIndexCount(i) / 3;
		}
		Bounds bounds = Mesh.bounds;
		BoundsPoints = new Vector3[8]
		{
			bounds.min,
			bounds.max,
			new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)
		};
		MultidrawSubmeshes = buffers.CopyMesh(Mesh);
	}

	public void SetMaterials(Material[] materials)
	{
		Materials = materials;
	}

	public void SetPlaceholderMaterials(Material[] materials)
	{
		Materials = materials;
	}

	public int GetDrawCallIndex(int submesh)
	{
		return DrawCallIndex + submesh;
	}

	public int GetIndirectArgIndex(int submesh)
	{
		return GetDrawCallIndex(submesh) * 5;
	}

	public int GetIndirectArgByteIndex(int submesh)
	{
		return GetIndirectArgIndex(submesh) * 4;
	}
}
