using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class GiganticEntAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _rangeAttack;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _meleeAttack;

		[SerializeField]
		private Collider2D _meleeAttackCollider;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
					continue;
				}
				if (_meleeAttack.CanUse() && FindClosestPlayerBody(_meleeAttackCollider) != null)
				{
					yield return _meleeAttack.CRun(this);
				}
				yield return _rangeAttack.CRun(this);
			}
		}
	}
}
