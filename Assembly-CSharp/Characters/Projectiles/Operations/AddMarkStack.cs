using Characters.Marks;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class AddMarkStack : CharacterHitOperation
	{
		[SerializeField]
		private MarkInfo _mark;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		[SerializeField]
		private float _count = 1f;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character target)
		{
			if (MMMaths.PercentChance(_chance) && !(target == null) && !(target.health == null) && !target.health.dead)
			{
				target.mark.AddStack(_mark, _count);
			}
		}
	}
}
