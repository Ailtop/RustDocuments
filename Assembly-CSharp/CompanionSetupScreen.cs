using Rust.UI;
using UnityEngine;

public class CompanionSetupScreen : SingletonComponent<CompanionSetupScreen>
{
	public enum ScreenState
	{
		Loading = 0,
		Error = 1,
		NoServer = 2,
		NotSupported = 3,
		NotInstalled = 4,
		Disabled = 5,
		Enabled = 6,
		ShowHelp = 7
	}

	public const string PairedKey = "companionPaired";

	public GameObject instructionsBody;

	public GameObject detailsPanel;

	public GameObject loadingMessage;

	public GameObject errorMessage;

	public GameObject notSupportedMessage;

	public GameObject disabledMessage;

	public GameObject enabledMessage;

	public GameObject refreshButton;

	public GameObject enableButton;

	public GameObject disableButton;

	public GameObject pairButton;

	public RustText serverName;

	public RustButton helpButton;
}
