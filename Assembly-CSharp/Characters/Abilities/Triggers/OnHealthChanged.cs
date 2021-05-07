using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnHealthChanged : Trigger
	{
		private enum CompareType
		{
			GreaterThanOrEqual,
			LessThan,
			Equal
		}

		private enum HealthType
		{
			Constant,
			Percent
		}

		[Header("Health")]
		[SerializeField]
		private CompareType _compareType;

		[SerializeField]
		private HealthType _healthType;

		[SerializeField]
		private int _amount;

		[Header("Damage")]
		[SerializeField]
		private double _minDamage = 1.0;

		[SerializeField]
		private bool _onCritical;

		[SerializeField]
		private MotionTypeBoolArray _attackTypes;

		[SerializeField]
		private AttackTypeBoolArray _damageTypes;

		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			_character.health.onTookDamage += OnCharacterTookDamage;
		}

		public override void Detach()
		{
			_character.health.onTookDamage -= OnCharacterTookDamage;
		}

		private void OnCharacterTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			Damage damage = tookDamage;
			if (!(damage.amount < _minDamage) && (!_onCritical || tookDamage.critical) && _attackTypes[tookDamage.motionType] && _damageTypes[tookDamage.attackType] && CheckHealthCondition())
			{
				Invoke();
			}
		}

		private bool CheckHealthCondition()
		{
			switch (_compareType)
			{
			case CompareType.GreaterThanOrEqual:
				if (_healthType == HealthType.Constant && _character.health.currentHealth >= (double)_amount)
				{
					return true;
				}
				if (_healthType == HealthType.Percent && _character.health.percent >= (double)_amount * 0.01)
				{
					return true;
				}
				break;
			case CompareType.LessThan:
				if (_healthType == HealthType.Constant && _character.health.currentHealth < (double)_amount)
				{
					return true;
				}
				if (_healthType == HealthType.Percent && _character.health.percent < (double)_amount * 0.01)
				{
					return true;
				}
				break;
			case CompareType.Equal:
				if (_healthType == HealthType.Constant && _character.health.currentHealth >= (double)_amount && _character.health.currentHealth < (double)(_amount + 1))
				{
					return true;
				}
				if (_healthType == HealthType.Percent && _character.health.percent > (double)(_amount - 1) * 0.01 && _character.health.percent < (double)(_amount + 1) * 0.01)
				{
					return true;
				}
				break;
			}
			return false;
		}
	}
}
