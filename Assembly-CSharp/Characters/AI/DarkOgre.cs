using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class DarkOgre : AIController
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

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chaseAndAttack };
		}

		public void StartCombat()
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
				yield return _chaseAndAttack.CRun(this);
			}
		}
	}
}
