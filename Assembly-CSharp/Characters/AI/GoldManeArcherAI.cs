using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class GoldManeArcherAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistanceWithJump;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistanceWithMove;

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

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _keepDistanceWithJump, _keepDistanceWithMove, _attack, _idle };
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
			yield return _idle.CRun(this);
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			StartCoroutine(ProcessBackStep());
			while (!base.dead)
			{
				yield return null;
				if (base.target == null || base.stuned || !character.movement.controller.isGrounded || _keepDistanceWithJump.result == Characters.AI.Behaviours.Behaviour.Result.Doing || _keepDistanceWithMove.result == Characters.AI.Behaviours.Behaviour.Result.Doing || _attack.result == Characters.AI.Behaviours.Behaviour.Result.Doing || FindClosestPlayerBody(_minimumCollider) != null)
				{
					continue;
				}
				if (FindClosestPlayerBody(_attackCollider) != null)
				{
					yield return _attack.CRun(this);
					continue;
				}
				yield return _chase.CRun(this);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return _attack.CRun(this);
				}
			}
		}

		private IEnumerator ProcessBackStep()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(FindClosestPlayerBody(_minimumCollider) == null) && _attack.result != Characters.AI.Behaviours.Behaviour.Result.Doing)
				{
					StopAllBehaviour();
					if (_keepDistanceWithJump.CanUseBackStep())
					{
						yield return _keepDistanceWithJump.CRun(this);
					}
					else if (_keepDistanceWithMove.CanUseBackMove())
					{
						yield return _keepDistanceWithMove.CRun(this);
						yield return _attack.CRun(this);
					}
				}
			}
		}
	}
}
