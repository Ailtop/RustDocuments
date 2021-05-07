using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class MusicPlayerOnMap : MonoBehaviour
	{
		[SerializeField]
		private MusicInfo _musicInfo;

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.type != Chapter.Type.Chapter5)
			{
				if (_musicInfo.audioClip != null)
				{
					PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_musicInfo);
				}
				else
				{
					PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
				}
			}
		}

		private void OnDestroy()
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(Singleton<Service>.Instance.levelManager.currentChapter.currentStage.music);
		}
	}
}
