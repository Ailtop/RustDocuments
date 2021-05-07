using Level.Traps;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class ControlTrap : Behavior
	{
		private enum Type
		{
			Activate,
			Deactivate
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private ControlableTrap _trap;

		public override void Run()
		{
			if (_type == Type.Activate)
			{
				_trap.Activate();
			}
			if (_type == Type.Deactivate)
			{
				_trap.Deactivate();
			}
		}
	}
}
