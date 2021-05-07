using UnityEngine;

namespace Characters.Operations.Customs
{
	public sealed class MultiDecoy : CharacterOperation
	{
		[SerializeField]
		private OperationRunner _decoy;

		[SerializeField]
		private Transform _spawnPointContainer;

		public override void Run(Character owner)
		{
			int childCount = _spawnPointContainer.childCount;
			Shuffle();
			for (int i = 0; i < childCount - 1; i++)
			{
				OperationInfos operationInfos = _decoy.Spawn().operationInfos;
				operationInfos.transform.position = _spawnPointContainer.GetChild(i).position;
				operationInfos.Run(owner);
			}
			owner.transform.position = _spawnPointContainer.GetChild(childCount - 1).position;
		}

		private void Shuffle()
		{
			foreach (Transform item in _spawnPointContainer)
			{
				item.SetSiblingIndex(Random.Range(0, _spawnPointContainer.childCount - 1));
			}
		}
	}
}
