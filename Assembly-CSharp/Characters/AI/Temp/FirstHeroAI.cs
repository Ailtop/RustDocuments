using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Temp
{
	public class FirstHeroAI : AIController
	{
		[SerializeField]
		private LayerMask _adventLayerMask;

		[SerializeField]
		private Action _adventIdle;

		[SerializeField]
		private MotionTrail _motionTrail;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _advent;

		[SerializeField]
		[Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _dash;

		[SerializeField]
		[Subcomponent(typeof(TeleportInRangeWithFly))]
		private TeleportInRangeWithFly _teleportInRangeWithFly;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _commboAttack1;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _commboAttack2;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _commboAttack3;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _energyBlast;

		[SerializeField]
		[Subcomponent(typeof(CircularProjectileAttack))]
		private CircularProjectileAttack _circularProjectileAttack;

		[SerializeField]
		private Character _aura;

		[SerializeField]
		private Action _auraAction;

		[SerializeField]
		private GameObject _auraEffect;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _advent, _dash, _energyBlast };
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
			yield return DoAdvent();
			_auraAction.Initialize(_aura);
			StartCoroutine(DoRangeContinuousAttack());
			_auraEffect.SetActive(true);
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
				}
				else
				{
					yield return DoComoboAttack();
				}
			}
		}

		private IEnumerator DoComoboAttack()
		{
			float num = base.target.transform.position.x - character.transform.position.x;
			character.lookingDirection = ((!(num > 0f)) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			yield return DoDash();
			yield return _commboAttack1.CRun(this);
			yield return DoDash();
			yield return _commboAttack2.CRun(this);
			yield return DoDash();
			yield return _commboAttack3.CRun(this);
		}

		private IEnumerator DoAdvent()
		{
			RaycastHit2D point;
			if (base.target.movement.TryBelowRayCast(_adventLayerMask, out point, 20f))
			{
				character.transform.position = point.point;
			}
			else
			{
				character.transform.position = base.target.transform.position;
			}
			yield return _advent.CRun(this);
			_adventIdle.TryStart();
			while (_adventIdle.running)
			{
				yield return null;
			}
		}

		private IEnumerator DoDash()
		{
			yield return _teleportInRangeWithFly.CRun(this);
			if (character.transform.position.x > base.target.transform.position.x)
			{
				character.lookingDirection = Character.LookingDirection.Left;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
			}
			base.destination = base.target.transform.position;
		}

		private IEnumerator DoEnergyBlast()
		{
			yield return DoDash();
			yield return _circularProjectileAttack.CRun(this);
		}

		private IEnumerator DoRangeContinuousAttack()
		{
			while (!base.dead)
			{
				if (_teleportInRangeWithFly.result == Characters.AI.Behaviours.Behaviour.Result.Doing)
				{
					yield return null;
					continue;
				}
				yield return Chronometer.global.WaitForSeconds(0.5f);
				if (_auraAction.gameObject.activeSelf)
				{
					_auraAction.TryStart();
				}
			}
		}
	}
}
