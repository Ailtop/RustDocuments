using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class OneByOne : CharacterOperation
	{
		[SerializeField]
		[Subcomponent]
		private Subcomponents _operations;

		[Tooltip("제일 먼저 실행될 위치를 임의로 지정")]
		[SerializeField]
		private bool _randomizeEntry = true;

		private int _index;

		public override void Initialize()
		{
			_operations.Initialize();
			if (_randomizeEntry)
			{
				_index = _operations.components.RandomIndex();
			}
		}

		public override void Run(Character owner)
		{
			CharacterOperation characterOperation = _operations.components[_index];
			_index = (_index + 1) % _operations.components.Length;
			if (!(characterOperation == null))
			{
				characterOperation.Run(owner);
			}
		}

		public override void Stop()
		{
			_operations.Stop();
		}
	}
}
