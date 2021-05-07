using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class ApplyStatus : CharacterHitOperation
	{
		[SerializeField]
		private CharacterStatus.ApplyInfo _status;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character target)
		{
			if (MMMaths.PercentChance(_chance))
			{
				projectile.owner.GiveStatus(target, _status);
			}
		}
	}
}
