using System.Collections;
using Characters.Abilities.Triggers;
using UnityEngine;

namespace Characters.Abilities
{
	public class RandomTriggerAbilityAttacher : AbilityAttacher
	{
		[SerializeField]
		[TriggerComponent.Subcomponent]
		private TriggerComponent _trigger;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent.Subcomponents _abilityComponents;

		private CoroutineReference _cUpdateReference;

		private void Awake()
		{
			_trigger.onTriggered += OnTriggered;
		}

		private void OnTriggered()
		{
			base.owner.ability.Add(_abilityComponents.components.Random().ability);
		}

		public override void OnIntialize()
		{
			_abilityComponents.Initialize();
		}

		public override void StartAttach()
		{
			_trigger.Attach(base.owner);
			_cUpdateReference = this.StartCoroutineWithReference(CUpdate());
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				_trigger.Detach();
				_cUpdateReference.Stop();
				AbilityComponent[] components = _abilityComponents.components;
				foreach (AbilityComponent abilityComponent in components)
				{
					base.owner.ability.Remove(abilityComponent.ability);
				}
			}
		}

		private IEnumerator CUpdate()
		{
			while (true)
			{
				_trigger.UpdateTime(Chronometer.global.deltaTime);
				yield return null;
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
