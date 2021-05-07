using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Thief")]
	public class ThiefPattern : ScriptableObject
	{
		[Serializable]
		public class ShadowStep : Pattern
		{
		}

		[Serializable]
		public class FlashCut : Pattern
		{
		}

		[Serializable]
		public class Shuriken : Pattern
		{
		}

		[Serializable]
		public class GiganticShuriken : Pattern
		{
		}

		[Serializable]
		public class GiganticShurikenLongCool : Pattern
		{
		}

		[Serializable]
		public class ShadowBunshin : Pattern
		{
		}

		[Serializable]
		public class SwithingTeleport : Pattern
		{
		}

		[Serializable]
		public class MultipleBunshin : Pattern
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
		private ShadowStep _shadowStep;

		[SerializeField]
		private FlashCut _flashCut;

		[SerializeField]
		private Shuriken _suriken;

		[SerializeField]
		private GiganticShuriken _giganticShuriken;

		[SerializeField]
		private GiganticShurikenLongCool _giganticShurikenLongCool;

		[SerializeField]
		private ShadowBunshin _shadowBunshin;

		[SerializeField]
		private SwithingTeleport _switchingTeleport;

		[SerializeField]
		private MultipleBunshin _multipleBunshin;

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
						_shadowStep, _flashCut, _suriken, _shadowBunshin, _giganticShuriken, _giganticShurikenLongCool, _switchingTeleport, _multipleBunshin, _skipableIdle, _idle,
						_drinkPotion, _runaway
					};
				}
				return _patterns;
			}
		}
	}
}
