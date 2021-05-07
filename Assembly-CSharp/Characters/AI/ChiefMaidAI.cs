using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class ChiefMaidAI : AIController
	{
		[SerializeField]
		private Collider2D _spawnCollider;

		[SerializeField]
		private Collider2D _keepDistanceCollider;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(SpawnEnemy))]
		private SpawnEnemy _spawnEnemy;

		[Header("ImpectBell")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _impactBell;

		[SerializeField]
		private Collider2D _impactBellTrigger;

		[SerializeField]
		private Transform _attackPoint;

		private void Awake()
		{
			_attackPoint.parent = null;
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
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null))
				{
					if ((bool)FindClosestPlayerBody(_spawnCollider))
					{
						yield return _spawnEnemy.CRun(this);
					}
					if ((bool)FindClosestPlayerBody(_impactBellTrigger) && _impactBell.CanUse())
					{
						ShiftAttackPoint();
						yield return _impactBell.CRun(this);
					}
					if ((bool)FindClosestPlayerBody(_keepDistanceCollider) && _keepDistance.CanUseBackMove())
					{
						character.movement.moveBackward = true;
						yield return _keepDistance.CRun(this);
						character.movement.moveBackward = false;
					}
				}
			}
		}

		private void ShiftAttackPoint()
		{
			_attackPoint.position = base.target.transform.position;
		}
	}
}
