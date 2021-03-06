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
		RiverMeshes = new Mesh[1] { RiverMesh };
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			foreach (PathList.MeshObject item in river.CreateMesh(RiverMeshes, 0.1f))
			{
				GameObject obj = new GameObject("River Mesh");
				obj.transform.position = item.Position;
				obj.tag = "River";
				obj.layer = 4;
				GameObjectEx.SetHierarchyGroup(obj, river.Name);
				obj.SetActive(false);
				MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RiverPhysicMaterial;
				meshCollider.sharedMesh = item.Meshes[0];
				obj.AddComponent<RiverInfo>();
				obj.AddComponent<WaterBody>();
				obj.AddComponent<AddToWaterMap>();
				obj.SetActive(true);
			}
		}
	}
}
