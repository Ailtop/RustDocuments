using UnityEngine;

namespace Characters.Abilities
{
	public class DuringCombatAbilityAttacher : AbilityAttacher
	{
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.playerComponents.combatDetector.onBeginCombat += Attach;
			base.owner.playerComponents.combatDetector.onFinishCombat += Detach;
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.playerComponents.combatDetector.onBeginCombat -= Attach;
				base.owner.playerComponents.combatDetector.onFinishCombat -= Detach;
				Detach();
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
	}
}
