using UnityEngine;

namespace Rust.UI;

public class ReportPlayer : UIDialog
{
	public GameObject FindPlayer;

	public GameObject GetInformation;

	public GameObject Finished;

	public GameObject RecentlyReported;

	public Dropdown ReasonDropdown;

	public RustInput Subject;

	public RustInput Message;

	public RustButton ReportButton;

	public SteamUserButton SteamUserButton;

	public RustIcon ProgressIcon;

	public RustText ProgressText;

	public static Option[] ReportReasons = new Option[5]
	{
		new Option(new Translate.Phrase("report.reason.none", "Select an option"), "none", isdefault: false, Icons.Bars),
		new Option(new Translate.Phrase("report.reason.abuse", "Racism/Sexism/Abusive"), "abusive", isdefault: false, Icons.Angry),
		new Option(new Translate.Phrase("report.reason.cheat", "Cheating"), "cheat", isdefault: false, Icons.Crosshairs),
		new Option(new Translate.Phrase("report.reason.spam", "Spamming"), "spam", isdefault: false, Icons.Bullhorn),
		new Option(new Translate.Phrase("report.reason.name", "Offensive Name"), "name", isdefault: false, Icons.Radiation)
	};
}
