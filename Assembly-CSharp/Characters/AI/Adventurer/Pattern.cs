using System;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	[Serializable]
	public abstract class Pattern
	{
		[SerializeField]
		[Range(0f, 10f)]
		private int _weight;

		[SerializeField]
		private Pattern _pattern;

		public int weight => _weight;

		public Pattern nextPattern => _pattern;
	}
}
