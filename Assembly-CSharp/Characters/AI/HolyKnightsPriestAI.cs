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
	public sealed class HolyKnightsPriestAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _heal;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _holyLight;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _keepDistanceTrigger;

		[SerializeField]
		[Range(0f, 1f)]
		private float _counterChance;

		private bool _counter;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _heal, _holyLight };
			character.health.onTookDamage += TryCounterAttack;
		}

		private void TryCounterAttack([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!_counter && _holyLight.CanUse())
			{
				if (character.health.dead || base.dead || character.health.percent <= damageDealt)
				{
					_counter = true;
				}
				else if (_holyLight.result != Characters.AI.Behaviours.Behaviour.Result.Doing && MMMaths.Chance(_counterChance))
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
				if (!(base.target == null) && !base.stuned)
				{
					if (_counter && character.health.currentHealth > 0.0 && !base.dead)
					{
						yield return _holyLight.CRun(this);
						_counter = false;
					}
					if (FindClosestPlayerBody(_keepDistanceTrigger) != null && _keepDistance.CanUseBackMove())
					{
						yield return _keepDistance.CRun(this);
					}
					else if (_heal.CanUse())
					{
						yield return _heal.CRun(this);
					}
					else if (_holyLight.CanUse())
					{
						yield return _holyLight.CRun(this);
					}
				}
			}
		}
	}
}
