using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class AlchemistAI : AIController
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
		[Attack.Subcomponent(true)]
		private Attack _attack;

		[SerializeField]
		[Chase.Subcomponent(true)]
		private Chase _chase;

		[SerializeField]
		private Collider2D _attackTrigger;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _idle, _attack, _chase };
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
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
					continue;
				}
				if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					if (base.target != null && base.target.movement != null && base.target.movement.isGrounded)
					{
						yield return _attack.CRun(this);
					}
					else
					{
						yield return null;
					}
					continue;
				}
				yield return _chase.CRun(this);
				if (base.target != null && base.target.movement != null && base.target.movement.isGrounded)
				{
					yield return _attack.CRun(this);
				}
				else
				{
					yield return null;
				}
			}
		}
	}
}
