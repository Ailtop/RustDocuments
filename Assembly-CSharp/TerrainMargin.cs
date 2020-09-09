using UnityEngine;

public class TerrainMargin
{
	private static MaterialPropertyBlock materialPropertyBlock;

	public static void Create()
	{
		Material marginMaterial = TerrainMeta.Config.MarginMaterial;
		Vector3 center = TerrainMeta.Center;
		Vector3 size = TerrainMeta.Size;
		Vector3 b = new Vector3(size.x, 0f, 0f);
		Vector3 b2 = new Vector3(0f, 0f, size.z);
		center.y = TerrainMeta.HeightMap.GetHeight(0, 0);
		Create(center - b2, size, marginMaterial);
		Create(center - b2 - b, size, marginMaterial);
		Create(center - b2 + b, size, marginMaterial);
		Create(center - b, size, marginMaterial);
		Create(center + b, size, marginMaterial);
		Create(center + b2, size, marginMaterial);
		Create(center + b2 - b, size, marginMaterial);
		Create(center + b2 + b, size, marginMaterial);
	}

	private static void Create(Vector3 position, Vector3 size, Material material)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
		gameObject.name = "TerrainMargin";
		gameObject.layer = 16;
		gameObject.transform.position = position;
		gameObject.transform.localScale = size * 0.1f;
		Object.Destroy(gameObject.GetComponent<MeshRenderer>());
		Object.Destroy(gameObject.GetComponent<MeshFilter>());
	}
}
