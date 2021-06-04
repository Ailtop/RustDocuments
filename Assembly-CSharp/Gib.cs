using UnityEngine;

public class Gib : ListComponent<Gib>
{
	public static int gibCount;

	public MeshFilter _meshFilter;

	public MeshRenderer _meshRenderer;

	public MeshCollider _meshCollider;

	public BoxCollider _boxCollider;

	public SphereCollider _sphereCollider;

	public CapsuleCollider _capsuleCollider;

	public Rigidbody _rigidbody;

	public static string GetEffect(PhysicMaterial physicMaterial)
	{
		switch (AssetNameCache.GetNameLower(physicMaterial))
		{
		case "wood":
			return "assets/bundled/prefabs/fx/building/wood_gib.prefab";
		case "concrete":
			return "assets/bundled/prefabs/fx/building/stone_gib.prefab";
		case "metal":
			return "assets/bundled/prefabs/fx/building/metal_sheet_gib.prefab";
		case "rock":
			return "assets/bundled/prefabs/fx/building/stone_gib.prefab";
		case "flesh":
			return "assets/bundled/prefabs/fx/building/wood_gib.prefab";
		default:
			return "assets/bundled/prefabs/fx/building/wood_gib.prefab";
		}
	}
}
