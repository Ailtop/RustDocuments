using System;
using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.Abilities
{
	public class DuringChargingAbilityAttacher : AbilityAttacher
	{
		[SerializeField]
		private float _cooldown;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private float _remainCooldown;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			Character character = base.owner;
			character.onStartCharging = (Action<Characters.Actions.Action>)Delegate.Combine(character.onStartCharging, new Action<Characters.Actions.Action>(OnStartCharging));
			Character character2 = base.owner;
			character2.onStopCharging = (Action<Characters.Actions.Action>)Delegate.Combine(character2.onStopCharging, new Action<Characters.Actions.Action>(OnEndCharging));
			Character character3 = base.owner;
			character3.onCancelCharging = (Action<Characters.Actions.Action>)Delegate.Combine(character3.onCancelCharging, new Action<Characters.Actions.Action>(OnEndCharging));
			StartCoroutine(CCooldown());
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				Character character = base.owner;
				character.onStartCharging = (Action<Characters.Actions.Action>)Delegate.Remove(character.onStartCharging, new Action<Characters.Actions.Action>(OnStartCharging));
				Character character2 = base.owner;
				character2.onStopCharging = (Action<Characters.Actions.Action>)Delegate.Remove(character2.onStopCharging, new Action<Characters.Actions.Action>(OnEndCharging));
				Character character3 = base.owner;
				character3.onCancelCharging = (Action<Characters.Actions.Action>)Delegate.Remove(character3.onCancelCharging, new Action<Characters.Actions.Action>(OnEndCharging));
				StopAllCoroutines();
				Detach();
			}
		}

		private IEnumerator CCooldown()
		{
			while (true)
			{
				_remainCooldown -= base.owner.chronometer.master.deltaTime;
				yield return null;
			}
		}

		private void Attach()
		{
			base.owner.ability.Add(_abilityComponent.ability);
		}

		private void Detach()
		{
			base.owner.ability.Remove(_abilityComponent.ability);
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}

		private void OnStartCharging(Characters.Actions.Action action)
		{
			if (!(_remainCooldown > 0f))
			{
				_remainCooldown = _cooldown;
				Attach();
			}
		}

		private void OnEndCharging(Characters.Actions.Action action)
		{
			Detach();
		}
	}
}
