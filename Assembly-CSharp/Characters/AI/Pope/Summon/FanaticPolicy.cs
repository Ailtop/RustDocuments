using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public abstract class FanaticPolicy : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[3]
			{
				typeof(RandomFanaticPolicy),
				typeof(WeightedFanaticPolicy),
				typeof(OneRandomFanaticPolicy)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		private List<FanaticFactory.SummonType> _toSummons;

		public List<FanaticFactory.SummonType> GetToSummons(int count)
		{
			if (_toSummons == null)
			{
				_toSummons = new List<FanaticFactory.SummonType>(count);
			}
			GetToSummons(ref _toSummons, count);
			return _toSummons;
		}

		protected abstract void GetToSummons(ref List<FanaticFactory.SummonType> results, int count);
	}
}
