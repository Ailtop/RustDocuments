using UnityEngine;

namespace Rust.UI.MainMenu
{
	public class Hero : SingletonComponent<Hero>
	{
		public CanvasGroup CanvasGroup;

		public Video VideoPlayer;

		public RustText TitleText;

		public RustText ButtonText;

		public RustButton ItemStoreButton;
	}
}
