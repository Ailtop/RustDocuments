using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Delivery Drone Config")]
public class DeliveryDroneConfig : BaseScriptableObject
{
	public Vector3 vendingMachineOffset = new Vector3(0f, 1f, 1f);

	public float maxDistanceFromVendingMachine = 1f;

	public Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);

	public float testHeight = 200f;

	public LayerMask layerMask = 27328768;

	public void FindDescentPoints(VendingMachine vendingMachine, float currentY, out Vector3 waitPosition, out Vector3 descendPosition)
	{
		float num = maxDistanceFromVendingMachine / 4f;
		for (int i = 0; i <= 4; i++)
		{
			Vector3 b = Vector3.forward * (num * (float)i);
			Vector3 vector = vendingMachine.transform.TransformPoint(vendingMachineOffset + b);
			Vector3 vector2 = vector + Vector3.up * testHeight;
			RaycastHit hitInfo;
			if (!Physics.BoxCast(vector2, halfExtents, Vector3.down, out hitInfo, vendingMachine.transform.rotation, testHeight, layerMask))
			{
				waitPosition = vector;
				descendPosition = vector2.WithY(currentY);
				return;
			}
			if (i == 4)
			{
				waitPosition = vector2 + Vector3.down * (hitInfo.distance - halfExtents.y * 2f);
				descendPosition = vector2.WithY(currentY);
				return;
			}
		}
		throw new Exception("Bug: FindDescentPoint didn't return a fallback value");
	}

	public bool IsVendingMachineAccessible(VendingMachine vendingMachine, Vector3 offset, out RaycastHit hitInfo)
	{
		Vector3 vector = vendingMachine.transform.TransformPoint(offset);
		if (Physics.BoxCast(vector + Vector3.up * testHeight, halfExtents, Vector3.down, out hitInfo, vendingMachine.transform.rotation, testHeight, layerMask))
		{
			return false;
		}
		return vendingMachine.IsVisibleAndCanSee(vector, 2f);
	}
}
