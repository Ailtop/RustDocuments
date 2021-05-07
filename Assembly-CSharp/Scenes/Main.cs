using System.Collections;
using Data;
using InControl;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Scenes
{
	public class Main : Scene<Main>
	{
		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private AudioClip _backgroundMusic;

		[SerializeField]
		private GameObject _loading;

		[SerializeField]
		private GameObject _gameLogo;

		[SerializeField]
		private GameObject _pressAnyKey;

		[SerializeField]
		private GameObject _earlyAccessPopup;

		[SerializeField]
		private int _gameBaseSceneNumber;

		private void Awake()
		{
			_loading.SetActive(false);
			_gameLogo.SetActive(false);
			_pressAnyKey.SetActive(false);
		}

		private void Start()
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_backgroundMusic);
		}

		public void LogoAnimationsCompleted()
		{
			StartCoroutine(CStartGameOnReady());
		}

		private IEnumerator CStartGameOnReady()
		{
			yield return CWaitForBackgroundFadeIn();
			yield return CWaitForInput();
			_loading.SetActive(true);
			StartGame();
		}

		private IEnumerator CWaitForBackgroundFadeIn()
		{
			_gameLogo.SetActive(true);
			yield return new WaitForSeconds(1f);
		}

		private IEnumerator CWaitForInput()
		{
			_pressAnyKey.SetActive(true);
			while (!Input.anyKey && !InputManager.ActiveDevice.AnyButtonIsPressed)
			{
				yield return null;
			}
			_pressAnyKey.SetActive(false);
		}

		private void StartGame()
		{
			Chapter.Type chapter = (GameData.Generic.tutorial.isPlayed() ? Chapter.Type.Castle : Chapter.Type.Tutorial);
			Singleton<Service>.Instance.levelManager.Load(chapter);
			Object.Destroy(_container);
		}
	}
}
