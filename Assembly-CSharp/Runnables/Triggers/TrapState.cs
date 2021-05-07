using Level.Traps;
using UnityEngine;

namespace Runnables.Triggers
{
	public class TrapState : Trigger
	{
		[SerializeField]
		private ControlableTrap _trap;

		[SerializeField]
		private bool _activated;

		protected override bool Check()
		{
			return _trap.activated == _activated;
		}
	}
}
