using UnityEngine;

namespace Rust.UI;

public class ReportBug : UIDialog
{
	public GameObject GetInformation;

	public GameObject Finished;

	public RustInput Subject;

	public RustInput Message;

	public RustButton ReportButton;

	public RustButtonGroup Category;

	public RustIcon ProgressIcon;

	public RustText ProgressText;
}
