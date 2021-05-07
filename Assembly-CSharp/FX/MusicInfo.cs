using System;
using UnityEngine;

namespace FX
{
	[Serializable]
	public class MusicInfo
	{
		[SerializeField]
		private AudioClip _audioClip;

		[SerializeField]
		[Range(0f, 1f)]
		private float _volume = 1f;

		[SerializeField]
		private bool _fade = true;

		[SerializeField]
		private bool _loop = true;

		[SerializeField]
		private bool _usePlayHistory = true;

		public AudioClip audioClip => _audioClip;

		public float volume => _volume;

		public bool fade => _fade;

		public bool loop => _loop;

		public bool usePlayHistory => _usePlayHistory;
	}
}
