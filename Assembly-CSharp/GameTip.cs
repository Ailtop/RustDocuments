using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameTip : SingletonComponent<GameTip>
{
	public enum Styles
	{
		Blue_Normal = 0,
		Red_Normal = 1,
		Blue_Long = 2,
		Blue_Short = 3,
		Server_Event = 4
	}

	[Serializable]
	public struct Theme
	{
		public Icons Icon;

		public Color BackgroundColor;

		public Color ForegroundColor;

		public float duration;
	}

	public CanvasGroup canvasGroup;

	public RustIcon icon;

	public Image background;

	public RustText text;

	public Theme[] themes;
}
