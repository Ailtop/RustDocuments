using FX;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public sealed class PlayMusic : CharacterOperation
	{
		[SerializeField]
		private MusicInfo _audioClipInfo;

		public override void Run(Character owner)
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_audioClipInfo);
		}
	}
}
