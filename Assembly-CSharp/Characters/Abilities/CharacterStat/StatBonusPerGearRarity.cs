using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusPerGearRarity : Ability
	{
		public class Instance : AbilityInstance<StatBonusPerGearRarity>
		{
			private int _stack;

			private Stat.Values _stat;

			public override int iconStacks => _stack;

			public Instance(Character owner, StatBonusPerGearRarity ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerGearTag.Clone();
				UpdateStack();
				owner.stat.AttachValues(_stat);
				UpdateStatBonus();
				owner.playerComponents.inventory.onUpdated += UpdateStatBonus;
			}

			protected override void OnDetach()
			{
				owner.playerComponents.inventory.onUpdated -= UpdateStatBonus;
				owner.stat.DetachValues(_stat);
			}

			private void UpdateStatBonus()
			{
				UpdateStack();
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerGearTag.values[i].GetStackedValue(_stack);
				}
				owner.stat.SetNeedUpdate();
			}

			private void UpdateStack()
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				int itemCountByRarity = owner.playerComponents.inventory.item.GetItemCountByRarity(ability._rarity);
				int countByRarity = owner.playerComponents.inventory.weapon.GetCountByRarity(ability._rarity);
				int countByRarity2 = owner.playerComponents.inventory.quintessence.GetCountByRarity(ability._rarity);
				_stack = itemCountByRarity + countByRarity + countByRarity2;
			}
		}

		[SerializeField]
		private Rarity _rarity;

		[SerializeField]
		private Stat.Values _statPerGearTag;

		public StatBonusPerGearRarity()
		{
		}

		public StatBonusPerGearRarity(Stat.Values stat)
		{
			_statPerGearTag = stat;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
