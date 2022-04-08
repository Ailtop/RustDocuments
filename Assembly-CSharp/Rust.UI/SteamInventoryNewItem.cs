using System.Threading.Tasks;
using UnityEngine;

namespace Rust.UI;

public class SteamInventoryNewItem : MonoBehaviour
{
	public async Task Open(IPlayerItem item)
	{
		base.gameObject.SetActive(value: true);
		GetComponentInChildren<SteamInventoryItem>().Setup(item);
		while ((bool)this && base.gameObject.activeSelf)
		{
			await Task.Delay(100);
		}
	}
}
