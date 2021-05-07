using Runnables;
using UnityEngine;

namespace SkulStories
{
	public sealed class ExecuteRunnable : Event
	{
		[SerializeField]
		[Runnable.Subcomponent]
		private Runnable _runnable;

		public override void Run()
		{
			_runnable.Run();
		}
	}
}
