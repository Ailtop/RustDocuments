namespace Characters.Abilities.Customs
{
	public class DoomsdayComponent : AbilityComponent<Doomsday>, IAttackDamage
	{
		public float amount { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			base.baseAbility.component = this;
		}
	}
}
