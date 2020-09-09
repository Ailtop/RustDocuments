using UnityEngine;
using UnityEngine.Rendering;

public class RendererInfo : ComponentInfo<Renderer>
{
	public ShadowCastingMode shadows;

	public Material material;

	public Mesh mesh;

	public MeshFilter meshFilter;

	public override void Reset()
	{
		component.shadowCastingMode = shadows;
		if ((bool)material)
		{
			component.sharedMaterial = material;
		}
		SkinnedMeshRenderer skinnedMeshRenderer;
		if ((object)(skinnedMeshRenderer = (component as SkinnedMeshRenderer)) != null)
		{
			skinnedMeshRenderer.sharedMesh = mesh;
		}
		else if (component is MeshRenderer)
		{
			meshFilter.sharedMesh = mesh;
		}
	}

	public override void Setup()
	{
		shadows = component.shadowCastingMode;
		material = component.sharedMaterial;
		SkinnedMeshRenderer skinnedMeshRenderer;
		if ((object)(skinnedMeshRenderer = (component as SkinnedMeshRenderer)) != null)
		{
			mesh = skinnedMeshRenderer.sharedMesh;
		}
		else if (component is MeshRenderer)
		{
			meshFilter = GetComponent<MeshFilter>();
			mesh = meshFilter.sharedMesh;
		}
	}
}
