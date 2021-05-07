using Services;
using Singletons;

namespace Characters.Operations.LookAtTargets
{
	public sealed class Player : Target
	{
		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (player == null)
			{
				return character.lookingDirection;
			}
			if (player.transform.position.x > character.transform.position.x)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
