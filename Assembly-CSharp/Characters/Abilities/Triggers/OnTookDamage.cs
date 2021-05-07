using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnTookDamage : Trigger
	{
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
			if (!(damage.amount < _minDamage) && (!_onCritical || tookDamage.critical) && _attackTypes[tookDamage.motionType] && _damageTypes[tookDamage.attackType])
			{
				Invoke();
			}
		}
	}
}
