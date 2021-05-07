using System;
using System.Collections;
using Characters.Abilities.Customs;
using Characters.Actions;
using FX;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Weapons.Rockstar
{
	public class RockstarMusic : MonoBehaviour
	{
		[Serializable]
		private class AmpMusic
		{
			[SerializeField]
			private Amp _amp;

			private bool _ampRunning;

			private float _currentBeat;

			private bool pause => Chronometer.global.timeScale <= 0f;

			public float currentBpm
			{
				set
				{
					_currentBeat = 60f / value;
				}
			}

			public float beatStart { private get; set; }

			public void Initialize(MonoBehaviour coroutineOwner)
			{
				_amp.onInstantiate += delegate
				{
					if (!_ampRunning)
					{
						coroutineOwner.StartCoroutine(CPlayAmpSound());
					}
				};
			}

			public void StopAmp()
			{
				_ampRunning = false;
			}

			private IEnumerator CPlayAmpSound()
			{
				_ampRunning = true;
				while (_amp.ampExists)
				{
					int timingIndex;
					yield return new WaitForSeconds(GetNextEffectTime(out timingIndex) - Time.time);
					if (!pause)
					{
						_amp.PlayAmpBeat(timingIndex);
					}
				}
				StopAmp();
			}

			private float GetNextEffectTime(out int timingIndex)
			{
				float num = Time.time - beatStart;
				float num2 = _currentBeat * (float)_amp.beat;
				float num3 = (float)(int)(num / num2) * num2;
				float[] timings = _amp.GetTimings();
				for (int i = 0; i < timings.Length; i++)
				{
					float num4 = beatStart + num3 + timings[i] * num2;
					if (Time.time < num4)
					{
						timingIndex = i;
						return num4;
					}
				}
				timingIndex = 0;
				return beatStart + num3 + num2 + timings[0] * num2;
			}
		}

		[SerializeField]
		[GetComponentInParent(false)]
		private Weapon _weapon;

		[Header("Solo")]
		[SerializeField]
		private SoundInfo _solo;

		private ReusableAudioSource _soloSource;

		private float _maxVolume;

		[SerializeField]
		[Tooltip("기본 공격 사운드의 BPM 값을 넣어주세요.")]
		private float _soloBpm = 95f;

		[SerializeField]
		[Tooltip("이 액션으로만 Solo 사운드가 시작됩니다.")]
		private Characters.Actions.Action[] _soundStartActions;

		[SerializeField]
		[Tooltip("패시브 발동 중이 아닐 때, 이 액션 중 단 하나라도 사용중이라면 Solo 사운드가 계속 재생됩니다.")]
		private Characters.Actions.Action[] _soundActions;

		[SerializeField]
		[Tooltip("Solo 사운드를 반복 재생하는 박자 길이입니다.\nBPM으로 입력한 값에 맞춰 실제 길이가 결정됩니다.")]
		private float _intervalBeat = 8f;

		[SerializeField]
		[Tooltip("Solo 사운드는 어떤 Sound Action도 사용중이지 않을 때 이 시간 만큼 페이드 되었다가 중지됩니다.")]
		private float _fadeTime = 0.6f;

		[Space]
		[Header("Passive")]
		[SerializeField]
		[GetComponentInParent(false)]
		private RockstarPassiveComponent _passive;

		[SerializeField]
		[Tooltip("패시브 사운드의 BPM 값을 넣어주세요.")]
		private float _passiveBpm = 185f;

		[SerializeField]
		[Tooltip("패시브 사운드의 첫 박자가 시작되기 까지의 시간입니다.")]
		private float _passiveIntro = 0.5f;

		[SerializeField]
		[Tooltip("패시브로 인해 사운드가 재생되는 박자 길이입니다.\n패시브 발동 이후 이 시간 동안 Solo 사운드가 들리지 않습니다.")]
		private float _passiveBeatDuration = 32f;

		[Space]
		[Header("Amp")]
		[SerializeField]
		private AmpMusic[] amps;

		private Coroutine _loopSound;

		private Coroutine _waitPassive;

		private Coroutine _ampPlayBeat;

		private bool _running;

		private bool _paused;

		private float _passiveStart = -100f;

		private float _soloVolume = 1f;

		private bool _ampRunning;

		private float soloBeat => 60f / _soloBpm;

		private float passiveBeat => 60f / _passiveBpm;

		private bool pause => Chronometer.global.timeScale <= 0f;

		private float currentBpm
		{
			set
			{
				AmpMusic[] array = amps;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].currentBpm = value;
				}
			}
		}

		private float beatStart
		{
			set
			{
				AmpMusic[] array = amps;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].beatStart = value;
				}
			}
		}

		private bool soundActionRunning
		{
			get
			{
				for (int i = 0; i < _soundActions.Length; i++)
				{
					if (_soundActions[i].running)
					{
						return true;
					}
				}
				return false;
			}
		}

		private bool passiveRunning => Time.time < _passiveStart + _passiveIntro + _passiveBeatDuration * passiveBeat;

		private void Awake()
		{
			_maxVolume = _solo.volume;
			currentBpm = _soloBpm;
			for (int i = 0; i < _soundStartActions.Length; i++)
			{
				_soundStartActions[i].onStart += StartSolo;
			}
			RockstarPassive obj = (RockstarPassive)_passive.ability;
			obj.onSummon = (System.Action)Delegate.Combine(obj.onSummon, new System.Action(StartPassive));
			AmpMusic[] array = amps;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Initialize(this);
			}
		}

		private void OnDisable()
		{
			SetBeat(_soloBpm);
			StopSolo();
			AmpMusic[] array = amps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].StopAmp();
			}
		}

		private void PlaySolo()
		{
			_soloSource = PersistentSingleton<SoundManager>.Instance.PlaySound(_solo, base.transform.position);
		}

		private void SetSoloVolume(float volume)
		{
			if (!(_soloSource == null))
			{
				_soloSource.audioSource.volume = volume * _maxVolume * PersistentSingleton<SoundManager>.Instance.masterVolume * PersistentSingleton<SoundManager>.Instance.sfxVolume;
			}
		}

		private void StartSolo()
		{
			if (!_running && !passiveRunning)
			{
				SetBeat(_soloBpm);
				_loopSound = StartCoroutine(CLoopSoundWhileAction());
			}
		}

		private void StopSolo()
		{
			_running = false;
			_soloSource?.Stop();
		}

		private void StartPassive()
		{
			_passiveStart = Time.time;
			SetBeat(_passiveBpm, _passiveIntro);
			StartCoroutine(CWaitForPassiveFinish());
		}

		private void SetBeat(float bpm, float beatInterval = 0f)
		{
			currentBpm = bpm;
			beatStart = Time.time + beatInterval;
		}

		private IEnumerator CLoopSoundWhileAction()
		{
			float repeatDelay = 0f;
			float fadeStart = 0f;
			_running = true;
			while ((soundActionRunning && !passiveRunning) || Time.time - fadeStart < _fadeTime)
			{
				if (pause)
				{
					if (!_paused)
					{
						_paused = true;
						_soloSource.Stop();
					}
					yield return null;
					continue;
				}
				if (_paused)
				{
					_paused = false;
				}
				repeatDelay -= Time.deltaTime;
				if (repeatDelay <= 0f)
				{
					PlaySolo();
					SetSoloVolume(1f);
					repeatDelay += _intervalBeat * soloBeat;
				}
				if (soundActionRunning && !passiveRunning)
				{
					SetSoloVolume(1f);
					fadeStart = Time.time;
				}
				else
				{
					SetSoloVolume(Mathf.Lerp(1f, 0f, (Time.time - fadeStart) / _fadeTime));
				}
				yield return null;
			}
			StopSolo();
		}

		private IEnumerator CWaitForPassiveFinish()
		{
			while (passiveRunning)
			{
				yield return null;
			}
			SetBeat(_soloBpm);
			if (soundActionRunning)
			{
				StartSolo();
			}
		}
	}
}
