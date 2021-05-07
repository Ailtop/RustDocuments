using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities
{
	public class InMapAbilityAttacher : AbilityAttacher
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
			Singleton<Service>.Instance.levelManager.onMapLoaded += ResetAbility;
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				Singleton<Service>.Instance.levelManager.onMapLoaded -= ResetAbility;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		private void ResetAbility()
		{
			base.owner.ability.Remove(_abilityComponent.ability);
			base.owner.ability.Add(_abilityComponent.ability);
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
