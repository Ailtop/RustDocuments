using System.Collections;
using Characters.AI.Hero.LightSwords;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class LightChase : Behaviour
	{
		[SerializeField]
		private int _consecutiveTimes = 3;

		[SerializeField]
		private float _attackDelay;

		[SerializeField]
		private LightSwordFieldHelper _helper;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Behaviour _ready;

		[SerializeField]
		private Behaviour _shortReady;

		[SerializeField]
		private Behaviour _attack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _readyGhost;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _attackGhost;

		private void Awake()
		{
			_readyGhost.Initialize();
			_attackGhost.Initialize();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			LightSword sword2 = _helper.GetFarthestFromHero();
			if (sword2 == null)
			{
				base.result = Result.Success;
				yield break;
			}
			_destination.position = sword2.GetStuckPosition();
			sword2.Sign();
			yield return _ready.CRun(controller);
			sword2.Despawn();
			RunOperation(_readyGhost, controller.character);
			yield return _attack.CRun(controller);
			for (int i = 0; i < _consecutiveTimes - 1; i++)
			{
				sword2 = _helper.GetFarthestFromHero();
				if (sword2 == null)
				{
					break;
				}
				_destination.position = sword2.GetStuckPosition();
				sword2.Sign();
				yield return _shortReady.CRun(controller);
				sword2.Despawn();
				RunOperation(_attackGhost, controller.character);
				yield return _attack.CRun(controller);
			}
			base.result = Result.Success;
		}

		private void RunOperation(OperationInfos operation, Character owner)
		{
			operation.gameObject.SetActive(true);
			operation.Run(owner);
		}
	}
}
