using System;
using System.Linq;
using Data;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class CurrencyPossibilities
	{
		public static readonly GameData.Currency.Type[] values = EnumValues<GameData.Currency.Type>.Values;

		[SerializeField]
		[Range(0f, 100f)]
		private int[] _possibilities;

		public int this[int index] => _possibilities[index];

		public int this[GameData.Currency.Type size] => _possibilities[(int)size];

		public GameData.Currency.Type? Evaluate()
		{
			return Evaluate(_possibilities);
		}

		public CurrencyPossibilities(params int[] possibilities)
		{
			_possibilities = possibilities;
		}

		public static GameData.Currency.Type? Evaluate(int[] possibilities)
		{
			int max = Mathf.Max(possibilities.Sum(), 100);
			int num = UnityEngine.Random.Range(0, max) + 1;
			for (int i = 0; i < possibilities.Length; i++)
			{
				num -= possibilities[i];
				if (num <= 0)
				{
					return values[i];
				}
			}
			return null;
		}
	}
}
