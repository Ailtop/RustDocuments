using System;
using Characters.Movements;
using FX;
using FX.SpriteEffects;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Statuses
{
	public class Bleed : IAbility, IAbilityInstance
	{
		private const float _tickInterval = 0.66f;

		private const float _maxDamageMultiplierByDistance = 1.5f;

		private const float _smashExpiredDamageMultiplier = 1.5f;

		private const float _smashCollidedDamageMultiplier = 2f;

		private const string _floatingTextKey = "floating/status/bleed";

		private const string _floatingTextColor = "#d62d00";

		private static readonly ColorBlend _colorBlend = new ColorBlend(100, new Color(0.8f, 0f, 0f, 1f), 0f);

		private EffectInfo _effect;

		private float _remainTimeToNextTick;

		private float _movedDistance;

		private double _damagePerTick;

		private Character _attacker;

		public Character owner { get; private set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => null;

		public float iconFillAmount => remainTime / duration;

		public bool iconFillInversed => false;

		public bool iconFillFlipped => false;

		public int iconStacks => 0;

		public bool expired => remainTime <= 0f;

		public float duration { get; set; }

		public int iconPriority => 0;

		public bool removeOnSwapWeapon => false;

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public Bleed(Character owner)
		{
			this.owner = owner;
		}

		public void UpdateTime(float deltaTime)
		{
			if (!owner.movement.push.smash || owner.movement.push.expired)
			{
				float num = _movedDistance * 0.2f;
				float damageMultiplier = Mathf.Min(1f + _movedDistance * 0.5f, 1.5f);
				TakeTime(deltaTime + num, damageMultiplier);
				_movedDistance = 0f;
			}
		}

		private void TakeTime(float deltaTime, float damageMultiplier)
		{
			remainTime -= deltaTime;
			_remainTimeToNextTick -= deltaTime;
			if (_remainTimeToNextTick <= 0f)
			{
				_remainTimeToNextTick += 0.66f;
				GiveDamage(damageMultiplier);
			}
		}

		private void GiveDamage(float multiplier)
		{
			Damage damage = new Damage(owner, _damagePerTick * (double)multiplier, MMMaths.RandomPointWithinBounds(owner.collider.bounds), Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Status);
			_attacker.Attack(owner, ref damage);
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _effect.Spawn(MMMaths.RandomPointWithinBounds(owner.collider.bounds));
			if (MMMaths.RandomBool())
			{
				reusableChronoSpriteEffect.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}

		public void Add(Character attacker, float duration, double damagePerSecond)
		{
			_attacker = attacker;
			if (remainTime < duration)
			{
				remainTime = duration;
				this.duration = duration;
			}
			_damagePerTick = Math.Max(_damagePerTick, damagePerSecond * 0.6600000262260437);
		}

		private void AccumulateMovedDistance(Vector2 distance)
		{
			_movedDistance += Mathf.Abs(distance.x) + Mathf.Abs(distance.y);
		}

		private void onPushEnd(Push push, Character from, Character to, Push.SmashEndType endType, RaycastHit2D? raycastHit, Movement.CollisionDirection direction)
		{
			if (push.smash)
			{
				float damageMultiplier;
				switch (endType)
				{
				default:
					return;
				case Push.SmashEndType.Expire:
					damageMultiplier = 1.5f;
					break;
				case Push.SmashEndType.Collide:
					damageMultiplier = 2f;
					break;
				}
				for (float num = push.totalForce.magnitude; num > 0.66f; num -= 0.66f)
				{
					TakeTime(num, damageMultiplier);
				}
			}
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			remainTime = duration;
			_remainTimeToNextTick = 0f;
			owner.spriteEffectStack.Add(_colorBlend);
			owner.movement.onMoved += AccumulateMovedDistance;
			owner.movement.push.onEnd += onPushEnd;
			SpawnFloatingText();
		}

		public void Detach()
		{
			owner.spriteEffectStack.Remove(_colorBlend);
			owner.movement.onMoved -= AccumulateMovedDistance;
			owner.movement.push.onEnd -= onPushEnd;
		}

		public void Initialize()
		{
			_effect = new EffectInfo(Resource.instance.bleedEffect);
		}

		private void SpawnFloatingText()
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnStatus(Lingua.GetLocalizedString("floating/status/bleed"), vector, "#d62d00");
		}
	}
}
