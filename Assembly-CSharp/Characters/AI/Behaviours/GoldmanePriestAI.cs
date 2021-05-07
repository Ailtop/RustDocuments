using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class GoldmanePriestAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _heal;

		[SerializeField]
		private Collider2D _minimumCollider;

		[SerializeField]
		private Collider2D _healRange;

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
				yield return null;
				if (base.target == null)
				{
					continue;
				}
				if (FindClosestPlayerBody(_minimumCollider) != null)
				{
					yield return _keepDistance.CRun(this);
					if (CanStartToHeal())
					{
						yield return _heal.CRun(this);
					}
				}
				else if (CanStartToHeal())
				{
					yield return _heal.CRun(this);
				}
			}
		}

		private bool CanStartToHeal()
		{
			foreach (Character item in FindEnemiesInRange(_healRange))
			{
				if (item.gameObject.activeSelf && !item.health.dead && item.health.percent < 1.0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
