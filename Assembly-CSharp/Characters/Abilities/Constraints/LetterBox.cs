using Scenes;

namespace Characters.Abilities.Constraints
{
	public class LetterBox : Constraint
	{
		public override bool Pass()
		{
			return !Scene<GameBase>.instance.uiManager.letterBox.visible;
		}
	}
}
