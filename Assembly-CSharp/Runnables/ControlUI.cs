using UnityEngine;

namespace Runnables
{
	public sealed class ControlUI : Runnable
	{
		[SerializeField]
		[UICommands.Subcomponent]
		private UICommands _commands;

		public override void Run()
		{
			_commands.Run();
		}
	}
}
