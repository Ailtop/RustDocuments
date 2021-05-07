namespace Characters.Abilities.Customs
{
	public class RockstarPassiveComponent : AbilityComponent<RockstarPassive>
	{
		public void AddStack(int amount)
		{
			_ability.AddStack(amount);
		}
	}
}
