using Level.Traps;
using UnityEngine;

namespace Runnables
{
	public class ControlTrap : Runnable
	{
		private enum Type
		{
			Activate,
			Deactivate,
			Toggle
		}

		[SerializeField]
		private ControlableTrap _trap;

		[SerializeField]
		private Type _type;

		public override void Run()
		{
			switch (_type)
			{
			case Type.Activate:
				_trap.Activate();
				break;
			case Type.Deactivate:
				_trap.Deactivate();
				break;
			case Type.Toggle:
				if (_trap.activated)
				{
					_trap.Deactivate();
				}
				else
				{
					_trap.Activate();
				}
				break;
			}
		}
	}
}
