using System;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class ValueByRarity
	{
		public static readonly string[] names = EnumValues<Rarity>.Names;

		public static readonly Rarity[] values = EnumValues<Rarity>.Values;

		[SerializeField]
		private float[] _values;

		public float this[int index] => _values[index];

		public float this[Rarity rarity] => _values[rarity];

		public ValueByRarity(params float[] values)
		{
			_values = values;
		}
	}
}
