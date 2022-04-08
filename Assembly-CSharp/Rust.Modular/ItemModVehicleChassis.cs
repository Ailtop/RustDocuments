using UnityEngine;

namespace Rust.Modular;

public class ItemModVehicleChassis : ItemMod, VehicleModuleInformationPanel.IVehicleModuleInfo
{
	public GameObjectRef entityPrefab;

	[Range(1f, 6f)]
	public int socketsTaken = 1;

	public int SocketsTaken => socketsTaken;
}
