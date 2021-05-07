using UnityEngine;

namespace UI.Pause
{
	public class PauseMenuPopUp : PauseEvent
	{
		[SerializeField]
		private Panel _panel;

		public override void Invoke()
		{
			_panel.Open();
		}
	}
}
