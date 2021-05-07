using UnityEngine;

namespace Characters
{
	public class CharacterStat : MonoBehaviour, ICharacterStat
	{
		public Stat stat { get; private set; }
	}
}
