using FX.SpriteEffects;
using UnityEngine;

namespace Characters.Abilities
{
	public abstract class AbilityInstance : IAbilityInstance
	{
		private ReusableChronoSpriteEffect _loopEffect;

		private GenericSpriteEffect _spriteEffect;

		public readonly Character owner;

		public readonly Ability ability;

		public float remainTime { get; set; }

		public bool attached { get; private set; }

		public virtual Sprite icon => ability.defaultIcon;

		public virtual float iconFillAmount
		{
			get
			{
				if (ability.duration != float.PositiveInfinity)
				{
					return 1f - remainTime / ability.duration;
				}
				return 0f;
			}
		}

		public bool iconFillInversed => ability.iconFillInversed;

		public bool iconFillFlipped => ability.iconFillFlipped;

		public virtual int iconStacks => 0;

		public bool expired => remainTime <= 0f;

		Character IAbilityInstance.owner => owner;

		IAbility IAbilityInstance.ability => ability;

		public AbilityInstance(Character owner, Ability ability)
		{
			this.owner = owner;
			this.ability = ability;
			remainTime = ability.duration;
		}

		public virtual void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
		}

		public virtual void Refresh()
		{
			remainTime = ability.duration;
		}

		public void Attach()
		{
			attached = true;
			_loopEffect = ((ability.loopEffect == null) ? null : ability.loopEffect.Spawn(owner.transform.position, owner));
			if (owner.spriteEffectStack != null && ability.spriteEffect != null && ability.spriteEffect.enabled)
			{
				_spriteEffect = ability.spriteEffect.CreateInstance();
				owner.spriteEffectStack.Add(_spriteEffect);
			}
			ability.effectOnAttach?.Spawn(owner.transform.position, owner);
			OnAttach();
		}

		public void Detach()
		{
			attached = false;
			if (!(owner == null))
			{
				if (_loopEffect != null)
				{
					_loopEffect.reusable.Despawn();
					_loopEffect = null;
				}
				if (owner.spriteEffectStack != null && ability.spriteEffect != null && ability.spriteEffect.enabled)
				{
					owner.spriteEffectStack.Remove(_spriteEffect);
				}
				ability.effectOnDetach?.Spawn(owner.transform.position, owner);
				OnDetach();
			}
		}

		protected abstract void OnAttach();

		protected abstract void OnDetach();
	}
	public abstract class AbilityInstance<T> : AbilityInstance where T : Ability
	{
		public new readonly T ability;

		public AbilityInstance(Character owner, T ability)
			: base(owner, ability)
		{
			this.ability = ability;
		}
	}
}
