using System;
using System.Linq;
using Characters.Gear;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class GearPossibilities
	{
		public static readonly Gear.Type[] values = EnumValues<Gear.Type>.Values;

		[SerializeField]
		[Range(0f, 100f)]
		private int[] _possibilities;

		public int this[int index] => _possibilities[index];

		public int this[Gear.Type size] => _possibilities[(int)size];

		public Gear.Type? Evaluate()
		{
			return Evaluate(_possibilities);
		}

		public GearPossibilities(params int[] possibilities)
		{
			_possibilities = possibilities;
		}

		public static Gear.Type? Evaluate(int[] possibilities)
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
