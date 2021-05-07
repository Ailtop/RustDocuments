using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class Doomsday : Ability
	{
		public class Instance : AbilityInstance<Doomsday>
		{
			private double _stackedDamage;

			public override int iconStacks => (int)(_stackedDamage * ability._damageConversionRatio);

			public Instance(Character owner, Doomsday ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				Explode();
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (!(target.character == null) && gaveDamage.attribute == Damage.Attribute.Physical)
				{
					_stackedDamage += damageDealt;
					if (_stackedDamage > ability._maxBaseDamage)
					{
						_stackedDamage = ability._maxBaseDamage;
					}
				}
			}

			private void Explode()
			{
				ability.component.amount = (float)(_stackedDamage * ability._damageConversionRatio);
				ability._operations.Run(owner);
			}
		}

		[NonSerialized]
		public DoomsdayComponent component;

		[SerializeField]
		private double _maxBaseDamage = 9999.0;

		[Information("입힌 물리피해의 전환비율, 폭발 시 입히는 피해도 물리피해라서 스탯 효과를 받으므로 주의.", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private double _damageConversionRatio = 0.20000000298023224;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
