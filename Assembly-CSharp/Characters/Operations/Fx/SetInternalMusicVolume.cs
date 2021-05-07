using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class SetInternalMusicVolume : Operation
	{
		[SerializeField]
		private float _volume;

		[SerializeField]
		private float _easeTime;

		[SerializeField]
		private AnimationCurve _easeCurve;

		public override void Run()
		{
			if (_easeTime > 0f)
			{
				PersistentSingleton<SoundManager>.Instance.SetInternalMusicVolume(_volume, _easeTime, _easeCurve);
			}
			else
			{
				PersistentSingleton<SoundManager>.Instance.SetInternalMusicVolume(_volume);
			}
		}

		public override void Stop()
		{
			PersistentSingleton<SoundManager>.Instance.ResetInternalMusicVolume();
		}
	}
}
