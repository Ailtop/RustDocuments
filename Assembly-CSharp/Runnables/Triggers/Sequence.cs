using UnityEditor;
using UnityEngine;

namespace Runnables.Triggers
{
	public class Sequence : Trigger
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TriggerArray))]
		private TriggerArray.Subcomponents _triggers;

		protected override bool Check()
		{
			if (_triggers == null)
			{
				return true;
			}
			foreach (bool item in _triggers.CCheckNext())
			{
				if (!item)
				{
					return false;
				}
			}
			return true;
		}
	}
}
