using UnityEngine;

public class GenerateRoadMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0f;

	public Mesh RoadMesh;

	public Mesh[] RoadMeshes;

	public Material RoadMaterial;

	public Material RoadRingMaterial;

	public PhysicMaterial RoadPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		if (RoadMeshes == null || RoadMeshes.Length == 0)
		{
			RoadMeshes = new Mesh[1]
			{
				RoadMesh
			};
		}
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			if (road.IsExtraNarrow)
			{
				continue;
			}
			foreach (PathList.MeshObject item in road.CreateMesh(RoadMeshes, 0f))
			{
				GameObject gameObject = new GameObject("Road Mesh");
				gameObject.transform.position = item.Position;
				gameObject.layer = 16;
				GameObjectEx.SetHierarchyGroup(gameObject, road.Name);
				gameObject.SetActive(false);
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RoadPhysicMaterial;
				meshCollider.sharedMesh = item.Meshes[0];
				gameObject.AddComponent<AddToHeightMap>();
				gameObject.SetActive(true);
			}
		}
	}
}
