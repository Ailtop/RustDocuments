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
	public sealed class HolyKnightsWizardAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(ChaseTeleport))]
		private ChaseTeleport _chaseTeleport;

		[SerializeField]
		[Subcomponent(typeof(CircularProjectileAttack))]
		private CircularProjectileAttack _homingRangeAttack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _radialRangeAttack;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		[Range(0f, 1f)]
		private float _counterChance;

		private bool _counter;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _chaseTeleport, _homingRangeAttack, _radialRangeAttack };
			character.health.onTookDamage += TryCounterAttack;
		}

		private void TryCounterAttack([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!_counter && _radialRangeAttack.CanUse())
			{
				if (character.health.dead || base.dead || character.health.percent <= damageDealt)
				{
					_counter = true;
				}
				else if (_radialRangeAttack.result != Characters.AI.Behaviours.Behaviour.Result.Doing && MMMaths.Chance(_counterChance))
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
			while (!base.dead)
			{
				yield return null;
				if (base.target == null || base.stuned)
				{
					continue;
				}
				if (_counter && character.health.currentHealth > 0.0 && !base.dead)
				{
					yield return _radialRangeAttack.CRun(this);
					_counter = false;
				}
				if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					if (!_radialRangeAttack.CanUse())
					{
						yield return _homingRangeAttack.CRun(this);
					}
					else if (MMMaths.RandomBool())
					{
						yield return _homingRangeAttack.CRun(this);
					}
					else
					{
						yield return _radialRangeAttack.CRun(this);
					}
				}
				else
				{
					yield return _chaseTeleport.CRun(this);
				}
			}
		}
	}
}
