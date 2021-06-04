using UnityEngine;

public class ConstructionSocket_Elevator : ConstructionSocket
{
	public int MaxFloor = 5;

	protected override bool CanConnectToEntity(Construction.Target target)
	{
		Elevator elevator;
		if ((object)(elevator = target.entity as Elevator) != null && elevator.Floor >= MaxFloor)
		{
			return false;
		}
		Vector3 vector = target.GetWorldPosition();
		Quaternion quaternion = target.GetWorldRotation(true);
		if (GamePhysics.CheckOBB(new OBB(vector, new Vector3(2f, 0.5f, 2f), quaternion), 2097152))
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
		Vector3 vector = Matrix4x4.TRS(position, rotation, Vector3.one).MultiplyPoint3x4(worldPosition);
		return !GamePhysics.CheckOBB(new OBB(vector, new Vector3(2f, 0.5f, 2f), rotation), 2097152);
	}
}
