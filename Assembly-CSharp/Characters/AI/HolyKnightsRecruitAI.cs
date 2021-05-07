using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class HolyKnightsRecruitAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _counterAttack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _comboAttack;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private CharacterAnimation _characterAnimation;

		[SerializeField]
		private AnimationClip _idleClipAfterWander;

		[SerializeField]
		private Collider2D _comboAttackTrigger;

		[SerializeField]
		[Range(0f, 1f)]
		private float _counterChance;

		private bool _counter;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _counterAttack, _comboAttack };
			character.health.onTookDamage += TryCounterAttack;
		}

		private void TryCounterAttack([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!_counter && _counterAttack.CanUse())
			{
				if (character.health.dead || base.dead || character.health.percent <= damageDealt)
				{
					_counter = true;
				}
				else if (_comboAttack.result != Characters.AI.Behaviours.Behaviour.Result.Doing && MMMaths.Chance(_counterChance))
				{
					StopAllBehaviour();
					_counter = true;
				}
			}
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
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			yield return _wander.CRun(this);
			_characterAnimation.SetIdle(_idleClipAfterWander);
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null) && !base.stuned)
				{
					if (_counter && character.health.currentHealth > 0.0 && !base.dead)
					{
						yield return _counterAttack.CRun(this);
						_counter = false;
					}
					if (FindClosestPlayerBody(_comboAttackTrigger) != null)
					{
						yield return _comboAttack.CRun(this);
					}
					else
					{
						yield return _chase.CRun(this);
					}
				}
			}
		}
	}
}
