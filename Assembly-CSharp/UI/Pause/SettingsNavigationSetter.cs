using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Pause
{
	public class SettingsNavigationSetter : MonoBehaviour
	{
		[SerializeField]
		private GameObject _graphics;

		private Selectable[] _graphicsSettings;

		[SerializeField]
		private GameObject _audio;

		private Selectable[] _audioSettings;

		[SerializeField]
		private GameObject _data;

		private Selectable[] _dataSettings;

		[SerializeField]
		private GameObject _gamePlay;

		private Selectable[] _gamePlaySettings;

		[SerializeField]
		private Selectable _return;

		private Dialogue _dialogue;

		private void Awake()
		{
			_dialogue = GetComponent<Dialogue>();
			_graphicsSettings = _graphics.GetComponentsInChildren<Selectable>();
			_audioSettings = _audio.GetComponentsInChildren<Selectable>();
			_dataSettings = _data.GetComponentsInChildren<Selectable>();
			_gamePlaySettings = _gamePlay.GetComponentsInChildren<Selectable>();
			SetNavigation(_graphicsSettings, null, _dataSettings);
			SetNavigation(_audioSettings, _dataSettings, _gamePlaySettings);
			SetNavigation(_dataSettings, _graphicsSettings, _audioSettings);
			SetNavigation(_gamePlaySettings, _audioSettings, null);
			Navigation navigation = _return.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnUp = _gamePlaySettings.Last();
			navigation.selectOnDown = _graphicsSettings.First();
			_return.navigation = navigation;
		}

		private void OnEnable()
		{
			Focus();
		}

		private void Focus()
		{
			_dialogue.Focus(_graphicsSettings.First());
		}

		private void SetNavigation(Selectable[] target, Selectable[] up, Selectable[] down)
		{
			for (int i = 0; i < target.Length; i++)
			{
				Navigation navigation = target[i].navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnUp = GetElementAtSafe(target, i - 1) ?? GetElementAtSafe(up, ((up != null) ? up.Length : 0) - 1) ?? _return;
				navigation.selectOnDown = GetElementAtSafe(target, i + 1) ?? GetElementAtSafe(down, 0) ?? _return;
				target[i].navigation = navigation;
			}
		}

		private T GetElementAtSafe<T>(IList<T> list, int index)
		{
			if (list == null)
			{
				return default(T);
			}
			if (index < 0 || index >= list.Count)
			{
				return default(T);
			}
			return list[index];
		}
	}
}
