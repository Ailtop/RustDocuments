using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI
{
	public class SteamInventoryManager : SingletonComponent<SteamInventoryManager>
	{
		public GameObject inventoryItemPrefab;

		public GameObject inventoryCanvas;

		public GameObject missingItems;

		public SteamInventoryCrafting CraftControl;

		public List<GameObject> items;

		public GameObject LoadingOverlay;
	}
}
