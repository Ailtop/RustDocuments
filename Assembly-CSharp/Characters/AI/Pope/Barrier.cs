using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope
{
	public sealed class Barrier : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSpawnOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onCrackOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDespawnOperations;

		public void Spawn()
		{
			if (!(_onSpawnOperations == null))
			{
				_onSpawnOperations.gameObject.SetActive(true);
				_onSpawnOperations.Run(_owner);
			}
		}

		public void Crack()
		{
			if (!(_onCrackOperations == null))
			{
				_onCrackOperations.gameObject.SetActive(true);
				_onCrackOperations.Run(_owner);
			}
		}

		public void Despawn()
		{
			if (!(_onDespawnOperations == null))
			{
				_onDespawnOperations.gameObject.SetActive(true);
				_onDespawnOperations.Run(_owner);
			}
		}
	}
}
