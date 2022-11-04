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

	public RectTransform demoInfo;

	public RustText demoName;

	public RustText demoLength;

	public RustText demoDate;

	public RustText demoMap;

	public RawImage backgroundImage;

	public Texture2D defaultBackground;

	public GameObject pingWarning;

	public RustText pingWarningText;

	[Tooltip("Ping must be at least this many ms higher than the server browser ping")]
	public int minPingDiffToShowWarning = 50;

	[Tooltip("Ping must be this many times higher than the server browser ping")]
	public float pingDiffFactorToShowWarning = 2f;

	[Tooltip("Number of ping samples required before showing the warning")]
	public int requiredPingSampleCount = 10;

	public GameObject blackout;

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
