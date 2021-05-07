using UnityEngine;

namespace Characters.Operations.Customs
{
	public sealed class StomArrow : CharacterOperation
	{
		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private Transform _spawnPointContainer;

		[SerializeField]
		private int _emptyCount = 2;

		private int[] _numbers;

		private void Awake()
		{
			_numbers = new int[_spawnPointContainer.childCount];
			for (int i = 0; i < _spawnPointContainer.childCount; i++)
			{
				_numbers[i] = i;
			}
		}

		public override void Run(Character owner)
		{
			_numbers.Shuffle();
			for (int i = 0; i < _spawnPointContainer.childCount - _emptyCount; i++)
			{
				int index = _numbers[i];
				Vector3 position = _spawnPointContainer.GetChild(index).position;
				OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
				operationInfos.transform.position = position;
				operationInfos.Run(owner);
			}
		}
	}
}
