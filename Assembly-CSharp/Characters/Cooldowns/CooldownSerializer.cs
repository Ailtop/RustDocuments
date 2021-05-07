using System;
using Characters.Cooldowns.Streaks;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Cooldowns
{
	[Serializable]
	public class CooldownSerializer : ICooldown
	{
		public enum Type
		{
			None,
			Time,
			Gauge,
			Custom,
			Damage,
			Kill
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private int _maxStack = 1;

		[SerializeField]
		private int _streakCount;

		[SerializeField]
		private float _streakTimeout;

		[SerializeField]
		private float _cooldownTime = 1f;

		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private int _requiredAmount;

		public Type type => _type;

		private ICooldown cooldown { get; set; }

		public None none { get; private set; }

		public Time time { get; private set; }

		public Gauge gauge { get; private set; }

		public Custom custom { get; private set; }

		public int maxStack => cooldown.maxStack;

		public int stacks
		{
			get
			{
				return cooldown.stacks;
			}
			set
			{
				cooldown.stacks = value;
			}
		}

		public bool canUse => cooldown.canUse;

		public float remainPercent => cooldown.remainPercent;

		public bool usedByStreak => streak.remains < streak.count;

		public IStreak streak => cooldown.streak;

		public event Action onReady
		{
			add
			{
				cooldown.onReady += value;
			}
			remove
			{
				cooldown.onReady -= value;
			}
		}

		public void Serialize()
		{
			if (cooldown == null)
			{
				switch (_type)
				{
				case Type.None:
					none = new None();
					cooldown = none;
					break;
				case Type.Time:
					time = new Time(_maxStack, _streakCount, _streakTimeout, _cooldownTime);
					cooldown = time;
					break;
				case Type.Gauge:
					gauge = new Gauge(_gauge, _requiredAmount, _streakCount, _streakTimeout);
					cooldown = gauge;
					break;
				case Type.Custom:
					custom = new Custom();
					cooldown = custom;
					break;
				}
			}
		}

		public void CopyCooldown(CooldownSerializer other)
		{
			if (_type != other._type)
			{
				Debug.LogError("CooldownSerializer type missmatching.");
				return;
			}
			Type type = _type;
			if (type == Type.Time)
			{
				time.remainTime = other.time.remainTime;
			}
		}

		public bool Consume()
		{
			return cooldown.Consume();
		}
	}
}
