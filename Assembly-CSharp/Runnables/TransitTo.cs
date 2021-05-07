using Runnables.States;
using UnityEngine;

namespace Runnables
{
	public class TransitTo : Runnable
	{
		[SerializeField]
		private StateMachine _stateMachine;

		[SerializeField]
		private State _targetState;

		public override void Run()
		{
			_stateMachine.TransitTo(_targetState);
		}
	}
}
