using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public sealed class StopMusic : CharacterOperation
	{
		[SerializeField]
		private float _fadeOutTime = 1f;

		public override void Run(Character owner)
		{
			PersistentSingleton<SoundManager>.Instance.FadeOutBackgroundMusic(_fadeOutTime);
		}
	}
}
