using Scenes;

namespace Characters.Abilities.Constraints
{
	public class Dialogue : Constraint
	{
		public override bool Pass()
		{
			return !Scene<GameBase>.instance.uiManager.npcConversation.visible;
		}
	}
}
