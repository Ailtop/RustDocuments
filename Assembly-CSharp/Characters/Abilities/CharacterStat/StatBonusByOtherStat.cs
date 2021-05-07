using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByOtherStat : Ability
	{
		public class Instance : AbilityInstance<StatBonusByOtherStat>
		{
			private const float _updateInterval = 0.2f;

			private float _remainUpdateTime;

			private Stat.Values _stat;

			private double _cachedBonus;

			public override int iconStacks => (int)(_cachedBonus * 100.0);

			public override float iconFillAmount => 0f;

			public Instance(Character owner, StatBonusByOtherStat ability)
				: base(owner, ability)
			{
				_stat = ability._targetStats.Clone();
			}

			protected override void OnAttach()
			{
				owner.stat.AttachValues(_stat);
				UpdateStat(true);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainUpdateTime -= deltaTime;
				if (_remainUpdateTime < 0f)
				{
					_remainUpdateTime = 0.2f;
					UpdateStat(false);
				}
			}

			public void UpdateStat(bool force)
			{
				double num = (owner.stat.GetFinal(Stat.Kind.values[ability._sourceStat.kindIndex]) - 1.0) * (double)ability._conversionRatio;
				double value = ability._targetStats.values[0].value;
				if (num > ability._targetStats.values[0].value)
				{
					num = value;
				}
				if (force || num != _cachedBonus)
				{
					_cachedBonus = num;
					SetStat(num);
				}
			}

			private void SetStat(double bonus)
			{
				Stat.Value[] values = _stat.values;
				for (int i = 0; i < values.Length; i++)
				{
					values[i].value = bonus;
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[Header("Source Stat, 오른쪽 두개는 무시됨")]
		[SerializeField]
		private Stat.Value _sourceStat;

		[Header("스탯 타입 설정, 스탯값은 최대치를 의미함")]
		[SerializeField]
		private Stat.Values _targetStats;

		[SerializeField]
		private float _conversionRatio = 0.5f;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
