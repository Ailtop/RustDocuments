using System.Collections;
using Characters.Abilities.Triggers;
using UnityEngine;

namespace Characters.Abilities
{
	public class TriggerAbilityAttacher : AbilityAttacher
	{
		[SerializeField]
		[TriggerComponent.Subcomponent]
		private TriggerComponent _trigger;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private CoroutineReference _cUpdateReference;

		private void Awake()
		{
			_trigger.onTriggered += OnTriggered;
		}

		private void OnTriggered()
		{
			base.owner.ability.Add(_abilityComponent.ability);
		}

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
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
				base.owner.ability.Remove(_abilityComponent.ability);
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
