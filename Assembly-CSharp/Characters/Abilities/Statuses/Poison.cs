using System.Collections.Generic;
using FX;
using FX.SpriteEffects;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Statuses
{
	public class Poison : IAbility, IAbilityInstance
	{
		private class Info
		{
			public readonly Character attacker;

			public readonly double damagePerTick;

			public int remainTicks;

			public double remainDamage => damagePerTick * (double)remainTicks;

			public Info(Character attacker, double damagePerTick, int ticks)
			{
				this.attacker = attacker;
				this.damagePerTick = damagePerTick;
				remainTicks = ticks;
			}
		}

		private static readonly ColorBlend _colorBlend = new ColorBlend(100, new Color(32f / 255f, 1f, 32f / 255f, 1f), 0f);

		private const float _tickInterval = 0.5f;

		private const string _floatingTextKey = "floating/status/poision";

		private const string _floatingTextColor = "#2cbb00";

		private EffectInfo _effect;

		private readonly List<Info> _infos = new List<Info>();

		private double _totalDamagePerTick;

		private float _remainTimeToNextTick;

		public Character owner { get; private set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => null;

		public float iconFillAmount => remainTime / duration;

		public bool iconFillInversed => false;

		public bool iconFillFlipped => false;

		public int iconStacks => 0;

		public bool expired => _infos.Count == 0;

		public float duration { get; set; }

		public int iconPriority => 0;

		public bool removeOnSwapWeapon => false;

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public Poison(Character owner)
		{
			this.owner = owner;
		}

		public void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
			_remainTimeToNextTick -= deltaTime;
			if (!(_remainTimeToNextTick <= 0f))
			{
				return;
			}
			_remainTimeToNextTick += 0.5f;
			GiveDamage(_infos[_infos.Count - 1].attacker, _totalDamagePerTick);
			bool flag = false;
			for (int num = _infos.Count - 1; num >= 0; num--)
			{
				Info info = _infos[num];
				info.remainTicks--;
				if (info.remainTicks == 0)
				{
					_infos.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				UpdateDamage();
			}
		}

		private void GiveDamage(Character attacker, double amount)
		{
			Damage damage = new Damage(attacker, amount, MMMaths.RandomPointWithinBounds(owner.collider.bounds), Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Status);
			attacker.Attack(owner, ref damage);
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _effect.Spawn(MMMaths.RandomPointWithinBounds(owner.collider.bounds));
			if (MMMaths.RandomBool())
			{
				reusableChronoSpriteEffect.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}

		private void UpdateDamage()
		{
			_totalDamagePerTick = 0.0;
			for (int i = 0; i < _infos.Count; i++)
			{
				_totalDamagePerTick += _infos[i].damagePerTick;
			}
		}

		public void Add(Character attacker, float duration, double damagePerSecond)
		{
			if (remainTime < duration)
			{
				remainTime = duration;
				this.duration = duration;
			}
			_infos.Add(new Info(attacker, damagePerSecond * 0.5, Mathf.FloorToInt(duration / 0.5f)));
			UpdateDamage();
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			remainTime = duration;
			owner.spriteEffectStack.Add(_colorBlend);
			SpawnFloatingText();
		}

		public void Detach()
		{
			owner.spriteEffectStack.Remove(_colorBlend);
		}

		public void Initialize()
		{
			_effect = new EffectInfo(Resource.instance.poisonEffect);
		}

		public double GetRemainDamage()
		{
			double num = 0.0;
			foreach (Info info in _infos)
			{
				num += info.remainDamage;
			}
			return num;
		}

		private void SpawnFloatingText()
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnStatus(Lingua.GetLocalizedString("floating/status/poision"), vector, "#2cbb00");
		}
	}
}
