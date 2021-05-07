using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class CaerleonArcherAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(HorizontalProjectileAttack))]
		private HorizontalProjectileAttack _attack;

		[SerializeField]
		[Chase.Subcomponent(true)]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		private Collider2D _minimumCollider;

		[SerializeField]
		private Collider2D _attackCollider;

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
			yield return _idle.CRun(this);
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
				}
				if (FindClosestPlayerBody(_attackCollider) != null)
				{
					yield return _attack.CRun(this);
					continue;
				}
				yield return _chase.CRun(this);
				if (_attack.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return _attack.CRun(this);
				}
			}
		}
	}
}
