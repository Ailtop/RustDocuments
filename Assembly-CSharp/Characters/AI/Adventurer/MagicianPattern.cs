using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Magician")]
	public class MagicianPattern : ScriptableObject
	{
		[Serializable]
		public class KeepDistance : Pattern
		{
		}

		[Serializable]
		public class KeepDistanceLongDistance : Pattern
		{
		}

		[Serializable]
		public class FireballCombo : Pattern
		{
		}

		[Serializable]
		public class PhoenixLanding : Pattern
		{
		}

		[Serializable]
		public class PhoenixLandingLongCool : Pattern
		{
		}

		[Serializable]
		public class WorldOnFire : Pattern
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
		private KeepDistance _keepDistance;

		[SerializeField]
		private KeepDistanceLongDistance _keepDistanceLongDistance;

		[SerializeField]
		private FireballCombo _fireballCombo;

		[SerializeField]
		private PhoenixLanding _phoneixLanding;

		[SerializeField]
		private PhoenixLandingLongCool _phoenixLandingLongCool;

		[SerializeField]
		private WorldOnFire _worldOnFire;

		[SerializeField]
		private SkipableIdle _skipableIdle;

		[SerializeField]
		private Idle _idle;

		[SerializeField]
		private DrinkPotion _drinkPotion;

		[SerializeField]
		private Runaway _runaway;

		private Pattern[] _patterns;

		private bool _initialized;

		public Pattern[] patterns
		{
			get
			{
				if (_patterns == null)
				{
					_patterns = new Pattern[10] { _keepDistance, _keepDistanceLongDistance, _fireballCombo, _worldOnFire, _phoneixLanding, _phoenixLandingLongCool, _skipableIdle, _idle, _drinkPotion, _runaway };
				}
				return _patterns;
			}
		}
	}
}
