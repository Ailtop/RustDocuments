using UnityEngine;

public class SpawnRandomModularCar : BaseEntity
{
	[SerializeField]
	private GameObjectRef[] chassisPrefabs;

	[SerializeField]
	private ItemDefinition[] moduleItemDefs;

	public override void Spawn()
	{
		base.Spawn();
		Kill();
	}
}
