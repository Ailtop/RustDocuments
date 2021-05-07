using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Characters.Operations;
using UnityEngine;

namespace Characters
{
	public class Stat
	{
		public sealed class Category
		{
			public static readonly Category Constant;

			public static readonly Category Fixed;

			public static readonly Category Percent;

			public static readonly Category PercentPoint;

			public static readonly Category Final;

			public static readonly int count;

			public static readonly ReadOnlyCollection<Category> values;

			public static readonly ReadOnlyCollection<string> names;

			private static readonly string[] _names;

			private static Category[] _values;

			private static int _count;

			public readonly string name;

			public readonly string format;

			public readonly int index;

			static Category()
			{
				_values = new Category[5]
				{
					Constant = new Category("Constant", ""),
					Fixed = new Category("Fixed", ""),
					Percent = new Category("Percent", "%"),
					PercentPoint = new Category("PercentPoint", "%p"),
					Final = new Category("Final", "")
				};
				_names = _values.Select((Category kind) => kind.name).ToArray();
				values = Array.AsReadOnly(_values);
				names = Array.AsReadOnly(_names);
				count = _count;
			}

			private Category(string name, string format)
			{
				this.name = name;
				this.format = format;
				index = _count++;
			}

			public override string ToString()
			{
				return name;
			}
		}

		public sealed class Kind
		{
			public enum ValueForm
			{
				Constant,
				Percent,
				Product
			}

			public static readonly Kind Health;

			public static readonly Kind AttackDamage;

			public static readonly Kind PhysicalAttackDamage;

			public static readonly Kind MagicAttackDamage;

			public static readonly Kind TakingDamage;

			public static readonly Kind TakingPhysicalDamage;

			public static readonly Kind TakingMagicDamage;

			public static readonly Kind AttackSpeed;

			public static readonly Kind MovementSpeed;

			public static readonly Kind CriticalChance;

			public static readonly Kind CriticalDamage;

			public static readonly Kind EvasionChance;

			public static readonly Kind MeleeEvasionChance;

			public static readonly Kind RangedEvasionChance;

			public static readonly Kind ProjectileEvasionChance;

			public static readonly Kind StoppingResistance;

			public static readonly Kind KnockbackResistance;

			public static readonly Kind StatusResistance;

			public static readonly Kind StoppingPower;

			public static readonly Kind BasicAttackDamage;

			public static readonly Kind SkillAttackDamage;

			public static readonly Kind CooldownSpeed;

			public static readonly Kind SkillCooldownSpeed;

			public static readonly Kind DashCooldownSpeed;

			public static readonly Kind SwapCooldownSpeed;

			public static readonly Kind EssenceCooldownSpeed;

			public static readonly Kind BuffDuration;

			public static readonly Kind BasicAttackSpeed;

			public static readonly Kind SkillAttackSpeed;

			public static readonly Kind CharacterSize;

			public static readonly Kind DashDistance;

			public static readonly Kind StunResistance;

			public static readonly Kind FreezeResistance;

			public static readonly Kind BurnResistance;

			public static readonly Kind BleedResistance;

			public static readonly Kind PoisonResistance;

			public static readonly Kind ProjectileAttackDamage;

			public static readonly Kind TakingHealAmount;

			public static readonly Kind ChargingSpeed;

			public static readonly int count;

			public static readonly ReadOnlyCollection<Kind> values;

			public static readonly ReadOnlyCollection<string> names;

			private static readonly string[] _names;

			private static readonly Kind[] _values;

			private static int _count;

			public readonly ValueForm valueForm;

			public readonly string name;

			public readonly int index;

