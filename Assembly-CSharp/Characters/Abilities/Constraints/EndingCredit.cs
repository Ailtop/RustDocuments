using Scenes;

namespace Characters.Abilities.Constraints
{
	public class EndingCredit : Constraint
	{
		public override bool Pass()
		{
			return !Scene<GameBase>.instance.uiManager.endingCredit.gameObject.activeInHierarchy;
		}
	}
}
