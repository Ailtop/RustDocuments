using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class UniformSelector : Decorator
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Weight))]
		private Weight.Subcomponents _weights;

		private List<Behaviour> _container = new List<Behaviour>();

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_container.Count <= 0)
			{
				Fill();
			}
			Behaviour behaviour = _container.Random();
			_container.Remove(behaviour);
			yield return behaviour.CRun(controller);
			base.result = Result.Success;
		}

		private void Fill()
		{
			Weight[] components = _weights.components;
			foreach (Weight weight in components)
			{
				for (int j = 0; j < weight.value; j++)
				{
					_container.Add(weight.key);
				}
			}
		}
	}
}
