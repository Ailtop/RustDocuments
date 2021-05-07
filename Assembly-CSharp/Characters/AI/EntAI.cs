using System.Collections;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class EntAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(ChaseAndAttack))]
		private ChaseAndAttack _chaseAndAttack;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			while (!base.dead)
			{
				yield return _wander.CRun(this);
				yield return _idle.CRun(this);
				yield return _chaseAndAttack.CRun(this);
			}
		}
	}
}
