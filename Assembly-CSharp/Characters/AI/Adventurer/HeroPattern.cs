using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Hero")]
	public class HeroPattern : ScriptableObject
	{
		[Serializable]
		public class Dash : Pattern
		{
		}

		[Serializable]
		public class BackDash : Pattern
		{
		}

		[Serializable]
		public class EnergyBall : Pattern
		{
		}

		[Serializable]
		public class ComboAttack : Pattern
		{
		}

		[Serializable]
		public class SwordAuraWave : Pattern
		{
		}

		[Serializable]
		public class EnergyBlast : Pattern
		{
		}

		[Serializable]
		public class Stinger : Pattern
		{
		}

		[Serializable]
		public class SkipableIdle : Pattern
		{
		}

		[Serializable]
		public class Idle : Pattern
		{
		}

		[Serializable]
		public class DrinkPotion : Pattern
		{
		}

		[Serializable]
		public class Runaway : Pattern
		{
		}

		[SerializeField]
		private Dash _dash;

		[SerializeField]
		private BackDash _backDash;

		[SerializeField]
		private EnergyBall _energyBall;

		[SerializeField]
		private EnergyBlast _energyBlast;

		[SerializeField]
		private Stinger _stinger;

		[SerializeField]
		private ComboAttack _comboAttack;

		[SerializeField]
		private SwordAuraWave _swordAuraWave;

		[SerializeField]
		private SkipableIdle _skipableIdle;

		[SerializeField]
		private Idle _idle;

		[SerializeField]
		private DrinkPotion _drinkPotion;

		[SerializeField]
		private Runaway _runaway;

		private Pattern[] _patterns;

		public Pattern[] patterns
		{
			get
			{
				if (_patterns == null)
				{
					_patterns = new Pattern[11]
					{
						_dash, _backDash, _energyBlast, _energyBall, _stinger, _comboAttack, _swordAuraWave, _skipableIdle, _idle, _drinkPotion,
						_runaway
					};
				}
				return _patterns;
			}
		}
	}
}
