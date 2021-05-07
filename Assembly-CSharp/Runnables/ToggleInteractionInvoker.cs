using Characters;
using UnityEngine;

namespace Runnables
{
	public class ToggleInteractionInvoker : InteractiveObject
	{
		[SerializeField]
		private bool _on;

		[SerializeField]
		private Runnable _onExecute;

		[SerializeField]
		private Runnable _offExecute;

		public override void InteractWith(Character character)
		{
			_on = !_on;
			if (_on)
			{
				_onExecute.Run();
			}
			else
			{
				_offExecute.Run();
			}
		}
	}
}
