using UnityEngine;

namespace Rust.Modular
{
	public class ItemModEngineItem : ItemMod
	{
		public EngineStorage.EngineItemTypes engineItemType;

		[Range(1f, 3f)]
		public int tier = 1;
	}
}
