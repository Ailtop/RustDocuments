using UnityEngine;

namespace VLB;

public static class GlobalMesh
{
	private static Mesh ms_Mesh;

	public static Mesh mesh
	{
		get
		{
			if (ms_Mesh == null)
			{
				ms_Mesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, cap: true);
				ms_Mesh.hideFlags = Consts.ProceduralObjectsHideFlags;
			}
			return ms_Mesh;
		}
	}
}
