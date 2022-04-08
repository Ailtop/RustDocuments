using UnityEngine;

public class GenerateRailMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0f;

	public Mesh RailMesh;

	public Mesh[] RailMeshes;

	public Material RailMaterial;

	public PhysicMaterial RailPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		if (RailMeshes == null || RailMeshes.Length == 0)
		{
			RailMeshes = new Mesh[1] { RailMesh };
		}
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			foreach (PathList.MeshObject item in rail.CreateMesh(RailMeshes, 0f))
			{
				GameObject obj = new GameObject("Rail Mesh");
				obj.transform.position = item.Position;
				obj.layer = 16;
				GameObjectEx.SetHierarchyGroup(obj, rail.Name);
				obj.SetActive(value: false);
				MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RailPhysicMaterial;
				meshCollider.sharedMesh = item.Meshes[0];
				obj.AddComponent<AddToHeightMap>();
				obj.SetActive(value: true);
			}
			AddTrackSpline(rail);
		}
	}

	private void AddTrackSpline(PathList rail)
	{
		TrainTrackSpline trainTrackSpline = HierarchyUtil.GetRoot(rail.Name).AddComponent<TrainTrackSpline>();
		Vector3[] points = rail.Path.Points;
		for (int i = 0; i < points.Length; i++)
		{
			points[i].y += 0.41f;
		}
		trainTrackSpline.SetAll(points, rail.Path.Tangents, 0.25f);
	}
}
