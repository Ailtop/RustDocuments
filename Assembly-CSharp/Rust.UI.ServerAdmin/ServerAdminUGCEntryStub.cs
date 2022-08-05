using UnityEngine;

namespace Rust.UI.ServerAdmin;

public class ServerAdminUGCEntryStub : MonoBehaviour
{
	public ServerAdminUGCEntryAudio AudioWidget;

	public ServerAdminUGCEntryImage ImageWidget;

	public ServerAdminUGCEntryPattern PatternWidget;

	public RustText PrefabName;

	public RustButton HistoryButton;

	public ServerAdminPlayerId[] HistoryIds = new ServerAdminPlayerId[0];
}
