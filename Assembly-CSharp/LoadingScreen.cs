using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : SingletonComponent<LoadingScreen>
{
	public CanvasRenderer panel;

	public TextMeshProUGUI title;

	public TextMeshProUGUI subtitle;

	public Button skipButton;

	public Button cancelButton;

	public GameObject performanceWarning;

	public AudioSource music;

	public RectTransform serverInfo;

	public RustText serverName;

	public RustText serverPlayers;

	public RustLayout serverModeSection;

	public RustText serverMode;

	public RustText serverMap;

	public RustLayout serverTagsSection;

	public ServerBrowserTagList serverTags;

	public static bool isOpen
	{
		get
		{
			if ((bool)SingletonComponent<LoadingScreen>.Instance && (bool)SingletonComponent<LoadingScreen>.Instance.panel)
			{
				return SingletonComponent<LoadingScreen>.Instance.panel.gameObject.activeSelf;
			}
			return false;
		}
	}

	public static bool WantsSkip { get; private set; }

	public static string Text { get; private set; }

	public static void Update(string strType)
	{
	}

	public static void Update(string strType, string strSubtitle)
	{
	}
}
