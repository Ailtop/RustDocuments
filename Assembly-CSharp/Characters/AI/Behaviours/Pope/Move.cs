using Characters.AI.Pope;

namespace Characters.AI.Behaviours.Pope
{
	public abstract class Move : Behaviour
	{
		public abstract void SetDestination(Point.Tag tag);
	}
}
