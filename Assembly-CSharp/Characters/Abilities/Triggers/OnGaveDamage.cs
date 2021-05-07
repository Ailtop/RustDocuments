using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnGaveDamage : Trigger
	{
		[SerializeField]
		private double _minDamage = 1.0;

		[SerializeField]
		[Range(0f, 1f)]
		private double _minDamagePercent;

		[SerializeField]
		private Transform _moveToHitPosition;

		[Header("Filter")]
		[SerializeField]
		private bool _needCritical;

		[SerializeField]
		private bool _backOnly;

		[SerializeField]
		[Tooltip("비어있지 않을 경우, 해당 키를 가진 공격에만 발동됨")]
		private string _attackKey;

		[SerializeField]
		private MotionTypeBoolArray _attackTypes;

		[SerializeField]
		private AttackTypeBoolArray _damageTypes;

		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			Character character2 = _character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character2.onGaveDamage, new GaveDamageDelegate(OnCharacterGaveDamage));
		}

		public override void Detach()
		{
			Character character = _character;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnCharacterGaveDamage));
		}

		private void OnCharacterGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (target.character == null)
			{
				return;
			}
			Damage damage = tookDamage;
			if (damage.amount < _minDamage)
			{
				return;
			}
			damage = tookDamage;
			if (damage.amount < target.character.health.maximumHealth * _minDamagePercent || (_needCritical && !tookDamage.critical) || !_attackTypes[tookDamage.motionType] || !_damageTypes[tookDamage.attackType] || (!string.IsNullOrWhiteSpace(_attackKey) && !tookDamage.key.Equals(_attackKey, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			if (_backOnly)
			{
				if (_character.movement.config.type == Movement.Config.Type.Static)
				{
					return;
				}
				Vector3 position = target.transform.position;
				Vector3 position2 = _character.transform.position;
				if ((target.character.lookingDirection == Character.LookingDirection.Right && position.x < position2.x) || (target.character.lookingDirection == Character.LookingDirection.Left && position.x > position2.x))
				{
					return;
				}
			}
			if (_moveToHitPosition != null)
			{
				_moveToHitPosition.position = tookDamage.hitPoint;
			}
			Invoke();
		}
	}
}
