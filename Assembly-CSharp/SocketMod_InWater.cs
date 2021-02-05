using UnityEngine;

public class SocketMod_InWater : SocketMod
{
	public bool wantsInWater = true;

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(Vector3.zero, 0.1f);
	}

	public override bool DoCheck(Construction.Placement place)
	{
		Vector3 vector = place.position + place.rotation * worldPosition;
		bool flag = WaterLevel.Test(vector);
		if (!flag && wantsInWater && GamePhysics.CheckSphere(vector, 0.1f, 16))
		{
			flag = true;
		}
		if (flag == wantsInWater)
		{
			return true;
		}
		Construction.lastPlacementError = "Failed Check: InWater (" + hierachyName + ")";
		return false;
	}
}
