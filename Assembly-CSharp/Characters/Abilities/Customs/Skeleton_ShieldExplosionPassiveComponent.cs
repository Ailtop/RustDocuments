using System;

namespace Characters.Abilities.Customs
{
	public class Skeleton_ShieldExplosionPassiveComponent : AbilityComponent<Skeleton_ShieldExplosionPassive>, IAttackDamage
	{
		[NonSerialized]
		public float attackDamage;

		public float amount => attackDamage;

		public override void Initialize()
		{
			base.Initialize();
			_ability.component = this;
		}
	}
}
