using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class CaerleonAssassinAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _attack;

		[SerializeField]
		[Subcomponent(typeof(Confusing))]
		private Confusing _confusing;

		[SerializeField]
		[Chase.Subcomponent(true)]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(ChaseTeleport))]
		private ChaseTeleport _chaseTelport;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		private Collider2D _attackCollider;

		[SerializeField]
		private Collider2D _teleportTrigger;

		private bool _chaseTeleportAttack;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _chaseTelport, _idle };
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
			while (!base.dead)
			{
				yield return _wander.CRun(this);
				yield return _idle.CRun(this);
				StartCoroutine(ChaseWithJump());
				yield return Combat();
			}
		}

		private IEnumerator ChaseWithJump()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null) && _chaseTelport.CanUse() && _chaseTelport.result != Characters.AI.Behaviours.Behaviour.Result.Doing && base.target.movement.controller.isGrounded && !(FindClosestPlayerBody(_teleportTrigger) == null) && !(base.target.movement.controller.collisionState.lastStandingCollider == character.movement.controller.collisionState.lastStandingCollider))
				{
					StopAllBehaviour();
					character.movement.moveBackward = false;
					_chaseTeleportAttack = true;
					yield return _chaseTelport.CRun(this);
					character.ForceToLookAt(base.target.transform.position.x);
					yield return character.chronometer.master.WaitForSeconds(0.3f);
					yield return _attack.CRun(this);
					yield return _confusing.CRun(this);
					_chaseTeleportAttack = false;
				}
			}
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (base.target == null)
				{
					break;
				}
				if (!character.movement.controller.isGrounded || _chaseTelport.result == Characters.AI.Behaviours.Behaviour.Result.Doing || _chaseTeleportAttack)
				{
					continue;
				}
				if ((bool)FindClosestPlayerBody(_attackCollider))
				{
					yield return _attack.CRun(this);
					yield return _confusing.CRun(this);
					continue;
				}
				yield return _chase.CRun(this);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return _attack.CRun(this);
				}
			}
		}
	}
}
