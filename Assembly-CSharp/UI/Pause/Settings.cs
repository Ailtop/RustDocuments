using System.Collections.Generic;
using System.Linq;
using Achievements;
using Data;
using Scenes;
using Singletons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

namespace UI.Pause
{
	public class Settings : Dialogue
	{
		public const string key = "label/pause/settings";

		[SerializeField]
		private Panel _panel;

		[Header("Graphics")]
		[SerializeField]
		private Selection _resolution;

		[SerializeField]
		private Selection _screen;

		[SerializeField]
		private Selection _particleQuality;

		[SerializeField]
		private Slider _cameraShake;

		[SerializeField]
		private Slider _vibrationPower;

		[SerializeField]
		private Selection _light;

		[Header("Audio")]
		[Space]
		[SerializeField]
		private Slider _master;

		[SerializeField]
		private Slider _music;

		[SerializeField]
		private Slider _sfx;

		[Header("Data")]
		[Space]
		[SerializeField]
		private Button _resetData;

		[SerializeField]
		private Confirm _resetDataConfirm;

		[SerializeField]
		private Button _resetCutsceneData;

		[SerializeField]
		private Confirm _resetCutsceneDataConfirm;

		[Header("Game Play")]
		[Space]
		[SerializeField]
		private Selection _language;

		[SerializeField]
		private Selection _easyMode;

		[SerializeField]
		private Confirm _easyModeConfirm;

		[SerializeField]
		private Selection _showTimer;

		[SerializeField]
		private Selection _showUI;

		[Space]
		[SerializeField]
		private Button _return;

		private List<Resolution> _resolutionList;

		public override bool closeWithPauseKey => false;

		private void Awake()
		{
			LoadStrings();
			InitializeGraphicsOptions();
			InitializeAudioOptions();
			InitializeDataOptions();
			InitializeGameplayOptions();
			_return.onClick.AddListener(delegate
			{
				_panel.state = Panel.State.Menu;
			});
		}

		private void InitializeGraphicsOptions()
		{
			InitializeResolutionOption();
			_light.value = (GameData.Settings.lightEnabled ? 1 : 0);
			_light.onValueChanged += delegate(int v)
			{
				Light2D.lightEnabled = (GameData.Settings.lightEnabled = v == 1);
			};
		}

		private void InitializeResolutionOption()
		{
			_resolutionList = new List<Resolution>();
			Resolution[] resolutions = Screen.resolutions;
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution resolution = resolutions[i];
				bool flag = false;
				for (int j = 0; j < _resolutionList.Count; j++)
				{
					if (resolution.width == _resolutionList[j].width && resolution.height == _resolutionList[j].height)
					{
						flag = true;
						if (resolution.refreshRate > _resolutionList[j].refreshRate)
						{
							_resolutionList[j] = resolution;
						}
					}
				}
				if (!flag)
				{
					_resolutionList.Add(resolution);
				}
			}
			_resolution.SetTexts(_resolutionList.Select((Resolution r) => $"{r.width} x {r.height}").ToArray());
			int num = -1;
			for (int k = 0; k < _resolutionList.Count; k++)
			{
				if (_resolutionList[k].width == Screen.width && _resolutionList[k].height == Screen.height)
				{
					num = k;
				}
			}
			if (num == -1)
			{
				Resolution item = default(Resolution);
				item.width = Screen.width;
				item.height = Screen.height;
				item.refreshRate = Screen.currentResolution.refreshRate;
				_resolutionList.Add(item);
				num = _resolutionList.Count - 1;
			}
			_resolution.value = num;
		}

		private void InitializeAudioOptions()
		{
			_master.value = GameData.Settings.masterVolume;
			_master.onValueChanged.AddListener(delegate(float v)
			{
				GameData.Settings.masterVolume = v;
				PersistentSingleton<SoundManager>.Instance.UpdateMusicVolume();
			});
			_music.value = GameData.Settings.musicVolume;
			_music.onValueChanged.AddListener(delegate(float v)
			{
				GameData.Settings.musicVolume = v;
				PersistentSingleton<SoundManager>.Instance.UpdateMusicVolume();
			});
			_sfx.value = GameData.Settings.sfxVolume;
			_sfx.onValueChanged.AddListener(delegate(float v)
			{
				GameData.Settings.sfxVolume = v;
			});
		}

