using System.Collections;
using FX;
using Singletons;
using UnityEngine;

namespace SkulStories
{
	public class PlayNarration : Sequence
	{
		[SerializeField]
		private SoundInfo _soundInfo;

		private const float _delay = 0.5f;

		public override IEnumerator CRun()
		{
			if (!(_soundInfo.audioClip == null) && _narration.sceneVisible)
			{
				float length = _soundInfo.length;
				if (_soundInfo.length == 0f)
				{
					length = _soundInfo.audioClip.length;
				}
				yield return CWaitForTime(0.5f);
				PlaySound();
				yield return CWaitForTime(length);
			}
		}

		private void PlaySound()
		{
			if (_narration.sceneVisible)
			{
				PersistentSingleton<SoundManager>.Instance.PlayNarrationSound(_soundInfo);
			}
		}

		private IEnumerator CWaitForTime(float length)
		{
			float elapsed = 0f;
			while (length > elapsed)
			{
				elapsed += Chronometer.global.deltaTime;
				yield return null;
				if (_narration.skipped || !_narration.sceneVisible)
				{
					PersistentSingleton<SoundManager>.Instance.StopNarrationSound();
					StopAllCoroutines();
					break;
				}
			}
		}
	}
}
