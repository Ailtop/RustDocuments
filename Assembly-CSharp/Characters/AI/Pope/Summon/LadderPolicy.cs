using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public abstract class LadderPolicy : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[1] { typeof(RandomLadderPolicy) };

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		private List<FanaticLadder> _results;

		public List<FanaticLadder> GetLadders(List<FanaticFactory.SummonType> summonTypes)
		{
			if (_results == null)
			{
				_results = new List<FanaticLadder>(summonTypes.Count);
			}
			GetLadders(ref _results, summonTypes.Count);
			for (int i = 0; i < summonTypes.Count; i++)
			{
				_results[i].SetFanatic(summonTypes[i]);
			}
			return _results;
		}

		protected abstract void GetLadders(ref List<FanaticLadder> results, int count);
	}
}
