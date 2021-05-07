using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Warrior")]
	public class WarriorPattern : ScriptableObject
	{
		[Serializable]
		public class Guard : Pattern
		{
		}

		[Serializable]
		public class PowerWave : Pattern
		{
		}

		[Serializable]
		public class Whirlwind : Pattern
		{
		}

		[Serializable]
		public class Stamping : Pattern
		{
		}

		[Serializable]
		public class Earthquake : Pattern
		{
		}

		[Serializable]
		public class Rescue : Pattern
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
		private Guard _guard;

		[SerializeField]
		private PowerWave _powerWave;

		[SerializeField]
		private Stamping _stamping;

		[SerializeField]
		private Earthquake _earthquake;

		[SerializeField]
		private Whirlwind _whirlwind;

		[SerializeField]
		private Rescue _rescue;

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
					_patterns = new Pattern[10] { _guard, _powerWave, _stamping, _earthquake, _whirlwind, _rescue, _skipableIdle, _idle, _drinkPotion, _runaway };
				}
				return _patterns;
			}
		}
	}
}
