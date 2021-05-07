using System.Collections.Generic;
using UnityEngine;

namespace Characters.Monsters
{
	public class MonsterContainer : MonoBehaviour
	{
		private List<Monster> _monsters;

		private void Awake()
		{
			_monsters = new List<Monster>();
		}

		public void Add(Monster minion)
		{
			_monsters.Add(minion);
		}

		public bool Remove(Monster minion)
		{
			return _monsters.Remove(minion);
		}

		public int Count()
		{
			return _monsters.Count;
		}
	}
}
