using Data;
using Scenes;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Pause
{
	public class Menu : Dialogue
	{
		[SerializeField]
		private Panel _panel;

		[SerializeField]
		private Button _continue;

		[SerializeField]
		private Button _newGame;

		[SerializeField]
		private Button _main;

		[SerializeField]
		private Button _controls;

		[SerializeField]
		private Button _settings;

		[SerializeField]
		private Button _quit;

		private GameObject _focusBefore;

		public override bool closeWithPauseKey => false;

		private void Awake()
		{
			_continue.onClick.AddListener(delegate
			{
				_panel.Close();
			});
			_newGame.onClick.AddListener(delegate
			{
				Scene<GameBase>.instance.uiManager.confirm.Open(Lingua.GetLocalizedString("label/pause/menu/newGame/confirm"), delegate
				{
					_panel.gameObject.SetActive(false);
					GameData.Generic.tutorial.Stop();
					Singleton<Service>.Instance.levelManager.ResetGame();
				});
			});
			_main.onClick.AddListener(delegate
			{
				Scene<GameBase>.instance.uiManager.confirm.Open(Lingua.GetLocalizedString("label/pause/menu/main/confirm"), _panel.ReturnToTitleScreen);
			});
			_controls.onClick.AddListener(delegate
			{
				_panel.state = Panel.State.Controls;
			});
			_settings.onClick.AddListener(delegate
			{
				_panel.state = Panel.State.Settings;
			});
			_quit.onClick.AddListener(delegate
			{
				Scene<GameBase>.instance.uiManager.confirm.Open(Lingua.GetLocalizedString("label/pause/menu/quit/confirm"), Application.Quit);
			});
		}
	}
}
