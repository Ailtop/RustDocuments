using UnityEngine;

namespace Rust.UI.ServerAdmin;

public class ServerAdminUI : SingletonComponent<ServerAdminUI>
{
	public GameObjectRef PlayerEntry;

	public RectTransform PlayerInfoParent;

	public RustText PlayerCount;

	public RustInput PlayerNameFilter;

	public GameObjectRef ServerInfoEntry;

	public RectTransform ServerInfoParent;

	public GameObjectRef ConvarInfoEntry;

	public GameObjectRef ConvarInfoLongEntry;

	public RectTransform ConvarInfoParent;

	public ServerAdminPlayerInfo PlayerInfo;
}
