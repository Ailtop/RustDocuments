using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class WeightedSelector : Decorator
	{
		private WeightedRandomizer<Behaviour> _weightedRandomizer;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Weight))]
		private Weight.Subcomponents _weights;

		private void Awake()
		{
			List<ValueTuple<Behaviour, float>> list = new List<ValueTuple<Behaviour, float>>();
			Weight[] components = _weights.components;
			foreach (Weight weight in components)
			{
				list.Add(new ValueTuple<Behaviour, float>(weight.key, weight.value));
			}
			_weightedRandomizer = new WeightedRandomizer<Behaviour>(list);
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _weightedRandomizer.TakeOne().CRun(controller);
			base.result = Result.Success;
		}
	}
}
