using System;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnKilled : Trigger
	{
		[SerializeField]
		private Transform _moveToHitPosition;

		[SerializeField]
		private bool _onCritical;

		[SerializeField]
		[Tooltip("비어있지 않을 경우, 해당 키를 가진 공격에만 발동됨")]
		private string _attackKey;

		[SerializeField]
		private MotionTypeBoolArray _attackTypes;

		[SerializeField]
		private AttackTypeBoolArray _damageTypes;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypes = new CharacterTypeBoolArray(true, true, true, true, true, false, false, false);

		[SerializeField]
		private int _killCount = 1;

		private int _remainKillCount;

		private Character _character;

		public OnKilled()
		{
			_remainKillCount = _killCount;
		}

		public override void Attach(Character character)
		{
			_character = character;
			Character character2 = _character;
			character2.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character2.onKilled, new Character.OnKilledDelegate(OnCharacterKilled));
		}

		public override void Detach()
		{
			Character character = _character;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnCharacterKilled));
		}

		private void OnCharacterKilled(ITarget target, ref Damage damage)
		{
			if (_characterTypes[target.character.type] && (!_characterTypes[Character.Type.Boss] || (target.character.key != Key.FirstHero1 && target.character.key != Key.FirstHero2 && target.character.key != 0)) && _attackTypes[damage.motionType] && _damageTypes[damage.attackType] && (string.IsNullOrWhiteSpace(_attackKey) || damage.key.Equals(_attackKey, StringComparison.OrdinalIgnoreCase)))
			{
				if (_moveToHitPosition != null)
				{
					_moveToHitPosition.position = damage.hitPoint;
				}
				_remainKillCount--;
				if (_remainKillCount <= 0)
				{
					_remainKillCount = _killCount;
					Invoke();
				}
			}
		}
	}
}
