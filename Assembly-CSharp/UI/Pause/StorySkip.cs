using UI.SkulStories;
using UnityEngine;

namespace UI.Pause
{
	public class StorySkip : PauseEvent
	{
		[SerializeField]
		private UI.SkulStories.Confirm _panel;

		public override void Invoke()
		{
			_panel.Open();
		}
	}
}
