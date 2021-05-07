namespace Characters.Abilities.Customs
{
	public class MummyGunDropPassiveComponent : AbilityComponent<MummyGunDropPassive>
	{
		public void SupplyGunBySwap()
		{
			_ability.SupplyGunBySwap();
		}
	}
}
