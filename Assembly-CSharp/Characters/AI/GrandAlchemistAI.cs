using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class GrandAlchemistAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _flaskAttack;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _summonAttack;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wanderAfterAttack;

		[SerializeField]
		private Collider2D _attackTrigger;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _idle, _chase, _flaskAttack, _summonAttack };
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
				if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					if (_summonAttack.CanUse())
					{
						yield return _summonAttack.CRun(this);
					}
					else
					{
						if (!character.movement.controller.isGrounded)
						{
							continue;
						}
						yield return _flaskAttack.CRun(this);
					}
					if (FindClosestPlayerBody(_attackTrigger) != null)
					{
						yield return _wanderAfterAttack.CRun(this);
					}
					else
					{
						yield return _idle.CRun(this);
					}
				}
				else
				{
					yield return _chase.CRun(this);
				}
			}
		}
	}
}
