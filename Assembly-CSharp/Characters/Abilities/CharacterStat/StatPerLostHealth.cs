using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatPerLostHealth : Ability
	{
		public class Instance : AbilityInstance<StatPerLostHealth>
		{
			private Stat.Values _stat;

			private double _firstStatValue;

			public override int iconStacks => (int)(_firstStatValue * 100.0);

			public Instance(Character owner, StatPerLostHealth ability)
				: base(owner, ability)
			{
				_stat = ability._statPerLostHealth.Clone();
			}

			protected override void OnAttach()
			{
				UpdateStat();
				owner.health.onChanged += UpdateStat;
				owner.stat.AttachValues(_stat);
			}

			protected override void OnDetach()
			{
				owner.health.onChanged -= UpdateStat;
				owner.stat.DetachValues(_stat);
			}

			private void UpdateStat()
			{
				double num = Math.Min(1.0 - owner.health.percent, ability._maxLostPercent) * 100.0;
				_firstStatValue = num * ability._statPerLostHealth.values[0].value;
				for (int i = 0; i < _stat.values.Length; i++)
				{
					double num2 = num * ability._statPerLostHealth.values[i].value;
					if (ability._statPerLostHealth.values[i].categoryIndex == Stat.Category.Percent.index)
					{
						num2 += 1.0;
					}
					_stat.values[i].value = num2;
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Stat.Values _statPerLostHealth;

		[SerializeField]
		[Range(0f, 1f)]
		private float _maxLostPercent;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
