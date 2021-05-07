using System.Collections;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations;
using Level.Chapter4;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class Parade : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		private Transform _toSpawnPoint;

		[SerializeField]
		private Transform _leftSpawnPoint;

		[SerializeField]
		private Transform _rightSpawnPoint;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		[SerializeField]
		private Scenario _scenario;

		private void Awake()
		{
			_scenario.OnPhase1End += delegate
			{
				_operations.StopAll();
			};
			_operations.Initialize();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (controller.target.transform.position.x > controller.transform.position.x)
			{
				_toSpawnPoint.position = _rightSpawnPoint.position;
			}
			else
			{
				_toSpawnPoint.position = _leftSpawnPoint.position;
			}
			StartCoroutine(_operations.CRun(controller.character));
			base.result = Result.Success;
			yield break;
		}
	}
}
