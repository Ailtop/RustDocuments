using Characters;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public sealed class OpenChapter4Phase1 : UICommands
	{
		[SerializeField]
		private Character _left;

		[SerializeField]
		private Character _right;

		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.OpenChapter4Phase1(_left, _right);
		}
	}
}
