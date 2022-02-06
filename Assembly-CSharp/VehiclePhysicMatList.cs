using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Vehicles/Physic Materials List", fileName = "Vehicle Physic Mat List")]
public class VehiclePhysicMatList : ScriptableObject
{
	public enum GroundType
	{
		None,
		HardSurface,
		Grass,
		Sand,
		Snow,
		Dirt,
		Gravel
	}

	[SerializeField]
	private PhysicMaterial defaultGroundMaterial;

	[SerializeField]
	private PhysicMaterial snowGroundMaterial;

	[SerializeField]
	private PhysicMaterial grassGroundMaterial;

	[SerializeField]
	private PhysicMaterial sandGroundMaterial;

	[SerializeField]
	private List<PhysicMaterial> dirtGroundMaterials;

	[SerializeField]
	private List<PhysicMaterial> stoneyGroundMaterials;

	public GroundType GetCurrentGroundType(bool isGrounded, RaycastHit hit)
	{
		PhysicMaterial materialAt = defaultGroundMaterial;
		if (isGrounded && hit.collider != null)
		{
			materialAt = ColliderEx.GetMaterialAt(hit.collider, hit.point);
		}
		if (!isGrounded)
		{
			return GroundType.None;
		}
		if (materialAt == null)
		{
			return GroundType.HardSurface;
		}
		string text = materialAt.name;
		if (text == grassGroundMaterial.name)
		{
			return GroundType.Grass;
		}
		if (text == sandGroundMaterial.name)
		{
			return GroundType.Sand;
		}
		if (text == snowGroundMaterial.name)
		{
			return GroundType.Snow;
		}
		for (int i = 0; i < dirtGroundMaterials.Count; i++)
		{
			if (dirtGroundMaterials[i].name == text)
			{
				return GroundType.Dirt;
			}
		}
		for (int j = 0; j < stoneyGroundMaterials.Count; j++)
		{
			if (stoneyGroundMaterials[j].name == text)
			{
				return GroundType.Gravel;
			}
		}
		return GroundType.HardSurface;
	}
}
