using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByShield : Ability
	{
		public class Instance : AbilityInstance<StatBonusByShield>
		{
			private Stat.Values _stat;

			private int _iconStacks;

			public override Sprite icon
			{
				get
				{
					if (_iconStacks <= 0)
					{
						return null;
					}
					return ability.defaultIcon;
				}
			}

			public override int iconStacks => _iconStacks;

			public Instance(Character owner, StatBonusByShield ability)
				: base(owner, ability)
			{
				_stat = ability._statPerStack.Clone();
			}

			protected override void OnAttach()
			{
				owner.stat.AttachValues(_stat);
				owner.health.onTookDamage += OnOwnerTookDamage;
				owner.health.shield.onAdd += OnShieldChanged;
				owner.health.shield.onUpdate += OnShieldChanged;
				owner.health.shield.onRemove += OnShieldChanged;
				UpdateStat();
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
				owner.health.onTookDamage -= OnOwnerTookDamage;
				owner.health.shield.onAdd -= OnShieldChanged;
				owner.health.shield.onUpdate -= OnShieldChanged;
				owner.health.shield.onRemove -= OnShieldChanged;
			}

			private void OnShieldChanged(Characters.Shield.Instance shieldInstance)
			{
				UpdateStat();
			}

			private void OnOwnerTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (owner.health.shield.hasAny && tookDamage.attackType != 0)
				{
					UpdateStat();
				}
			}

			public void UpdateStat()
			{
				double num = owner.health.shield.amount * ability._stackMultiplier;
				_iconStacks = (int)(num * ability._iconStacksPerStack);
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerStack.values[i].GetStackedValue(num);
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		[Tooltip("실드량에 이 값을 곱한 숫자가 스택이 됨")]
		private double _stackMultiplier = 1.0;

		[SerializeField]
		[Tooltip("실제 스택 1개당 아이콘 상에 표시할 스택")]
		private double _iconStacksPerStack = 1.0;

		[SerializeField]
		private Stat.Values _statPerStack;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
