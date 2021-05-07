using Characters.Controllers;
using Data;
using FX;
using Scenes;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UserInput;

namespace UI.Pause
{
	public class Panel : Dialogue
	{
		public enum State
		{
			Menu,
			Controls,
			Settings
		}

		[SerializeField]
		private SoundInfo _openSound;

		[SerializeField]
		private SoundInfo _closeSound;

		[SerializeField]
		private SoundInfo _selectSound;

		[SerializeField]
		private Menu _menu;

		[SerializeField]
		private Controls _controls;

		[SerializeField]
		private Settings _settings;

		private State _state;

		private EnumArray<State, Dialogue> _statePanels;

		public State state
		{
			get
			{
				return _state;
			}
			set
			{
				_statePanels[_state].Close();
				_statePanels[value].Open();
				_state = value;
			}
		}

		public override bool closeWithPauseKey => false;

		private void Awake()
		{
			_statePanels = new EnumArray<State, Dialogue>(_menu, _controls, _settings);
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddComponent<PlaySoundOnSelected>().soundInfo = _selectSound;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			state = State.Menu;
			PersistentSingleton<SoundManager>.Instance.PlaySound(_openSound, Vector3.zero);
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
		}

		protected override void OnDisable()
		{
			if (!Service.quitting)
			{
				base.OnDisable();
				PersistentSingleton<SoundManager>.Instance.PlaySound(_closeSound, Vector3.zero);
				_statePanels[_state].Close();
				PlayerInput.blocked.Detach(this);
				Chronometer.global.DetachTimeScale(this);
			}
		}

		private void Update()
		{
			if (KeyMapper.Map.Pause.WasPressed)
			{
				Return();
			}
		}

		public void Return()
		{
			if (base.gameObject.activeSelf)
			{
				if (_menu.focused || _settings.focused || _controls.focused)
				{
					if (state == State.Menu)
					{
						Close();
					}
					else
					{
						state = State.Menu;
					}
				}
			}
			else if (!Scene<GameBase>.instance.uiManager.npcConversation.visible && !Scene<GameBase>.instance.uiManager.testingTool.gameObject.activeSelf)
			{
				Open();
			}
		}

		public void ReturnToTitleScreen()
		{
			GameData.Generic.tutorial.Stop();
			PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
			SceneManager.LoadScene(0);
			base.gameObject.SetActive(false);
		}
	}
}
