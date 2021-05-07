using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Archer")]
	public class ArcherPattern : ScriptableObject
	{
		[Serializable]
		public class Attack : Pattern
		{
		}

		[Serializable]
		public class ArrowRain : Pattern
		{
		}

		[Serializable]
		public class MeleeAttack : Pattern
		{
		}

		[Serializable]
		public class MeleeAttackShortCool : Pattern
		{
		}

		[Serializable]
		public class BirdHunt : Pattern
		{
		}

		[Serializable]
		public class Backstep : Pattern
		{
		}

		[Serializable]
		public class BackstepShortCool : Pattern
		{
		}

		[Serializable]
		public class SecondBackstep : Pattern
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
		private Attack _attack;

		[SerializeField]
		private MeleeAttack _meleeAttack;

		[SerializeField]
		private MeleeAttackShortCool _meleeAttackShortCool;

		[SerializeField]
		private Backstep _backStep;

		[SerializeField]
		private BackstepShortCool _backstepShortCool;

		[SerializeField]
		private SecondBackstep _secondBackstep;

		[SerializeField]
		private ArrowRain _arrowRain;

		[SerializeField]
		private BirdHunt _birdHunt;

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
					_patterns = new Pattern[12]
					{
						_attack, _meleeAttack, _meleeAttackShortCool, _backStep, _backstepShortCool, _secondBackstep, _arrowRain, _birdHunt, _skipableIdle, _idle,
						_drinkPotion, _runaway
					};
				}
				return _patterns;
			}
		}
	}
}
