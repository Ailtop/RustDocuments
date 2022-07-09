using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.ServerAdmin;

public class ServerAdminUGCEntryImage : ServerAdminUGCEntry
{
	public RawImage Image;

	public RectTransform Backing;

	public GameObject MultiImageRoot;

	public RustText ImageIndex;

	public ServerAdminPlayerId[] historyIds = new ServerAdminPlayerId[0];
}
