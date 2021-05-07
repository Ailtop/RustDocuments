using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public class OneRandomFanaticPolicy : FanaticPolicy
	{
		[SerializeField]
		private FanaticFactory.SummonType[] _summonTypes = new FanaticFactory.SummonType[3]
		{
			FanaticFactory.SummonType.Fanatic,
			FanaticFactory.SummonType.AgedFanatic,
			FanaticFactory.SummonType.MartyrFanatic
		};

		protected override void GetToSummons(ref List<FanaticFactory.SummonType> results, int count)
		{
			results.Clear();
			FanaticFactory.SummonType item = _summonTypes.Random();
			for (int i = 0; i < count; i++)
			{
				results.Add(item);
			}
		}
	}
}
