using Runnables;
using UnityEngine;

namespace SkulStories
{
	public class Skip : Runnable
	{
		[SerializeField]
		[Event.Subcomponent]
		private Event.Subcomponents _onSkip;

		public override void Run()
		{
			_onSkip.Run();
		}
	}
}
