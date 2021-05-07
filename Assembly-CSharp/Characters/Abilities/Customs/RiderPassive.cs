using System;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class RiderPassive : Ability
	{
		public class Instance : AbilityInstance<RiderPassive>
		{
			private float _remainCheckTime;

			private int _criticalChanceStack;

			public override int iconStacks => _criticalChanceStack;

			public Instance(Character owner, RiderPassive ability)
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

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCheckTime -= deltaTime;
				if (_remainCheckTime < 0f)
				{
					_remainCheckTime += 0.25f;
					UpdateStat();
				}
			}

			private void UpdateStat()
			{
				double num = Math.Min(owner.stat.GetPercentPoint(Stat.Kind.MovementSpeed) * (double)ability._criticalChancePerSpeed, (double)ability._maxCriticalChance * 0.01);
				_criticalChanceStack = (int)(num * 100.0);
				ability._stat.values[0].value = num;
				owner.stat.SetNeedUpdate();
			}
		}

		public const float _overlapInterval = 0.25f;

		private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.CriticalChance, 1.0));

		[SerializeField]
		private float _criticalChancePerSpeed;

		[SerializeField]
		private float _maxCriticalChance;

		public RiderPassive()
		{
		}

		public RiderPassive(Stat.Values stat)
		{
			_stat = stat;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
