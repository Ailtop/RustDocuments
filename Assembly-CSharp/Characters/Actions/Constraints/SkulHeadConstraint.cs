namespace Characters.Actions.Constraints
{
	public class SkulHeadConstraint : Constraint
	{
		public override bool Pass()
		{
			if (SkulHeadToTeleport.instance != null)
			{
				return SkulHeadToTeleport.instance.gameObject.activeSelf;
			}
			return false;
		}
	}
}
