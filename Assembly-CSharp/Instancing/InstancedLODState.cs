using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

[Serializable]
public class InstancedLODState
{
	public Mesh Mesh;

	public Material[] Materials;

	public Matrix4x4 LocalToWorld;

	public ShadowCastingMode CastShadows;

	public bool RecieveShadows;

	public LightProbeUsage LightProbes;

	public int LodLevel;

	public int TotalLodLevels;

	public InstancedMeshCategory MeshCategory;

	public float MinimumDistance;

	public float MaximumDistance;

	public InstancedLODState(Matrix4x4 localToWorld, MeshRenderer renderer, float minDistance, float maxDistance, int lodLevel, int lodLevels, InstancedMeshCategory category)
	{
		MeshCull component = renderer.GetComponent<MeshCull>();
		MeshFilter component2 = renderer.GetComponent<MeshFilter>();
		Mesh = component2.sharedMesh;
		Materials = renderer.sharedMaterials;
		LocalToWorld = localToWorld;
		CastShadows = renderer.shadowCastingMode;
		RecieveShadows = renderer.receiveShadows;
		LightProbes = renderer.lightProbeUsage;
		MinimumDistance = minDistance;
		MaximumDistance = component?.Distance ?? maxDistance;
		MeshCategory = category;
		LodLevel = lodLevel;
		TotalLodLevels = lodLevels;
	}
}
