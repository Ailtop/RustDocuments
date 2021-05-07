using Scenes;
using UnityEngine;

namespace Runnables
{
	public sealed class SetHeadUpDisplay : UICommands
	{
		[SerializeField]
		private bool _visible;

		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = _visible;
		}
	}
}
