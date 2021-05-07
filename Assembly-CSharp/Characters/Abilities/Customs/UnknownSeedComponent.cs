namespace Characters.Abilities.Customs
{
	public class UnknownSeedComponent : AbilityComponent<UnknownSeed>
	{
		public float healed { get; set; }

		public float healedBefore { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			base.baseAbility.component = this;
		}
	}
}
