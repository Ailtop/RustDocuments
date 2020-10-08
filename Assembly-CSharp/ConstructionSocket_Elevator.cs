using UnityEngine;

public class ConstructionSocket_Elevator : ConstructionSocket
{
	public int MaxFloor = 5;

	protected override bool CanConnectToEntity(Construction.Target target)
	{
		Elevator elevator;
		if ((object)(elevator = (target.entity as Elevator)) != null && elevator.Floor >= MaxFloor)
		{
			return false;
		}
		Vector3 worldPosition = target.GetWorldPosition();
		Quaternion worldRotation = target.GetWorldRotation(true);
		if (GamePhysics.CheckOBB(new OBB(worldPosition, new Vector3(2f, 0.5f, 2f), worldRotation), 2097152))
		{
			return false;
		}
		return base.CanConnectToEntity(target);
	}

	public override bool CanConnect(Vector3 position, Quaternion rotation, Socket_Base socket, Vector3 socketPosition, Quaternion socketRotation)
	{
		if (!base.CanConnect(position, rotation, socket, socketPosition, socketRotation))
		{
			return false;
		}
		Vector3 position2 = Matrix4x4.TRS(position, rotation, Vector3.one).MultiplyPoint3x4(worldPosition);
		return !GamePhysics.CheckOBB(new OBB(position2, new Vector3(2f, 0.5f, 2f), rotation), 2097152);
	}
}
