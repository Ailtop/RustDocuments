using System;
using System.Linq;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class PotionPossibilities
	{
		public static readonly Potion.Size[] values = EnumValues<Potion.Size>.Values;

		[SerializeField]
		[Range(0f, 100f)]
		private int[] _possibilities;

		public int this[int index] => _possibilities[index];

		public int this[Potion.Size size] => _possibilities[(int)size];

		public Potion Get()
		{
			Potion.Size? size = Evaluate(_possibilities);
			if (size.HasValue)
			{
				return Resource.instance.potions[size.Value];
			}
			return null;
		}

		public Potion.Size? Evaluate()
		{
			return Evaluate(_possibilities);
		}

		public PotionPossibilities(params int[] possibilities)
		{
			_possibilities = possibilities;
		}

		public static Potion.Size? Evaluate(int[] possibilities)
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
