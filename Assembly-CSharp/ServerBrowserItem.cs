using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerBrowserItem : MonoBehaviour
{
	public TextMeshProUGUI serverName;

	public RustText mapName;

	public TextMeshProUGUI playerCount;

	public TextMeshProUGUI ping;

	public Toggle favourited;

	public ServerBrowserTag[] serverTags;
}