			static Kind()
			{
				_values = new Kind[39]
				{
					Health = new Kind("체력", ValueForm.Constant),
					AttackDamage = new Kind("공격력/모두", ValueForm.Percent),
					PhysicalAttackDamage = new Kind("공격력/물리", ValueForm.Percent),
					MagicAttackDamage = new Kind("공격력/마법", ValueForm.Percent),
					TakingDamage = new Kind("받는피해감소/모두", ValueForm.Product),
					TakingPhysicalDamage = new Kind("받는피해감소/물리", ValueForm.Product),
					TakingMagicDamage = new Kind("받는피해감소/마법", ValueForm.Product),
					AttackSpeed = new Kind("공격속도/모두", ValueForm.Percent),
					MovementSpeed = new Kind("이동속도", ValueForm.Constant),
					CriticalChance = new Kind("치명타 확률", ValueForm.Percent),
					CriticalDamage = new Kind("치명타 피해량", ValueForm.Percent),
					EvasionChance = new Kind("회피율/모두", ValueForm.Product),
					MeleeEvasionChance = new Kind("회피율/근접", ValueForm.Product),
					RangedEvasionChance = new Kind("회피율/원거리", ValueForm.Product),
					StoppingResistance = new Kind("경직저항", ValueForm.Product),
					KnockbackResistance = new Kind("넉백저항", ValueForm.Product),
					StatusResistance = new Kind("상태이상저항/모두", ValueForm.Product),
					ProjectileEvasionChance = new Kind("회피율/투사체", ValueForm.Product),
					StoppingPower = new Kind("저지력", ValueForm.Product),
					BasicAttackDamage = new Kind("공격력/기본", ValueForm.Percent),
					SkillAttackDamage = new Kind("공격력/스킬", ValueForm.Percent),
					CooldownSpeed = new Kind("쿨다운가속/모두", ValueForm.Percent),
					SkillCooldownSpeed = new Kind("쿨다운가속/스킬", ValueForm.Percent),
					DashCooldownSpeed = new Kind("쿨다운가속/대시", ValueForm.Percent),
					SwapCooldownSpeed = new Kind("쿨다운가속/교대", ValueForm.Percent),
					EssenceCooldownSpeed = new Kind("쿨다운가속/정수", ValueForm.Percent),
					BuffDuration = new Kind("버프 지속시간", ValueForm.Product),
					BasicAttackSpeed = new Kind("공격속도/기본", ValueForm.Percent),
					SkillAttackSpeed = new Kind("공격속도/스킬", ValueForm.Percent),
					CharacterSize = new Kind("크기", ValueForm.Percent),
					DashDistance = new Kind("대시거리", ValueForm.Percent),
					StunResistance = new Kind("상태이상저항/스턴", ValueForm.Product),
					FreezeResistance = new Kind("상태이상저항/빙결", ValueForm.Product),
					BurnResistance = new Kind("상태이상저항/화상", ValueForm.Product),
					BleedResistance = new Kind("상태이상저항/출혈", ValueForm.Product),
					PoisonResistance = new Kind("상태이상저항/중독", ValueForm.Product),
					ProjectileAttackDamage = new Kind("공격력/투사체", ValueForm.Percent),
					TakingHealAmount = new Kind("받는 회복량", ValueForm.Percent),
					ChargingSpeed = new Kind("공격속도/차지", ValueForm.Percent)
				};
				_names = _values.Select((Kind kind) => kind.name).ToArray();
				values = Array.AsReadOnly(_values);
				names = Array.AsReadOnly(_names);
				count = _count;
			}

			private Kind(string name, ValueForm type)
			{
				this.name = name;
				valueForm = type;
				index = _count++;
			}

			public override string ToString()
			{
				return name;
			}
		}

		[Serializable]
		public sealed class Value : ICloneable
		{
			public static readonly Value blockMovement = new Value(Category.Percent, Kind.MovementSpeed, 0.0);

			public int categoryIndex;

			public int kindIndex;

			public double value;

			internal static string positiveString => Lingua.GetLocalizedString("stat_Positive");

			internal static string negativeString => Lingua.GetLocalizedString("stat_Negative");

			public Value(Category category, Kind kind, double value)
			{
				categoryIndex = category.index;
				kindIndex = kind.index;
				this.value = value;
			}

			public Value(int categoryIndex, int kindIndex, double value)
			{
				this.categoryIndex = categoryIndex;
				this.kindIndex = kindIndex;
				this.value = value;
			}

			public Value Clone()
			{
				return new Value(categoryIndex, kindIndex, value);
			}

