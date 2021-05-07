using System;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class UnknownSeed : Ability
	{
		public class Instance : AbilityInstance<UnknownSeed>
		{
			private const float _updateInterval = 0.2f;

			private float _remainUpdateTime;

			private Stat.Values _stat;

			public override Sprite icon
			{
				get
				{
					if (!(ability.component.healed > 0f))
					{
						return null;
					}
					return ability.defaultIcon;
				}
			}

			public override float iconFillAmount => 1f - ability.component.healed / ability._healToMax;

			public override int iconStacks => (int)(_stat.values[0].value * 100.0);

			public Instance(Character owner, UnknownSeed ability)
				: base(owner, ability)
			{
				_stat = ability._maxStat.Clone();
			}

			protected override void OnAttach()
			{
				owner.stat.AttachValues(_stat);
				UpdateStat(true);
				owner.health.onHealed += OnHealed;
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
				owner.health.onHealed -= OnHealed;
			}

			private void OnHealed(double healed, double overHealed)
			{
				ability.component.healed += (float)healed;
				if (ability.component.healed > ability._healToMax)
				{
					ability._changingOperations.Run(owner);
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainUpdateTime -= deltaTime;
				if (_remainUpdateTime < 0f)
				{
					_remainUpdateTime = 0.2f;
					UpdateStat();
				}
			}

			public void UpdateStat(bool force = false)
			{
				if (force || ability.component.healed != ability.component.healedBefore)
				{
					Stat.Value[] values = _stat.values;
					float multiplier = ability.component.healed / ability._healToMax;
					for (int i = 0; i < values.Length; i++)
					{
						values[i].value = ability._maxStat.values[i].GetMultipliedValue(multiplier);
					}
					owner.stat.SetNeedUpdate();
					ability.component.healedBefore = ability.component.healed;
				}
			}
		}

		[Space]
		[SerializeField]
		private Stat.Values _maxStat;

		[SerializeField]
		private float _healToMax;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _changingOperations;

		public UnknownSeedComponent component { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			_changingOperations.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
