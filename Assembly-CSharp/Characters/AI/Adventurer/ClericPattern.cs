using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[CreateAssetMenu(menuName = "Patterns/Cleric")]
	public class ClericPattern : ScriptableObject
	{
		[Serializable]
		public class EscapeTeleport : Pattern
		{
		}

		[Serializable]
		public class EscapeTeleportLongCool : Pattern
		{
		}

		[Serializable]
		public class Heal : Pattern
		{
		}

		[Serializable]
		public class HealShortCool : Pattern
		{
		}

		[Serializable]
		public class HolyCross : Pattern
		{
		}

		[Serializable]
		public class MassiveHeal : Pattern
		{
		}

		[Serializable]
		public class Reinforce : Pattern
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

		[Serializable]
		public class ChaseTeleport : Pattern
		{
		}

		[SerializeField]
		private EscapeTeleport _teleport;

		[SerializeField]
		private EscapeTeleportLongCool _teleportLongCool;

		[SerializeField]
		private Heal _heal;

		[SerializeField]
		private HealShortCool _healShortCool;

		[SerializeField]
		private HolyCross _holyCross;

		[SerializeField]
		private Reinforce _reinforce;

		[SerializeField]
		private MassiveHeal _massiveHeal;

		[SerializeField]
		private SkipableIdle _skipableIdle;

		[SerializeField]
		private Idle _idle;

		[SerializeField]
		private DrinkPotion _drinkPotion;

		[SerializeField]
		private Runaway _runaway;

		[SerializeField]
		private ChaseTeleport _chaseTeleort;

		private Pattern[] _patterns;

		public Pattern[] patterns
		{
			get
			{
				if (_patterns == null)
				{
					_patterns = new Pattern[12]
					{
						_teleport, _teleportLongCool, _heal, _healShortCool, _holyCross, _reinforce, _massiveHeal, _skipableIdle, _idle, _drinkPotion,
						_runaway, _chaseTeleort
					};
				}
				return _patterns;
			}
		}
	}
}
