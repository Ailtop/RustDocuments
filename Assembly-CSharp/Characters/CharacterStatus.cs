using System;
using Characters.Abilities.Statuses;
using UnityEngine;

namespace Characters
{
	public class CharacterStatus : MonoBehaviour
	{
		private class Grades
		{
			internal delegate void OnChanged(int old, int @new);

			private readonly int[] _grades;

			internal int max { get; private set; }

			internal event OnChanged onChanged;

			internal Grades(int maxGrade)
			{
				_grades = new int[maxGrade];
				max = -1;
			}

			internal void Attach(int grade)
			{
				_grades[grade]++;
				if (grade > max)
				{
					this.onChanged?.Invoke(max + 1, grade + 1);
					max = grade;
				}
			}

			internal void Detach(int grade)
			{
				_grades[grade]--;
				if (grade != max || _grades[grade] != 0)
				{
					return;
				}
				for (int num = grade - 1; num >= 0; num--)
				{
					if (_grades[num] > 0)
					{
						this.onChanged?.Invoke(max + 1, num + 1);
						max = num;
						return;
					}
				}
				this.onChanged?.Invoke(max + 1, 0);
				max = -1;
			}
		}

		private abstract class Status
		{
			protected readonly CharacterStatus _characterStatus;

			protected readonly Character _character;

			protected internal int currentGrade { get; protected set; }

			internal Status(CharacterStatus characterStatus)
			{
				_characterStatus = characterStatus;
				_character = characterStatus._character;
			}

			internal abstract void Apply(int grade);

			internal abstract void Stop();
		}

		[Serializable]
		public class ApplyInfo
		{
			public Kind kind;

			public float duration;

			public double damage;

			public ApplyInfo(Kind kind, float duration, double damage = 0.0)
			{
				this.kind = kind;
				this.duration = duration;
				this.damage = damage;
			}
		}

		public enum Kind
		{
			Stun,
			Freeze,
			Burn,
			Bleed,
			Poison
		}

		public delegate void OnApplyDelegate(ApplyInfo applyInfo);

		public readonly Sum<int> gradeBonuses = new Sum<int>(0);

		private const int _maxGrade = 4;

		[SerializeField]
		[GetComponent]
		private Character _character;

		private readonly EnumArray<Kind, Status> _statuses = new EnumArray<Kind, Status>();

		private Renderer _mainRenderer;

		private Stun _stun;

		private Burn _burn;

		private Bleed _bleed;

		public int this[Kind kind] => _statuses[kind].currentGrade;

		public bool stuned => !_stun.expired;

		public bool freezed => !freeze.expired;

		public bool burning => !_burn.expired;

		public bool bleeding => !_bleed.expired;

		public bool poisoned => !poison.expired;

		public Freeze freeze { get; private set; }

		public Poison poison { get; private set; }

		public event OnApplyDelegate onApply;

		private void Awake()
		{
			_stun = new Stun(_character);
			_stun.Initialize();
			freeze = new Freeze(_character);
			freeze.Initialize();
			_burn = new Burn(_character);
			_burn.Initialize();
			_bleed = new Bleed(_character);
			_bleed.Initialize();
			poison = new Poison(_character);
			poison.Initialize();
			_mainRenderer = _character.spriteEffectStack.mainRenderer;
		}

		public bool Apply(Character attacker, ApplyInfo applyInfo)
		{
			bool flag = false;
			if ((double)applyInfo.duration * _character.stat.GetStatusResistacneFor(applyInfo.kind) > 0.0)
			{
				switch (applyInfo.kind)
				{
				case Kind.Stun:
					flag = ApplyStun(attacker, applyInfo.duration);
					break;
				case Kind.Freeze:
					flag = ApplyFreeze(attacker, applyInfo.duration, applyInfo.damage);
					break;
				case Kind.Burn:
					flag = ApplyBurn(attacker, applyInfo.duration, applyInfo.damage);
					break;
				case Kind.Bleed:
					flag = ApplyBleed(attacker, applyInfo.duration, applyInfo.damage);
					break;
				case Kind.Poison:
					flag = ApplyPoison(attacker, applyInfo.duration, applyInfo.damage);
					break;
				}
			}
			if (flag)
			{
				this.onApply?.Invoke(applyInfo);
			}
			return flag;
		}

		public void RemoveAllStatus()
		{
			_character.ability.Remove(_stun);
			_character.ability.Remove(freeze);
			_character.ability.Remove(_burn);
			_character.ability.Remove(_bleed);
			_character.ability.Remove(poison);
		}

		private bool ApplyStun(Character attacker, float duration)
		{
			_stun.attacker = attacker;
			_stun.duration = duration;
			_character.ability.Add(_stun);
			return true;
		}

		private bool ApplyFreeze(Character attacker, float duration, double damage)
		{
			if (_character.ability.Contains(freeze))
			{
				return false;
			}
			freeze.attacker = attacker;
			freeze.duration = duration;
			freeze.damageOnEnd = damage;
			_character.ability.Add(freeze);
			return true;
		}

		private bool ApplyBurn(Character attacker, float duration, double damagePerSecond)
		{
			_burn.Add(attacker, duration, damagePerSecond);
			_character.ability.Add(_burn);
			return true;
		}

		private bool ApplyBleed(Character attacker, float duration, double damagePerSecond)
		{
			_bleed.Add(attacker, duration, damagePerSecond);
			_character.ability.Add(_bleed);
			return true;
		}

		private bool ApplyPoison(Character attacker, float duration, double damagePerSecond)
		{
			poison.Add(attacker, duration, damagePerSecond);
			_character.ability.Add(poison);
			return true;
		}

		public bool IsApplying(Kind kind)
		{
			switch (kind)
			{
			case Kind.Stun:
				return stuned;
			case Kind.Freeze:
				return freezed;
			case Kind.Burn:
				return burning;
			case Kind.Bleed:
				return bleeding;
			case Kind.Poison:
				return poisoned;
			default:
				return false;
			}
		}

		public bool IsApplying(EnumArray<Kind, bool> enumArray)
		{
			for (int i = 0; i < enumArray.Count; i++)
			{
				if (enumArray.Array[i] && IsApplying(enumArray.Keys[i]))
				{
					return true;
				}
			}
			return false;
		}
	}
}
