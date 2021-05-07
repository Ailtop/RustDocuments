namespace Characters.Abilities.Customs
{
	public class ForbiddenSwordComponent : AbilityComponent<ForbiddenSword>
	{
		public int currentKillCount { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			base.baseAbility.component = this;
		}
	}
}
