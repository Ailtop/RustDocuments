using System;
using UnityEngine;

namespace FX
{
	[Serializable]
	public class SoundInfo
	{
		[SerializeField]
		private AudioClip _audioClip;

		[SerializeField]
		[Tooltip("0일 경우 AudioClip의 Length")]
		private float _length;

		[SerializeField]
		[Range(0f, 1f)]
		private float _volume = 1f;

		[SerializeField]
		private float _uniqueTime = 0.1f;

		[SerializeField]
		[Range(0f, 256f)]
		private int _priority = 128;

		[SerializeField]
		[Range(-1f, 1f)]
		private float _stereoPan;

		[SerializeField]
		private bool _bypassEffects;

		[SerializeField]
		private bool _bypassListenerEffects;

		[SerializeField]
		private bool _bypassReverbZones;

		[SerializeField]
		private bool _loop;

		[SerializeField]
		[Range(0f, 1f)]
		private float _spatialBlend;

		public AudioClip audioClip => _audioClip;

		public float length => _length;

		public float volume => _volume;

		public float uniqueTime => _uniqueTime;

		public int priority => _priority;

		public float stereoPan => _stereoPan;

		public bool bypassEffects => _bypassEffects;

		public bool bypassListenerEffects => _bypassListenerEffects;

		public bool bypassReverbZones => _bypassReverbZones;

		public bool loop => _loop;

		public float spatialBlend => _spatialBlend;

		public SoundInfo(AudioClip clip)
		{
			_audioClip = clip;
		}
	}
}
