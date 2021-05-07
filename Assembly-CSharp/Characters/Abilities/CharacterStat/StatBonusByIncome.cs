using System;
using Data;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByIncome : Ability
	{
		public class Instance : AbilityInstance<StatBonusByIncome>
		{
			private int _remainGoldForStack;

			private int _remainDarkQuartzForStack;

			private int _stacks;

			private Stat.Values _stat;

			public Instance(Character owner, StatBonusByIncome ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				GameData.Currency.gold.onEarn += OnGoldEarn;
				GameData.Currency.darkQuartz.onEarn += OnDarkQuartzEarn;
				_stat = ability._statPerStack.Clone();
				owner.stat.AttachValues(_stat);
			}

			protected override void OnDetach()
			{
				GameData.Currency.gold.onEarn -= OnGoldEarn;
				GameData.Currency.darkQuartz.onEarn -= OnDarkQuartzEarn;
				owner.stat.DetachValues(_stat);
			}

			private void OnGoldEarn(int amount)
			{
				amount += _remainGoldForStack;
				AddStack(amount / ability._goldPerStack);
				_remainGoldForStack = amount % ability._goldPerStack;
			}

			private void OnDarkQuartzEarn(int amount)
			{
				amount += _remainDarkQuartzForStack;
				AddStack(amount / ability._darkQuartzPerStack);
				_remainDarkQuartzForStack = amount % ability._darkQuartzPerStack;
			}

			private void AddStack(int amount)
			{
				_stacks += amount;
				if (ability._maxStack > 0 && _stacks > ability._maxStack)
				{
					_stacks = ability._maxStack;
					GameData.Currency.gold.onEarn -= OnGoldEarn;
					GameData.Currency.darkQuartz.onEarn -= OnDarkQuartzEarn;
					UpdateStat();
				}
				else
				{
					UpdateStat();
				}
			}

			private void UpdateStat()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = (double)_stacks * ability._statPerStack.values[i].value;
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Stat.Values _statPerStack;

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		private int _goldPerStack;

		[SerializeField]
		private int _darkQuartzPerStack;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
