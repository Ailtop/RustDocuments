using UnityEngine;

namespace Rust.UI.ServerAdmin;

public class ServerAdminUGCEntryPattern : ServerAdminUGCEntry
{
	public GameObjectRef StarPrefab;

	public RectTransform StarRoot;

	public ServerAdminPlayerId EditedBy;
}
