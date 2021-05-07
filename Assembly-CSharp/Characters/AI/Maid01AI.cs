using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class Maid01AI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _dashAfterAttack;

		[SerializeField]
		private Collider2D _attackCollider;

		[SerializeField]
		private Collider2D _dashAttackCollider;

		[SerializeField]
		[Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _dash;

		[SerializeField]
		private Action _dashBuff;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			StartCoroutine(CChangeStopTrigger());
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			while (!base.dead)
			{
				yield return null;
				if (base.target == null)
				{
					continue;
				}
				if (FindClosestPlayerBody(_dashAttackCollider) != null)
				{
					if (FindClosestPlayerBody(_attackCollider) != null)
					{
						yield return _attack.CRun(this);
						continue;
					}
					if (_dashBuff.canUse)
					{
						yield return CDoDash();
						yield return _dashAfterAttack.CRun(this);
						continue;
					}
					yield return _chase.CRun(this);
					if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
					{
						yield return _attack.CRun(this);
					}
				}
				else
				{
					yield return _chase.CRun(this);
					if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
					{
						yield return _attack.CRun(this);
					}
				}
			}
		}

		private IEnumerator CDoDash()
		{
			base.destination = base.target.transform.position;
			_dashBuff.TryStart();
			StartCoroutine(_dash.CRun(this));
			yield return CStopDash();
			if (!character.hit.action.running)
			{
				character.CancelAction();
			}
		}

		private IEnumerator CStopDash()
		{
			while (!base.dead)
			{
				yield return null;
				if (_dash.result == Characters.AI.Behaviours.Behaviour.Result.Doing)
				{
					if (base.stuned)
					{
						_dash.result = Characters.AI.Behaviours.Behaviour.Result.Done;
						break;
					}
					continue;
				}
				break;
			}
		}

		private IEnumerator CChangeStopTrigger()
		{
			while (!base.dead)
			{
				yield return null;
				if (_dashBuff.canUse)
				{
					stopTrigger = _dashAttackCollider;
				}
				else
				{
					stopTrigger = _attackCollider;
				}
			}
		}
	}
}
