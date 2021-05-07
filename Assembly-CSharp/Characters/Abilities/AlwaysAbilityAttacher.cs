using UnityEngine;

namespace Characters.Abilities
{
	public class AlwaysAbilityAttacher : AbilityAttacher
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
			base.owner.ability.Add(_abilityComponent.ability);
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
