using Characters;
using Scenes;
using UI;
using UnityEngine;

namespace Runnables
{
	public sealed class OpenUIHealthBar : UICommands
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private BossHealthbarController.Type _type;

		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.Open(_type, _character);
		}

		private void OnDestroy()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
		}
	}
}
