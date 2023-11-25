using Rust.UI;
using UnityEngine;

public class ServerBrowserInfoNexus : SingletonComponent<ServerBrowserInfoNexus>
{
	public GameObject loadingIndicator;

	public RustText serverName;

	public RustText serverDesc;

	public RustText playerCount;

	public RustText zoneCount;

	public RustText lastWiped;

	public HttpImage coverImage;

	public HttpImage logoImage;

	public RectTransform zoneListParent;

	public GameObjectRef zoneListItem;

	public RustButton joinServer;

	public RustButton viewWebpage;
}
