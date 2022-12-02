using System;
using UnityEngine;

[Serializable]
public class ModularCarCodeLockVisuals : VehicleModuleButtonComponent
{
	[SerializeField]
	private GameObject lockedVisuals;

	[SerializeField]
	private GameObject unlockedVisuals;

	[SerializeField]
	private GameObject blockedVisuals;

	[SerializeField]
	private GameObjectRef codelockEffectDenied;

	[SerializeField]
	private GameObjectRef codelockEffectShock;

	[SerializeField]
	private float xOffset = 0.91f;

	public override void ServerUse(BasePlayer player, BaseVehicleModule parentModule)
	{
	}
}
