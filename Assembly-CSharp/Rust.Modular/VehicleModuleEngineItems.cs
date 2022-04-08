using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Modular;

[CreateAssetMenu(fileName = "Vehicle Module Engine Items", menuName = "Rust/Vehicles/Module Engine Items")]
public class VehicleModuleEngineItems : ScriptableObject
{
	[SerializeField]
	private ItemModEngineItem[] engineItems;

	public bool TryGetItem(int tier, EngineStorage.EngineItemTypes type, out ItemModEngineItem output)
	{
		List<ItemModEngineItem> obj = Pool.GetList<ItemModEngineItem>();
		bool result = false;
		output = null;
		ItemModEngineItem[] array = engineItems;
		foreach (ItemModEngineItem itemModEngineItem in array)
		{
			if (itemModEngineItem.tier == tier && itemModEngineItem.engineItemType == type)
			{
				obj.Add(itemModEngineItem);
			}
		}
		if (obj.Count > 0)
		{
			output = obj.GetRandom();
			result = true;
		}
		Pool.FreeList(ref obj);
		return result;
	}
}
