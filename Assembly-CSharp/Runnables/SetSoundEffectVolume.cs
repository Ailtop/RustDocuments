using Data;
using UnityEngine;

namespace Runnables
{
	public class SetSoundEffectVolume : Runnable
	{
		[SerializeField]
		private int _value;

		private float _originalVolume;

		private void OnEnable()
		{
			_originalVolume = GameData.Settings.sfxVolume;
		}

		public override void Run()
		{
			_originalVolume = GameData.Settings.sfxVolume;
			GameData.Settings.sfxVolume = _value;
		}

		public void SetOriginalVolume()
		{
			GameData.Settings.sfxVolume = _originalVolume;
		}

		private void OnDisable()
		{
			SetOriginalVolume();
		}
	}
}
