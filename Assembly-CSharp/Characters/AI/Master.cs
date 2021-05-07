using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI
{
	public class Master : MonoBehaviour
	{
		public Character character;

		private List<Slave> _slaves = new List<Slave>();

		public void AddSlave(Slave slave)
		{
			_slaves.Add(slave);
		}

		public void RemoveSlave(Slave slave)
		{
			_slaves.Remove(slave);
		}

		public bool isCleared()
		{
			return _slaves.Count == 0;
		}
	}
}
