using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runnables.Triggers
{
	internal class TriggerArray : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<TriggerArray>
		{
			internal IEnumerable<bool> CCheckNext()
			{
				for (int operationIndex = 0; operationIndex < base.components.Length; operationIndex++)
				{
					yield return base.components[operationIndex].trigger.isSatisfied();
				}
			}
		}

		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		public Trigger trigger => _trigger;

		public override string ToString()
		{
			return trigger.GetAutoName() ?? "";
		}
	}
}
