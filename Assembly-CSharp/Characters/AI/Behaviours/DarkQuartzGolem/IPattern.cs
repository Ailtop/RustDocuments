namespace Characters.AI.Behaviours.DarkQuartzGolem
{
	public interface IPattern
	{
		bool CanUse();

		bool CanUse(AIController controller);
	}
}