			object ICloneable.Clone()
			{
				return Clone();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsCategory(Category category)
			{
				return categoryIndex == category.index;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsCategory(int categoryIndex)
			{
				return this.categoryIndex == categoryIndex;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsKind(Kind.ValueForm valueForm)
			{
				return Kind.values[kindIndex].valueForm == valueForm;
			}

			public double GetStackedValue(double stacks)
			{
				if (IsKind(Kind.ValueForm.Product))
				{
					return 1.0 - (1.0 - value) * stacks;
				}
				if (IsCategory(Category.Percent))
				{
					return 1.0 + value * stacks;
				}
				return value * stacks;
			}

			public double GetMultipliedValue(float multiplier)
			{
				if (IsCategory(Category.Percent))
				{
					return (value - 1.0) * (double)multiplier + 1.0;
				}
				return (double)multiplier * value;
			}
		}

		[Serializable]
		public class Values : ReorderableArray<Value>, ICloneable
		{
			public static readonly Values blockMovement = new Values(Value.blockMovement);

			private List<string> _strings;

			public List<string> strings => _strings ?? (_strings = values.Select((Value v) => v.ToString()).ToList());

			public Values(params Value[] values)
			{
				base.values = values;
			}

			public Values Clone()
			{
				Value[] array = new Value[values.Length];
				for (int i = 0; i < values.Length; i++)
				{
					array[i] = values[i].Clone();
				}
				return new Values(array);
			}

			object ICloneable.Clone()
			{
				return Clone();
			}
		}

		public class ValuesWithEvent
		{
			public delegate void OnDetachDelegate(Stat stat);

			public readonly Values values;

			internal OnDetachDelegate _onDetach;

			internal ValuesWithEvent(Values values, OnDetachDelegate onDetach)
			{
				this.values = values;
				_onDetach = onDetach;
			}
		}

		private class TimedValues : ValuesWithEvent
		{
			private float _timeToExpire;

			public TimedValues(Values values, float duration, OnDetachDelegate onDetach = null)
				: base(values, onDetach)
			{
				_timeToExpire = duration;
			}

			public void SetTime(float time)
			{
				_timeToExpire = time;
			}

			public bool TakeTime(float time)
			{
				return (_timeToExpire -= time) <= 0f;
			}
		}

		private readonly Character _owner;

		private readonly double[,] _values = new double[Category.count, Kind.count];

		private bool _needUpdate;

		private readonly List<Values> _bonuses = new List<Values>();

		private readonly List<ValuesWithEvent> _bonusesWithEvent = new List<ValuesWithEvent>();

		private readonly List<TimedValues> _timedBonuses = new List<TimedValues>();

		public Stat getDamageOverridingStat { get; set; }

		public Stat(Character owner)
		{
			_owner = owner;
			SetAll(Category.Percent, 1.0);
		}

		public Stat(Character owner, Stat stat)
		{
			_owner = owner;
			SetAll(Category.Percent, 1.0);
			_values = (double[,])stat._values.Clone();
		}

		private void Initialize()
		{
			SetAll(Category.Constant, 0.0);
			SetAll(Category.Fixed, 0.0);
			SetAll(Category.Percent, 1.0);
			SetAll(Category.PercentPoint, 0.0);
			SetAll(Category.Final, 0.0);
			_values[Category.Percent.index, Kind.CriticalDamage.index] = 1.0;
			_values[Category.PercentPoint.index, Kind.CriticalDamage.index] = 0.5;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Apply(Values value)
		{
			for (int i = 0; i < value.values.Length; i++)
			{
				Apply(value.values[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Apply(Value value)
		{
			if (value.categoryIndex == Category.Percent.index)
			{
				_values[value.categoryIndex, value.kindIndex] *= value.value;
			}
			else
			{
				_values[value.categoryIndex, value.kindIndex] += value.value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetAll(Category category, double value)
		{
			for (int i = 0; i < Kind.count; i++)
			{
				_values[category.index, i] = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Get(Category category, Kind kind)
		{
			return _values[category.index, kind.index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetConstant(Kind kind)
		{
			return Get(Category.Constant, kind);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetFixed(Kind kind)
		{
			return Get(Category.Fixed, kind);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetPercent(Kind kind)
		{
			return Get(Category.Percent, kind);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetPercentPoint(Kind kind)
		{
			return Get(Category.PercentPoint, kind);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetFinal(Kind kind)
		{
			return Get(Category.Final, kind);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetFinalPercent(Kind kind)
		{
			return Get(Category.Percent, kind) * (1.0 + Get(Category.PercentPoint, kind));
		}

		public Damage GetDamage(double baseDamage, Vector2 hitPoint, HitInfo hitInfo)
		{
			if (getDamageOverridingStat != null)
			{
				return getDamageOverridingStat.GetDamage(baseDamage, hitPoint, hitInfo);
			}
			double num = 1.0;
			switch (hitInfo.attribute)
			{
			case Damage.Attribute.Physical:
				num += GetFinal(Kind.PhysicalAttackDamage) - 1.0;
				break;
			case Damage.Attribute.Magic:
				num += GetFinal(Kind.MagicAttackDamage) - 1.0;
				break;
			}
			Damage.AttackType attackType = hitInfo.attackType;
			if (attackType == Damage.AttackType.Projectile)
			{
				num += GetFinal(Kind.ProjectileAttackDamage) - 1.0;
			}
			switch (hitInfo.motionType)
			{
			case Damage.MotionType.Basic:
				num += GetFinal(Kind.BasicAttackDamage) - 1.0;
				break;
			case Damage.MotionType.Skill:
				num += GetFinal(Kind.SkillAttackDamage) - 1.0;
				break;
			}
			num *= GetFinal(Kind.AttackDamage);
			return new Damage(_owner, baseDamage * (double)hitInfo.damageMultiplier, hitPoint, hitInfo.attribute, hitInfo.attackType, hitInfo.motionType, hitInfo.key, num, hitInfo.stoppingPower * (float)GetFinal(Kind.StoppingPower), GetFinal(Kind.CriticalChance) - 1.0 + (double)hitInfo.extraCriticalChance, GetFinal(Kind.CriticalDamage) + (double)hitInfo.extraCriticalDamage, hitInfo.fixedDamage);
		}

		public float GetCooldownSpeed()
		{
			return (float)GetFinal(Kind.CooldownSpeed);
		}

		public float GetCooldownSpeed(Kind kind)
		{
			return GetCooldownSpeed() + (float)GetFinal(kind) - 1f;
		}

		public float GetSkillCooldownSpeed()
		{
			return GetCooldownSpeed(Kind.SkillCooldownSpeed);
		}

		public float GetDashCooldownSpeed()
		{
			return GetCooldownSpeed(Kind.DashCooldownSpeed);
		}

		public float GetSwapCooldownSpeed()
		{
			return GetCooldownSpeed(Kind.SwapCooldownSpeed);
		}

		public float GetQuintessenceCooldownSpeed()
		{
			return GetCooldownSpeed(Kind.EssenceCooldownSpeed);
		}

		private float InterpolatedAttackSpeed(double attackSpeed)
		{
			if (attackSpeed > 1.1000000238418579)
			{
				attackSpeed = 1.1000000238418579 + (attackSpeed - 1.1000000238418579) * 0.949999988079071;
			}
			if (attackSpeed > 1.2000000476837158)
			{
				attackSpeed = 1.2000000476837158 + (attackSpeed - 1.2000000476837158) * 0.89999997615814209;
			}
			if (attackSpeed > 1.2999999523162842)
			{
				attackSpeed = 1.2999999523162842 + (attackSpeed - 1.2999999523162842) * 0.85000002384185791;
			}
			if (attackSpeed > 1.3999999761581421)
			{
				attackSpeed = 1.3999999761581421 + (attackSpeed - 1.3999999761581421) * 0.800000011920929;
			}
			if (attackSpeed > 1.5)
			{
				attackSpeed = 1.5 + (attackSpeed - 1.5) * 0.75;
			}
			return (float)attackSpeed;
		}

		public float GetInterpolatedBasicAttackSpeed()
		{
			return InterpolatedAttackSpeed(GetFinal(Kind.AttackSpeed) * GetFinal(Kind.BasicAttackSpeed));
		}

		public float GetInterpolatedSkillAttackSpeed()
		{
			return InterpolatedAttackSpeed(GetFinal(Kind.AttackSpeed) * GetFinal(Kind.SkillAttackSpeed));
		}

		public float GetInterpolatedChargingSpeed()
		{
			return InterpolatedAttackSpeed(GetFinal(Kind.AttackSpeed) * GetFinal(Kind.ChargingSpeed));
		}

		public float GetInterpolatedBasicAttackChargingSpeed()
		{
			return InterpolatedAttackSpeed(GetFinal(Kind.AttackSpeed) * GetFinal(Kind.BasicAttackSpeed) * GetFinal(Kind.ChargingSpeed));
		}

		public float GetInterpolatedSkillAttackChargingSpeed()
		{
			return InterpolatedAttackSpeed(GetFinal(Kind.AttackSpeed) * GetFinal(Kind.SkillAttackSpeed) * GetFinal(Kind.ChargingSpeed));
		}

		public float GetInterpolatedMovementSpeed()
		{
			float num = (float)GetFinal(Kind.MovementSpeed);
			if (num > 6f)
			{
				num = 6f + (num - 6f) * 0.95f;
			}
			if (num > 6.5f)
			{
				num = 6.5f + (num - 6.5f) * 0.9f;
			}
			if (num > 7f)
			{
				num = 7f + (num - 7f) * 0.85f;
			}
			if (num > 7.5f)
			{
				num = 7.5f + (num - 7.5f) * 0.8f;
			}
			if (num > 8f)
			{
				num = 8f + (num - 8f) * 0.75f;
			}
			return num;
		}

		public double GetStatusResistacneFor(CharacterStatus.Kind kind)
		{
			switch (kind)
			{
			case CharacterStatus.Kind.Stun:
				return GetFinal(Kind.StatusResistance) * GetFinal(Kind.StunResistance);
			case CharacterStatus.Kind.Freeze:
				return GetFinal(Kind.StatusResistance) * GetFinal(Kind.FreezeResistance);
			case CharacterStatus.Kind.Burn:
				return GetFinal(Kind.StatusResistance) * GetFinal(Kind.BurnResistance);
			case CharacterStatus.Kind.Bleed:
				return GetFinal(Kind.StatusResistance) * GetFinal(Kind.BleedResistance);
			case CharacterStatus.Kind.Poison:
				return GetFinal(Kind.StatusResistance) * GetFinal(Kind.PoisonResistance);
			default:
				return GetFinal(Kind.StatusResistance);
			}
		}

		public bool ApplyDefense(ref Damage damage)
		{
			damage.multiplier *= GetFinal(Kind.TakingDamage);
			switch (damage.attackType)
			{
			case Damage.AttackType.Melee:
				damage.multiplier *= GetFinal(Kind.TakingPhysicalDamage);
				break;
			case Damage.AttackType.Ranged:
				damage.multiplier *= GetFinal(Kind.TakingMagicDamage);
				break;
			case Damage.AttackType.Projectile:
				damage.multiplier *= GetFinal(Kind.TakingMagicDamage);
				break;
			}
			return false;
		}

		public bool Evade(ref Damage damage)
		{
			double num = GetFinal(Kind.EvasionChance);
			switch (damage.attackType)
			{
			case Damage.AttackType.Melee:
				num *= GetFinal(Kind.MeleeEvasionChance);
				break;
			case Damage.AttackType.Ranged:
				num *= GetFinal(Kind.RangedEvasionChance);
				break;
			}
			return !MMMaths.Chance(num);
		}

		public bool Contains(Values values)
		{
			if (_bonuses.Contains(values))
			{
				return true;
			}
			for (int i = 0; i < _bonusesWithEvent.Count; i++)
			{
				if (_bonusesWithEvent[i].values == values)
				{
					return true;
				}
			}
			return false;
		}

		public void AttachValues(Values values)
		{
			_bonuses.Add(values);
			_needUpdate = true;
		}

		public void AttachValues(Values values, ValuesWithEvent.OnDetachDelegate onDetach)
		{
			_bonusesWithEvent.Add(new ValuesWithEvent(values, onDetach));
			_needUpdate = true;
		}

		public void DetachValues(Values values)
		{
			if (_bonuses.Remove(values))
			{
				_needUpdate = true;
				return;
			}
			for (int i = 0; i < _bonusesWithEvent.Count; i++)
			{
				if (_bonusesWithEvent[i].values == values)
				{
					_bonusesWithEvent[i]._onDetach?.Invoke(this);
					_bonusesWithEvent.RemoveAt(i);
					_needUpdate = true;
					break;
				}
			}
		}

		public void AttachTimedValues(Values values, float duration, ValuesWithEvent.OnDetachDelegate onDetach = null)
		{
			if (!(duration <= 0f))
			{
				_timedBonuses.Add(new TimedValues(values, duration));
				_needUpdate = true;
			}
		}

		public void DetachTimedValues(Values values)
		{
			for (int i = 0; i < _timedBonuses.Count; i++)
			{
				if (_timedBonuses[i].values == values)
				{
					_timedBonuses.RemoveAt(i);
					_needUpdate = true;
					break;
				}
			}
		}

		public bool UpdateTimedValues(Values values, float duration, ValuesWithEvent.OnDetachDelegate onDetach = null)
		{
			for (int i = 0; i < _timedBonuses.Count; i++)
			{
				if (_timedBonuses[i].values == values)
				{
					_timedBonuses[i]._onDetach?.Invoke(this);
					_timedBonuses[i]._onDetach = onDetach;
					_timedBonuses[i].SetTime(duration);
					return true;
				}
			}
			return false;
		}

		public void AttachOrUpdateTimedValues(Values values, float duration, ValuesWithEvent.OnDetachDelegate onDetach = null)
		{
			if (!UpdateTimedValues(values, duration, onDetach))
			{
				_timedBonuses.Add(new TimedValues(values, duration, onDetach));
				_needUpdate = true;
			}
		}

		public void TakeTime(float deltaTime)
		{
			deltaTime /= (float)GetFinal(Kind.BuffDuration);
			for (int num = _timedBonuses.Count - 1; num >= 0; num--)
			{
				if (_timedBonuses[num].TakeTime(deltaTime))
				{
					_timedBonuses[num]._onDetach?.Invoke(this);
					_timedBonuses.RemoveAt(num);
					_needUpdate = true;
				}
			}
		}

		public void SetNeedUpdate()
		{
			_needUpdate = true;
		}

		public bool UpdateIfNecessary()
		{
			if (_needUpdate)
			{
				Update();
				return true;
			}
			return false;
		}

		public void Update()
		{
			_needUpdate = false;
			Initialize();
			for (int i = 0; i < _bonuses.Count; i++)
			{
				Apply(_bonuses[i]);
			}
			for (int j = 0; j < _bonusesWithEvent.Count; j++)
			{
				Apply(_bonusesWithEvent[j].values);
			}
			for (int k = 0; k < _timedBonuses.Count; k++)
			{
				Apply(_timedBonuses[k].values);
			}
			for (int l = 0; l < Kind.count; l++)
			{
				switch (Kind.values[l].valueForm)
				{
				case Kind.ValueForm.Constant:
					_values[Category.Final.index, l] = _values[Category.Constant.index, l] * (_values[Category.Percent.index, l] * (1.0 + _values[Category.PercentPoint.index, l])) + _values[Category.Fixed.index, l];
					break;
				case Kind.ValueForm.Percent:
					_values[Category.Final.index, l] = _values[Category.Percent.index, l] * (1.0 + _values[Category.PercentPoint.index, l]);
					break;
				case Kind.ValueForm.Product:
					_values[Category.Final.index, l] = _values[Category.Percent.index, l];
					break;
				}
			}
		}

		public void Clear()
		{
			_bonuses.Clear();
			for (int i = 0; i < _bonusesWithEvent.Count; i++)
			{
				_bonusesWithEvent[i]._onDetach?.Invoke(this);
			}
			_bonusesWithEvent.Clear();
			for (int j = 0; j < _timedBonuses.Count; j++)
			{
				_timedBonuses[j]._onDetach?.Invoke(this);
			}
			_timedBonuses.Clear();
			_needUpdate = true;
		}

		public void ClearNontimedValues()
		{
			_bonuses.Clear();
			for (int i = 0; i < _bonusesWithEvent.Count; i++)
			{
				_bonusesWithEvent[i]._onDetach?.Invoke(this);
			}
			_bonusesWithEvent.Clear();
			_needUpdate = true;
		}

		public void ClearTimedValues()
		{
			for (int i = 0; i < _timedBonuses.Count; i++)
			{
				_timedBonuses[i]._onDetach?.Invoke(this);
			}
			_timedBonuses.Clear();
			_needUpdate = true;
		}
	}
}
