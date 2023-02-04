using UnityEngine;

public class BoxStorage : StorageContainer
{
	public override Vector3 GetDropPosition()
	{
		return ClosestPoint(base.GetDropPosition() + base.LastAttackedDir * 10f);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (children.Count == 0)
		{
			return base.CanPickup(player);
		}
		return false;
	}
}
