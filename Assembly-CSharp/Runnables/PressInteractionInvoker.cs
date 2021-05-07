using Characters;
using UI;
using UnityEngine;

namespace Runnables
{
	public class PressInteractionInvoker : InteractiveObject
	{
		[SerializeField]
		private PressingButton _releaseButton;

		[SerializeField]
		private Runnable _cutScene;

		private void Start()
		{
			_releaseButton.onPressed += _cutScene.Run;
		}

		public override void InteractWith(Character character)
		{
		}

		private void OnDestroy()
		{
			if (!(_releaseButton == null))
			{
				_releaseButton.onPressed -= _cutScene.Run;
			}
		}
	}
}
