using UnityEngine;

namespace Characters
{
	public class PlayerStat : MonoBehaviour, ICharacterStat
	{
		private readonly Stat _commonStat;

		public readonly Stat weaponStat;

		public readonly Stat quintessenceStat;

		public Stat stat { get; private set; }

		public void Awake()
		{
		}
	}
}
