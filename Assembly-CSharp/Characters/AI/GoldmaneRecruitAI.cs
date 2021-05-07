using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class GoldmaneRecruitAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _attack;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[Header("Guard")]
		[SerializeField]
		private Action _guard;

		[SerializeField]
		private double _breakDamage;

		[Header("Range")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private Collider2D _guardTrigger;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _wander, _attack, _chase, _idle };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			StartCoroutine(ChangeStopTrigger());
			while (!base.dead)
			{
				yield return _wander.CRun(this);
				yield return Combat();
			}
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (base.stuned || base.target == null || !character.movement.controller.isGrounded)
				{
					continue;
				}
				if (FindClosestPlayerBody(_guardTrigger) != null)
				{
					if (_guard.canUse)
					{
						yield return Guard();
					}
					else if (FindClosestPlayerBody(_attackTrigger) != null)
					{
						yield return _attack.CRun(this);
					}
					else
					{
						yield return _chase.CRun(this);
					}
				}
				else
				{
					yield return _chase.CRun(this);
				}
			}
		}

		private bool TryBreakGuard(ref Damage damage)
		{
			if (damage.amount >= _breakDamage)
			{
				character.CancelAction();
				return false;
			}
			character.health.onTakeDamage.Remove(TryBreakGuard);
			return true;
		}

		private IEnumerator Guard()
		{
			_guard.TryStart();
			while (_guard.running)
			{
				yield return null;
			}
			yield return _idle.CRun(this);
		}

		private IEnumerator ChangeStopTrigger()
		{
			while (!base.dead)
			{
				if (_guard.canUse)
				{
					stopTrigger = _guardTrigger;
				}
				else
				{
					stopTrigger = _attackTrigger;
				}
				yield return null;
			}
		}
	}
}
