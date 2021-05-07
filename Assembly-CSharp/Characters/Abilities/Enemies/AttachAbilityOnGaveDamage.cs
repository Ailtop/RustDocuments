using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities.Enemies
{
	[Serializable]
	public class AttachAbilityOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<AttachAbilityOnGaveDamage>
		{
			private float _remainCooldown;

			private IAbilityInstance _cache;

			public Instance(Character owner, AttachAbilityOnGaveDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldown -= deltaTime;
			}

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (_remainCooldown > 0f || target.character == null || target.character.health.dead || (!string.IsNullOrWhiteSpace(ability._attackKey) && !gaveDamage.key.Equals(ability._attackKey, StringComparison.OrdinalIgnoreCase)) || !ability._motionTypeFilter[gaveDamage.motionType] || !ability._damageTypeFilter[gaveDamage.attackType])
				{
					return;
				}
				if (ability._refreshIfHas)
				{
					if (_cache != null && _cache.attached)
					{
						RefreshCache();
						return;
					}
					_cache = target.character.ability.GetInstanceType(ability._abilityComponent.ability);
					if (_cache != null)
					{
						RefreshCache();
						return;
					}
				}
				_remainCooldown = ability._cooldown;
				target.character.ability.Add(ability._abilityComponent.ability);
			}

			private void RefreshCache()
			{
				_remainCooldown = ability._cooldown;
				_cache.Refresh();
			}
		}

		[Information("Add만 할 뿐 Remove를 하지 않으니 신중히 사용해야함", InformationAttribute.InformationType.Warning, true)]
		[SerializeField]
		private float _cooldown;

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _damageTypeFilter;

		[SerializeField]
		private string _attackKey;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		[SerializeField]
		private bool _refreshIfHas = true;

		public override void Initialize()
		{
			base.Initialize();
			_abilityComponent.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
