using System.Collections;
using Characters.Operations;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class Bite : Behaviour
	{
		[SerializeField]
		private Collider2D _trigger;

		[Header("Ready")]
		[SerializeField]
		private OperationInfos _readyOperations;

		[Header("Attack")]
		[SerializeField]
		private OperationInfos _attackOperations;

		[Header("Hit")]
		[SerializeField]
		private OperationInfos _terrainHitOperations;

		[Header("End")]
		[SerializeField]
		private OperationInfos _endOperations;

		private void Awake()
		{
			_readyOperations.Initialize();
			_attackOperations.Initialize();
			_endOperations.Initialize();
			_terrainHitOperations.Initialize();
		}

		public void Ready(Character character)
		{
			_readyOperations.gameObject.SetActive(true);
			_readyOperations.Run(character);
		}

		public void Hit(Character character)
		{
			_terrainHitOperations.gameObject.SetActive(true);
			_terrainHitOperations.Run(character);
		}

		public void End(Character character)
		{
			_endOperations.gameObject.SetActive(true);
			_endOperations.Run(character);
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_attackOperations.gameObject.SetActive(true);
			_attackOperations.Run(controller.character);
			base.result = Result.Done;
			yield break;
		}

		public bool CanUse(AIController controller)
		{
			return controller.FindClosestPlayerBody(_trigger) != null;
		}
	}
}
