using Scenes;

namespace Runnables
{
	public sealed class CompleteUIConversation : UICommands
	{
		public override void Run()
		{
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
		}
	}
}
