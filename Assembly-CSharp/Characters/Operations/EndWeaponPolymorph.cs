namespace Characters.Operations
{
	public class EndWeaponPolymorph : CharacterOperation
	{
		public override void Run(Character target)
		{
			target.playerComponents.inventory.weapon.Unpolymorph();
		}
	}
}
