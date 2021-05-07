using System.Collections;
using Characters.Operations;
using UnityEngine;

namespace Characters.AI.Behaviours.Chimera
{
	public class Stomp : Behaviour
	{
		[SerializeField]
		private float _coolTime;

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

		private bool _coolDown = true;

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
			StartCoroutine(CoolDown(controller.character.chronometer.master));
			_attackOperations.gameObject.SetActive(true);
			_attackOperations.Run(controller.character);
			base.result = Result.Done;
			yield break;
		}

		private IEnumerator CoolDown(Chronometer chronometer)
		{
			_coolDown = false;
			yield return chronometer.WaitForSeconds(_coolTime);
			_coolDown = true;
		}

		public bool CanUse(AIController controller)
		{
			if (!_coolDown)
			{
				return false;
			}
			return controller.FindClosestPlayerBody(_trigger) != null;
		}
	}
}
