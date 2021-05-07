using System;
using System.Collections.Generic;
using FX.SpriteEffects;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Statuses
{
	public class Burn : IAbility, IAbilityInstance
	{
		private const float _tickInterval = 0.33f;

		private static readonly ColorBlend _colorBlend = new ColorBlend(100, new Color(8f / 15f, 142f / (339f * (float)Math.PI), 0f, 1f), 0f);

		private const string _floatingTextKey = "floating/status/burn";

		private const string _floatingTextColor = "#DD4900";

		private readonly List<Character> _attackers = new List<Character>();

		private readonly List<double> _damages = new List<double>();

		private readonly List<float> _remainTimes = new List<float>();

		private readonly double _damageMultiplier;

		private float _remainTimeToNextTick;

		private double _currentDamage;

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

		public Burn(Character owner)
		{
			this.owner = owner;
			Vector3 size = owner.collider.bounds.size;
			_damageMultiplier = 1.0;
		}

		public void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
			_remainTimeToNextTick -= deltaTime;
			if (_remainTimeToNextTick <= 0f)
			{
				_remainTimeToNextTick += 0.33f;
				GiveDamage();
			}
			bool flag = false;
			for (int num = _remainTimes.Count - 1; num >= 0; num--)
			{
				if ((_remainTimes[num] -= deltaTime) <= 0f)
				{
					_damages.RemoveAt(num);
					_remainTimes.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				UpdateDamage();
			}
		}

		private void GiveDamage()
		{
			Damage damage = new Damage(_attackers[_attackers.Count - 1], _currentDamage * _damageMultiplier, MMMaths.RandomPointWithinBounds(owner.collider.bounds), Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Status);
			_attackers[_attackers.Count - 1].Attack(owner, ref damage);
		}

		private void UpdateDamage()
		{
			_currentDamage = 0.0;
			for (int i = 0; i < _damages.Count; i++)
			{
				double num = _damages[i];
				if (_currentDamage < num)
				{
					_currentDamage = num;
				}
			}
		}

		public void Add(Character attacker, float duration, double damagePerSecond)
		{
			if (remainTime < duration)
			{
				remainTime = duration;
				this.duration = duration;
			}
			_attackers.Add(attacker);
			_damages.Add(damagePerSecond * 0.33000001311302185);
			_remainTimes.Add(duration);
			UpdateDamage();
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			remainTime = duration;
			_remainTimeToNextTick = 0f;
			owner.spriteEffectStack.Add(_colorBlend);
			SpawnFloatingText();
		}

		public void Detach()
		{
			owner.spriteEffectStack.Remove(_colorBlend);
		}

		public void Initialize()
		{
		}

		private void SpawnFloatingText()
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnStatus(Lingua.GetLocalizedString("floating/status/burn"), vector, "#DD4900");
		}
	}
}
