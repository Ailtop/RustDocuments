using Singletons;
using UnityEngine;

namespace FX
{
	public class NarrationAudioSource : PersistentSingleton<SoundManager>
	{
		[SerializeField]
		[GetComponent]
		private AudioSource _audioSource;

		private const float _narraionVolum = 1f;

		public void Play(AudioClip clip, float masterVolume)
		{
			_audioSource.volume = 1f * masterVolume;
			_audioSource.clip = clip;
			_audioSource.Play();
		}

		public void Play(SoundInfo info, float masterVolume)
		{
			_audioSource.clip = info.audioClip;
			_audioSource.volume = 1f * masterVolume;
			_audioSource.priority = info.priority;
			_audioSource.panStereo = info.stereoPan;
			_audioSource.bypassEffects = info.bypassEffects;
			_audioSource.bypassListenerEffects = info.bypassListenerEffects;
			_audioSource.bypassReverbZones = info.bypassReverbZones;
			_audioSource.loop = info.loop;
			_audioSource.spatialBlend = info.spatialBlend;
			_audioSource.Play();
		}

		public void Stop()
		{
			_audioSource.Stop();
		}
	}
}
