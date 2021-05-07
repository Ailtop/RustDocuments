using System.Collections;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace GameEvent
{
	public class FirstHeroAdvent : MonoBehaviour
	{
		[SerializeField]
		private float _duration = 120f;

		[SerializeField]
		private float _notificationTime = 10f;

		[SerializeField]
		private GameObject _firstHero;

		[SerializeField]
		private string _notifyKey;

		[SerializeField]
		private MusicInfo _backgroundMusic;

		private GameObject _spawned;

		private void Start()
		{
			StartCoroutine(WaitForAdvent());
		}

		private IEnumerator WaitForAdvent()
		{
			yield return Chronometer.global.WaitForSeconds(_duration - _notificationTime);
			PersistentSingleton<SoundManager>.Instance.FadeOutBackgroundMusic(4f);
			yield return Chronometer.global.WaitForSeconds(_notificationTime);
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_backgroundMusic);
			_spawned = Object.Instantiate(_firstHero);
		}

		private void OnDestroy()
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(Singleton<Service>.Instance.levelManager.currentChapter.currentStage.music);
			Object.Destroy(_spawned);
		}
	}
}
