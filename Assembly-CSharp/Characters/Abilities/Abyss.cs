using System;
using Characters.Gear;
using Characters.Gear.Weapons;
using Characters.Operations;
using FX;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class Abyss : Ability
	{
		public class Instance : AbilityInstance<Abyss>
		{
			private Weapon _from;

			private float _remainHitTime;

			public Instance(Character owner, Abyss ability, Weapon from)
				: base(owner, ability)
			{
				_from = from;
			}

			protected override void OnAttach()
			{
				base.remainTime = ability.duration;
				_remainHitTime = ability._hitInterval;
				owner.stat.AttachValues(ability._stat);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(ability._stat);
			}

			public override void Refresh()
			{
				base.remainTime = ability.duration;
			}

			public override void UpdateTime(float deltaTime)
			{
				if (_from == null || _from.state != Characters.Gear.Gear.State.Equipped)
				{
					base.remainTime = 0f;
					return;
				}
				base.remainTime -= deltaTime;
				_remainHitTime -= deltaTime;
				if (_remainHitTime < 0f)
				{
					_remainHitTime += ability._hitInterval;
					Hit();
				}
			}

			private void Hit()
			{
				Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
				ability._hitEffect.Spawn(vector);
				if (!owner.invulnerable.value)
				{
					Damage damage = owner.stat.GetDamage(ability._attackDamage.amount, vector, ability._hitInfo);
					_from.owner.Attack(owner, ref damage);
					ability._operationOnHit.Run(_from.owner, owner);
				}
			}
		}

		[SerializeField]
		private Weapon _weapon;

		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		private float _hitInterval;

		[SerializeField]
		private AttackDamage _attackDamage;

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Additional);

		[SerializeField]
		private EffectInfo _hitEffect;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operationOnHit;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this, _weapon);
		}
	}
}