		private void InitializeDataOptions()
		{
			_resetData.onClick.AddListener(delegate
			{
				_resetDataConfirm.Open(string.Empty, delegate
				{
					GameData.Generic.ResetAll();
					GameData.Currency.ResetAll();
					GameData.Progress.ResetAll();
					GameData.Gear.ResetAll();
					_panel.ReturnToTitleScreen();
					Focus(_resetData);
				}, delegate
				{
					Focus(_resetData);
				});
			});
			_resetCutsceneData.onClick.AddListener(delegate
			{
				_resetCutsceneDataConfirm.Open(string.Empty, delegate
				{
					GameData.Progress.cutscene.ResetAll();
					GameData.Progress.skulstory.ResetAll();
					GameData.Progress.cutscene.SaveAll();
					GameData.Progress.skulstory.SaveAll();
					Focus(_resetCutsceneData);
				}, delegate
				{
					Focus(_resetCutsceneData);
				});
			});
		}

		private void InitializeGameplayOptions()
		{
			_language.value = GameData.Settings.language;
			_language.onValueChanged += delegate(int v)
			{
				GameData.Settings.language = v;
			};
			_cameraShake.value = GameData.Settings.cameraShakeIntensity;
			_cameraShake.onValueChanged.AddListener(delegate(float v)
			{
				GameData.Settings.cameraShakeIntensity = v;
			});
			_vibrationPower.value = GameData.Settings.vibrationIntensity;
			_vibrationPower.onValueChanged.AddListener(delegate(float v)
			{
				GameData.Settings.vibrationIntensity = v;
			});
			_particleQuality.value = GameData.Settings.particleQuality;
			_particleQuality.onValueChanged += delegate(int v)
			{
				GameData.Settings.particleQuality = v;
			};
			_easyMode.value = (GameData.Settings.easyMode ? 1 : 0);
			_easyMode.onValueChanged += delegate(int v)
			{
				if (v == 1)
				{
					_easyModeConfirm.Open(string.Empty, delegate
					{
						GameData.Settings.easyMode = true;
						Achievement.SetAchievement(Achievement.Type.RookieWelcome);
						Focus(_easyMode);
					}, delegate
					{
						_easyMode.SetValueWithoutNotify(0);
						EventSystem.current.SetSelectedGameObject(null);
						Focus(_easyMode);
					});
				}
				else
				{
					GameData.Settings.easyMode = false;
				}
			};
			_showTimer.value = (GameData.Settings.showTimer ? 1 : 0);
			_showTimer.onValueChanged += delegate(int v)
			{
				GameData.Settings.showTimer = ((v == 1) ? true : false);
			};
			_showUI.value = (int)Scene<GameBase>.instance.uiManager.hideOption;
			_showUI.onValueChanged += delegate(int v)
			{
				Scene<GameBase>.instance.uiManager.SetHideOption((UIManager.HideOption)v);
			};
		}

		private void LoadStrings()
		{
			_light.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/off", "label/pause/settings/on"));
			string text = "label/pause/settings/graphics/screen";
			_screen.SetTexts(Lingua.GetLocalizedStrings(text + "/borderless", text + "/fullscreen", text + "/windowed"));
			_language.SetTexts(Lingua.nativeNames.ToArray());
			_particleQuality.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/off", "label/pause/settings/low", "label/pause/settings/medium", "label/pause/settings/high"));
			_easyMode.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/off", "label/pause/settings/on"));
			_showTimer.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/off", "label/pause/settings/on"));
			_showUI.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/gamePlay/showUI/all", "label/pause/settings/gamePlay/showUI/hideHUD", "label/pause/settings/gamePlay/showUI/hideAll"));
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			LoadStrings();
			switch (Screen.fullScreenMode)
			{
			case FullScreenMode.FullScreenWindow:
				_screen.value = 0;
				break;
			case FullScreenMode.ExclusiveFullScreen:
				_screen.value = 1;
				break;
			case FullScreenMode.Windowed:
				_screen.value = 2;
				break;
			case FullScreenMode.MaximizedWindow:
				break;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ApplyDisplayOptions();
			GameData.Settings.Save();
		}

		private void ApplyDisplayOptions()
		{
			Resolution resolution = _resolutionList[_resolution.value];
			FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;
			switch (_screen.value)
			{
			case 0:
				fullscreenMode = FullScreenMode.FullScreenWindow;
				break;
			case 1:
				fullscreenMode = FullScreenMode.ExclusiveFullScreen;
				break;
			case 2:
				fullscreenMode = FullScreenMode.Windowed;
				break;
			}
			Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, resolution.refreshRate);
		}
	}
}
