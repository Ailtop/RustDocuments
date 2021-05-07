using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class Chance : CharacterOperation
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _successChance = 0.5f;

		[SerializeField]
		[Subcomponent]
		private CharacterOperation _onSuccess;

		[SerializeField]
		[Subcomponent]
		private CharacterOperation _onFail;

		public override void Initialize()
		{
			if (_onSuccess != null)
			{
				_onSuccess.Initialize();
			}
			if (_onFail != null)
			{
				_onFail.Initialize();
			}
		}

		public override void Run(Character owner)
		{
			if (MMMaths.Chance(_successChance))
			{
				if (!(_onSuccess == null))
				{
					_onSuccess.Stop();
					_onSuccess.Run(owner);
				}
			}
			else if (!(_onFail == null))
			{
				_onFail.Stop();
				_onFail.Run(owner);
			}
		}

		public override void Stop()
		{
			if (_onSuccess != null)
			{
				_onSuccess.Stop();
			}
			if (_onFail != null)
			{
				_onFail.Stop();
			}
		}
	}
}
