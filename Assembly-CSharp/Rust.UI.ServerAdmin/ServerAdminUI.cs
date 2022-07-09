using UnityEngine;
using UnityEngine.UI;

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

	public RustInput UgcNameFilter;

	public GameObjectRef ImageEntry;

	public GameObjectRef PatternEntry;

	public GameObjectRef SoundEntry;

	public RectTransform UgcParent;

	public GameObject ExpandedUgcRoot;

	public RawImage ExpandedImage;

	public RectTransform ExpandedImageBacking;
}
