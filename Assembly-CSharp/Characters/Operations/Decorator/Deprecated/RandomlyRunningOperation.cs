using UnityEngine;

namespace Characters.Operations.Decorator.Deprecated
{
	public class RandomlyRunningOperation : CharacterOperation
	{
		[SerializeField]
		[Range(1f, 100f)]
		private int _actuationrate;

		[SerializeField]
		[Subcomponent]
		private CharacterOperation _operation;

		public override void Initialize()
		{
			_operation.Initialize();
		}

		public override void Run(Character owner)
		{
			if (_actuationrate >= UnityEngine.Random.Range(1, 100))
			{
				_operation.Run(owner);
			}
		}

		public override void Stop()
		{
			_operation.Stop();
		}
	}
}
