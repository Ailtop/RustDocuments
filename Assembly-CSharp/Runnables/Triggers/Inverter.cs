using UnityEngine;

namespace Runnables.Triggers
{
	public class Inverter : Trigger
	{
		[SerializeField]
		[Subcomponent]
		private Trigger _trigger;

		protected override bool Check()
		{
			return !_trigger.isSatisfied();
		}
	}
}
