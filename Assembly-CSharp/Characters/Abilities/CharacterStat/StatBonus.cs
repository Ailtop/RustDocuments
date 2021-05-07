using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonus : Ability
	{
		public class Instance : AbilityInstance<StatBonus>
		{
			public Instance(Character owner, StatBonus ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.stat.AttachValues(ability._stat);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(ability._stat);
			}
		}

		[SerializeField]
		private Stat.Values _stat;

		public StatBonus()
		{
		}

		public StatBonus(Stat.Values stat)
		{
			_stat = stat;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
