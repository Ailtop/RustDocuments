using Scenes;

namespace Characters.Abilities.Constraints
{
	public class Story : Constraint
	{
		public override bool Pass()
		{
			return !Scene<GameBase>.instance.uiManager.narration.sceneVisible;
		}
	}
}
