using Oxide.Core;
using UnityEngine;

public class ModularCarSeat : BaseVehicleSeat
{
	[SerializeField]
	private bool supportsMouseSteer;

	[SerializeField]
	public Vector3 leftFootIKPos;

	[SerializeField]
	public Vector3 rightFootIKPos;

	[SerializeField]
	private Vector3 leftHandIKPos;

	[SerializeField]
	private Vector3 rightHandIKPos;

	[SerializeField]
	private bool forceHeadLookAt;

	public float providesComfort;

	public VehicleModuleSeating associatedSeatingModule;

	public override bool CanSwapToThis(BasePlayer player)
	{
		object obj = Interface.CallHook("CanSwapToSeat", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (associatedSeatingModule.DoorsAreLockable)
		{
			ModularCar modularCar = associatedSeatingModule.Vehicle as ModularCar;
			if (modularCar != null)
			{
				return modularCar.PlayerCanUseThis(player, ModularCarLock.LockType.Door);
			}
		}
		return true;
	}

	public override float GetComfort()
	{
		return providesComfort;
	}
}
