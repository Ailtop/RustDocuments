using System.Collections;
using Singletons;
using UnityEngine;

namespace FX
{
	public class PlaySoundInfo : MonoBehaviour
	{
		[Information("SoundInfo를 오브젝트풀을 이용하지 않고 현재 게임오브젝트에서 곧바로 재생합니다. AudioSource는 자동으로 생성됩니다. length가 0이고 loop이면 무한히 지속됩니다. length가 0이고 loop가 아니면 AudioClip의 length만큼 재생됩니다. 즉 딱 한 번 재생됩니다.", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private bool _playOnEnable = true;

		[SerializeField]
		private SoundInfo _soundInfo;

		private AudioSource _audioSource;

		public bool playing { get; private set; }

		private void Awake()
		{
			if (PersistentSingleton<SoundManager>.Instance.sfxEnabled && !(_soundInfo.audioClip == null))
			{
				_audioSource = base.gameObject.AddComponent<AudioSource>();
				Vector3 position = _audioSource.transform.position;
				position.z = Camera.main.transform.position.z;
				_audioSource.transform.position = position;
				_audioSource.clip = _soundInfo.audioClip;
				_audioSource.priority = _soundInfo.priority;
				_audioSource.panStereo = _soundInfo.stereoPan;
				_audioSource.bypassEffects = _soundInfo.bypassEffects;
				_audioSource.bypassListenerEffects = _soundInfo.bypassListenerEffects;
				_audioSource.bypassReverbZones = _soundInfo.bypassReverbZones;
				_audioSource.loop = _soundInfo.loop;
				_audioSource.spatialBlend = _soundInfo.spatialBlend;
				_audioSource.playOnAwake = _playOnEnable;
				UpdateVolume();
				if (!_playOnEnable)
				{
					_audioSource.Stop();
				}
			}
		}

		private void OnEnable()
		{
			if (_playOnEnable && (_soundInfo.length != 0f || !_soundInfo.loop))
			{
				StartCoroutine(CPlay());
			}
		}

		private void OnDisable()
		{
			playing = false;
		}

		public IEnumerator CPlay()
		{
			if (_soundInfo.audioClip == null)
			{
				Debug.LogWarning("AudioClip of PlaySoundInfo is null. Object name " + base.name + ".");
				yield break;
			}
			float length = _soundInfo.length;
			if (_soundInfo.length == 0f)
			{
				length = _soundInfo.audioClip.length;
			}
			UpdateVolume();
			_audioSource.Play();
			playing = true;
			yield return new WaitForSecondsRealtime(length);
			_audioSource.Stop();
			playing = false;
		}

		public void Play()
		{
			if (!playing)
			{
				StartCoroutine(CPlay());
			}
		}

		public void Stop()
		{
			if (playing)
			{
				StopAllCoroutines();
				_audioSource.Stop();
				playing = false;
			}
		}

		private void UpdateVolume()
		{
			_audioSource.volume = PersistentSingleton<SoundManager>.Instance.sfxVolume * _soundInfo.volume * PersistentSingleton<SoundManager>.Instance.masterVolume;
		}
	}
}
