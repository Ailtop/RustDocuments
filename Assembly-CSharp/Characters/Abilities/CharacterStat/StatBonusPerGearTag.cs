using System;
using Characters.Gear;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusPerGearTag : Ability
	{
		public class Instance : AbilityInstance<StatBonusPerGearTag>
		{
			private Stat.Values _stat;

			public override int iconStacks => owner.playerComponents.inventory.item.GetItemCountByTag(ability._tag);

			public Instance(Character owner, StatBonusPerGearTag ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerGearTag.Clone();
				owner.stat.AttachValues(_stat);
				UpdateStatBonus();
				owner.playerComponents.inventory.item.onChanged += UpdateStatBonus;
			}

			protected override void OnDetach()
			{
				owner.playerComponents.inventory.item.onChanged -= UpdateStatBonus;
				owner.stat.DetachValues(_stat);
			}

			private void UpdateStatBonus()
			{
				int itemCountByTag = owner.playerComponents.inventory.item.GetItemCountByTag(ability._tag);
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = (double)itemCountByTag * ability._statPerGearTag.values[i].value;
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Characters.Gear.Gear.Tag _tag;

		[SerializeField]
		private Stat.Values _statPerGearTag;

		public StatBonusPerGearTag()
		{
		}

		public StatBonusPerGearTag(Stat.Values stat)
		{
			_statPerGearTag = stat;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
