using System;
using UnityEngine;

[Serializable]
public class ModularCarCentralLockingSwitch : VehicleModuleButtonComponent
{
	public Transform centralLockingSwitch;

	public Vector3 switchOffPos;

	public Vector3 switchOnPos;

	public override void ServerUse(BasePlayer player, BaseVehicleModule parentModule)
	{
		ModularCar modularCar;
		if ((object)(modularCar = (parentModule.Vehicle as ModularCar)) != null)
		{
			modularCar.carLock.ToggleCentralLocking();
		}
	}
}
