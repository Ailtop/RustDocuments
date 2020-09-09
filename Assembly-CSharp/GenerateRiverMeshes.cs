using UnityEngine;

public class GenerateRiverMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0.1f;

	public Mesh RiverMesh;

	public Mesh[] RiverMeshes;

	public Material RiverMaterial;

	public PhysicMaterial RiverPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		RiverMeshes = new Mesh[1]
		{
			RiverMesh
		};
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			foreach (PathList.MeshObject item in river.CreateMesh(RiverMeshes, 0.1f))
			{
				GameObject gameObject = new GameObject("River Mesh");
				gameObject.transform.position = item.Position;
				gameObject.tag = "River";
				gameObject.layer = 4;
				GameObjectEx.SetHierarchyGroup(gameObject, river.Name);
				gameObject.SetActive(false);
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RiverPhysicMaterial;
				meshCollider.sharedMesh = item.Meshes[0];
				gameObject.AddComponent<RiverInfo>();
				gameObject.AddComponent<WaterBody>();
				gameObject.AddComponent<AddToWaterMap>();
				gameObject.SetActive(true);
			}
		}
	}
}
