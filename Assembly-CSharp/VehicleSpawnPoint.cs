using UnityEngine;

public class VehicleSpawnPoint : SpaceCheckingSpawnPoint
{
	public override void ObjectSpawned(SpawnPointInstance instance)
	{
		base.ObjectSpawned(instance);
		AddStartingFuel(GameObjectEx.ToBaseEntity(instance.gameObject) as VehicleSpawner.IVehicleSpawnUser);
	}

	public static void AddStartingFuel(VehicleSpawner.IVehicleSpawnUser vehicle)
	{
		vehicle?.GetFuelSystem()?.AddStartingFuel(vehicle.StartingFuelUnits());
	}
}
