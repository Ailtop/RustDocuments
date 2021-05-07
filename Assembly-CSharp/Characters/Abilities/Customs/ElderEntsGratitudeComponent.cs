namespace Characters.Abilities.Customs
{
	public class ElderEntsGratitudeComponent : AbilityComponent<ElderEntsGratitude>
	{
		public double shieldAmount { get; set; }

		private void Awake()
		{
			shieldAmount = base.baseAbility.amount;
		}

		public override void Initialize()
		{
			base.Initialize();
			base.baseAbility.component = this;
		}
	}
}
