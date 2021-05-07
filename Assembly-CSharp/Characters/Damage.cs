using System;
using UnityEngine;

namespace Characters
{
	public struct Damage
	{
		public enum AttackType
		{
			None,
			Melee,
			Ranged,
			Projectile,
			Additional
		}

		public enum MotionType
		{
			Basic,
			Skill,
			Item,
			Quintessence,
			Status,
			Dash
		}

		public enum Attribute
		{
			Physical,
			Magic,
			Fixed
		}

		public readonly Attacker attacker;

		public double @base;

		public readonly Attribute attribute;

		public readonly AttackType attackType;

		public readonly MotionType motionType;

		public readonly string key;

		public bool fixedDamage;

		public bool critical;

		public double multiplier;

		public float stoppingPower;

		public double criticalChance;

		public double criticalDamageMultiplier;

		public readonly Vector2 hitPoint;

		public double amount
		{
			get
			{
				if (attackType == AttackType.None)
				{
					return 0.0;
				}
				if (fixedDamage)
				{
					return Math.Ceiling(@base);
				}
				double num = @base * multiplier;
				if (critical)
				{
					num *= criticalDamageMultiplier;
				}
				return Math.Ceiling(num);
			}
		}

		public Damage(Attacker attacker, double @base, Vector2 hitPoint, Attribute attribute, AttackType attackType, MotionType motionType, double multiplier = 1.0, float stoppingPower = 0f, double criticalChance = 0.0, double criticalDamageMultiplier = 1.0, bool fixedDamage = false)
		{
			this.attacker = attacker;
			this.@base = @base;
			this.multiplier = multiplier;
			this.attribute = attribute;
			this.attackType = attackType;
			this.motionType = motionType;
			key = string.Empty;
			this.hitPoint = hitPoint;
			critical = false;
			this.stoppingPower = stoppingPower;
			this.criticalChance = criticalChance;
			this.criticalDamageMultiplier = criticalDamageMultiplier;
			this.fixedDamage = fixedDamage;
		}

		public Damage(Attacker attacker, double @base, Vector2 hitPoint, Attribute attribute, AttackType attackType, MotionType motionType, string key, double multiplier = 1.0, float stoppingPower = 0f, double criticalChance = 0.0, double criticalDamageMultiplier = 1.0, bool fixedDamage = false)
		{
			this.attacker = attacker;
			this.@base = @base;
			this.multiplier = multiplier;
			this.attribute = attribute;
			this.attackType = attackType;
			this.motionType = motionType;
			this.key = key;
			this.hitPoint = hitPoint;
			critical = false;
			this.stoppingPower = stoppingPower;
			this.criticalChance = criticalChance;
			this.criticalDamageMultiplier = criticalDamageMultiplier;
			this.fixedDamage = fixedDamage;
		}

		public void Evaluate(bool immuneToCritical)
		{
			if (!immuneToCritical && motionType != MotionType.Item && motionType != MotionType.Quintessence && motionType != MotionType.Status)
			{
				critical = MMMaths.Chance(criticalChance);
			}
		}

		public override string ToString()
		{
			return amount.ToString("0");
		}
	}
}
