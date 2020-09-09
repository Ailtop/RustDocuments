using Rust.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LookatTooltip : MonoBehaviour
{
	public static bool Enabled = true;

	[NonSerialized]
	public BaseEntity currentlyLookingAt;

	public RustText textLabel;

	public Image icon;

	public CanvasGroup canvasGroup;

	public CanvasGroup infoGroup;
}
