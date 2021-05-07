using Scenes;

namespace Runnables
{
	public sealed class CloseAllUIHealthBar : UICommands
	{
		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
		}
	}
}
