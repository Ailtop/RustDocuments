using System.Collections;
using Characters.Abilities;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class StrangeSubjectAI : AIController
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
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		private Collider2D _attackCollider;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _wanderSpeedAbilityComponent;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _chaseSpeedAbilityComponent;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _confusingSpeedAbilityComponent;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			character.ability.Add(_wanderSpeedAbilityComponent.ability);
			yield return _wander.CRun(this);
			character.ability.Remove(_wanderSpeedAbilityComponent.ability);
			yield return _idle.CRun(this);
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
					continue;
				}
				if ((bool)FindClosestPlayerBody(_attackCollider))
				{
					yield return CAttack();
					continue;
				}
				character.ability.Add(_chaseSpeedAbilityComponent.ability);
				yield return _chase.CRun(this);
				character.ability.Remove(_chaseSpeedAbilityComponent.ability);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CAttack();
				}
			}
		}

		private IEnumerator CAttack()
		{
			yield return _attack.CRun(this);
			character.ability.Add(_confusingSpeedAbilityComponent.ability);
			yield return _confusing.CRun(this);
			character.ability.Remove(_confusingSpeedAbilityComponent.ability);
		}
	}
}
