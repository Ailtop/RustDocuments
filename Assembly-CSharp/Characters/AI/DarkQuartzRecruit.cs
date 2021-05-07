using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class DarkQuartzRecruit : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(ChaseAndAttack))]
		private ChaseAndAttack _chaseAndAttack;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chaseAndAttack, _idle };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return _wander.CRun(this);
			while (!base.dead)
			{
				yield return _chaseAndAttack.CRun(this);
			}
		}
	}
}
