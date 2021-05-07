using Characters.Operations.Attack;
using Characters.Projectiles;
using UnityEngine;

namespace Characters.Operations.Customs.AquaSkull
{
	public class SetFloodSweepAttackDamageMultiplier : Operation
	{
		[SerializeField]
		private SweepAttack _sweepAttack;

		[SerializeField]
		private Projectile[] _projectilesToCount;

		[SerializeField]
		private float[] _damageMultiplierByCount;

		public override void Run()
		{
			int num = 0;
			Projectile[] projectilesToCount = _projectilesToCount;
			foreach (Projectile projectile in projectilesToCount)
			{
				num += projectile.reusable.spawnedCount;
			}
			int num2 = Mathf.Clamp(num, 0, _damageMultiplierByCount.Length - 1);
			_sweepAttack.hitInfo.damageMultiplier = _damageMultiplierByCount[num2];
		}
	}
}
