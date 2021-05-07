using Characters.Actions;
using UnityEngine;

namespace Characters.Operations
{
	public class RunAction : CharacterOperation
	{
		[SerializeField]
		private Action _action;

		public override void Run(Character owner)
		{
			_action.TryStart();
		}
	}
}
