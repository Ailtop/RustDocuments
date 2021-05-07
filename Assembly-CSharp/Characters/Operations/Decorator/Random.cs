using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class Random : CharacterOperation
	{
		[SerializeField]
		[Subcomponent]
		private Subcomponents _toRandom;

		public override void Initialize()
		{
			_toRandom.Initialize();
		}

		public override void Run(Character owner)
		{
			CharacterOperation characterOperation = _toRandom.components.Random();
			if (!(characterOperation == null))
			{
				characterOperation.Run(owner);
			}
		}

		public override void Stop()
		{
			_toRandom.Stop();
		}
	}
}
