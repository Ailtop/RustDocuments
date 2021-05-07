using Characters.Actions;
using UnityEngine;

namespace Characters.Operations
{
	public class TriggerActionStart : Operation
	{
		[SerializeField]
		private Action _action;

		public override void Run()
		{
			_action.TriggerStartManually();
		}
	}
}
