using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public class WeightedFanaticPolicy : FanaticPolicy
	{
		private List<FanaticFactory.SummonType> _summonTypes;

		[SerializeField]
		private int _fanaticWeight = 1;

		[SerializeField]
		private int _agedFanaticWeight = 1;

		[SerializeField]
		private int _martyrFanaticWeight = 1;

		private void Awake()
		{
			_summonTypes = new List<FanaticFactory.SummonType>(_fanaticWeight + _agedFanaticWeight + _martyrFanaticWeight);
			for (int i = 0; i < _fanaticWeight; i++)
			{
				_summonTypes.Add(FanaticFactory.SummonType.Fanatic);
			}
			for (int j = 0; j < _agedFanaticWeight; j++)
			{
				_summonTypes.Add(FanaticFactory.SummonType.AgedFanatic);
			}
			for (int k = 0; k < _martyrFanaticWeight; k++)
			{
				_summonTypes.Add(FanaticFactory.SummonType.MartyrFanatic);
			}
		}

		protected override void GetToSummons(ref List<FanaticFactory.SummonType> results, int count)
		{
			results.Clear();
			for (int i = 0; i < count; i++)
			{
				results.Add(_summonTypes.Random());
			}
		}
	}
}
