using System;
using Data;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class CurrencyBonus : Ability
	{
		public class Instance : AbilityInstance<CurrencyBonus>
		{
			public Instance(Character owner, CurrencyBonus ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				GameData.Currency.currencies[ability._type].multiplier.AddOrUpdate(this, ability._percentPoint);
			}

			protected override void OnDetach()
			{
				GameData.Currency.currencies[ability._type].multiplier.Remove(this);
			}
		}

		[SerializeField]
		private GameData.Currency.Type _type;

		[SerializeField]
		private float _percentPoint;

		public CurrencyBonus()
		{
		}

		public CurrencyBonus(GameData.Currency.Type type, float percentPoint)
		{
			_type = type;
			_percentPoint = percentPoint;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
