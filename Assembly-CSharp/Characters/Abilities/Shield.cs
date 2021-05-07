using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class Shield : Ability
	{
		public class Instance : AbilityInstance<Shield>
		{
			private Characters.Shield.Instance _shieldInstance;

			public override int iconStacks => (int)_shieldInstance.amount;

			public Instance(Character owner, Shield ability)
				: base(owner, ability)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				_shieldInstance.amount = ability._amount;
			}

			private void OnShieldBroke()
			{
				ability.onBroke?.Invoke(_shieldInstance);
				owner.ability.Remove(this);
			}

			protected override void OnAttach()
			{
				_shieldInstance = owner.health.shield.Add(ability, ability._amount, OnShieldBroke);
			}

			protected override void OnDetach()
			{
				ability.onDetach?.Invoke(_shieldInstance);
				if (owner.health.shield.Remove(ability))
				{
					_shieldInstance = null;
				}
			}
		}

		[SerializeField]
		private float _amount;

		public float amount
		{
			get
			{
				return _amount;
			}
			set
			{
				_amount = value;
			}
		}

		public event Action<Characters.Shield.Instance> onBroke;

		public event Action<Characters.Shield.Instance> onDetach;

		public Shield()
		{
		}

		public Shield(float amount)
		{
			_amount = amount;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
