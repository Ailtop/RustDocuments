using UnityEngine;

namespace Rust.Modular;

public class ItemModVehicleModule : ItemMod, VehicleModuleInformationPanel.IVehicleModuleInfo
{
	public GameObjectRef entityPrefab;

	[Range(1f, 2f)]
	public int socketsTaken = 1;

	public bool doNonUserSpawn;

	public int SocketsTaken => socketsTaken;

	public BaseVehicleModule CreateModuleEntity(BaseEntity parent, Vector3 position, Quaternion rotation)
	{
		if (!entityPrefab.isValid)
		{
			Debug.LogError("Invalid entity prefab for module");
			return null;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(entityPrefab.resourcePath, position, rotation);
		BaseVehicleModule baseVehicleModule = null;
		if (baseEntity != null)
		{
			if (parent != null)
			{
				baseEntity.SetParent(parent, worldPositionStays: true);
				baseEntity.canTriggerParent = false;
			}
			baseEntity.Spawn();
			baseVehicleModule = baseEntity.GetComponent<BaseVehicleModule>();
			if (doNonUserSpawn)
			{
				doNonUserSpawn = false;
				baseVehicleModule.NonUserSpawn();
			}
		}
		return baseVehicleModule;
	}
}
