namespace Characters.Abilities.Customs
{
	public class BombSkulPassiveComponent : AbilityComponent<BombSkulPassive>
	{
		public void Explode()
		{
			_ability.Explode();
		}

		public void RiskyUpgrade()
		{
			_ability.RiskyUpgrade();
		}

		public void AddDamageStack(int amount)
		{
			_ability.AddDamageStack(amount);
		}

		public void RegisterSmallBomb(OperationRunner smallBomb)
		{
			_ability.RegisterSmallBomb(smallBomb);
		}
	}
}
