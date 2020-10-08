using UnityEngine;

public class ConstructionSocket : Socket_Base
{
	public enum Type
	{
		None,
		Foundation,
		Floor,
		Misc,
		Doorway,
		Wall,
		Block,
		Ramp,
		StairsTriangle,
		Stairs,
		FloorFrameTriangle,
		Window,
		Shutters,
		WallFrame,
		FloorFrame,
		WindowDressing,
		DoorDressing,
		Elevator
	}

	public Type socketType;

	public int rotationDegrees;

	public int rotationOffset;

	public bool restrictPlacementRotation;

	public bool restrictPlacementAngle;

	public float faceAngle;

	public float angleAllowed = 150f;

	[Range(0f, 1f)]
	public float support = 1f;

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.6f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(Vector3.zero, Vector3.right * 0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.1f);
		Gizmos.DrawIcon(base.transform.position, "light_circle_green.png", false);
	}

	private void OnDrawGizmosSelected()
	{
		if (female)
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(selectCenter, selectSize);
		}
	}

	public override bool TestTarget(Construction.Target target)
	{
		if (!base.TestTarget(target))
		{
			return false;
		}
		return IsCompatible(target.socket);
	}

	public override bool IsCompatible(Socket_Base socket)
	{
		if (!base.IsCompatible(socket))
		{
			return false;
		}
		ConstructionSocket constructionSocket = socket as ConstructionSocket;
		if (constructionSocket == null)
		{
			return false;
		}
		if (constructionSocket.socketType == Type.None || socketType == Type.None)
		{
			return false;
		}
		if (constructionSocket.socketType != socketType)
		{
			return false;
		}
		return true;
	}

	public override bool CanConnect(Vector3 position, Quaternion rotation, Socket_Base socket, Vector3 socketPosition, Quaternion socketRotation)
	{
		if (!base.CanConnect(position, rotation, socket, socketPosition, socketRotation))
		{
			return false;
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(socketPosition, socketRotation, Vector3.one);
		Vector3 a = matrix4x.MultiplyPoint3x4(worldPosition);
		Vector3 b = matrix4x2.MultiplyPoint3x4(socket.worldPosition);
		if (Vector3.Distance(a, b) > 0.01f)
		{
			return false;
		}
		Vector3 vector = matrix4x.MultiplyVector(worldRotation * Vector3.forward);
		Vector3 vector2 = matrix4x2.MultiplyVector(socket.worldRotation * Vector3.forward);
		float num = Vector3.Angle(vector, vector2);
		if (male && female)
		{
			num = Mathf.Min(num, Vector3.Angle(-vector, vector2));
		}
		if (socket.male && socket.female)
		{
			num = Mathf.Min(num, Vector3.Angle(vector, -vector2));
		}
		if (num > 1f)
		{
			return false;
		}
		return true;
	}

	public bool TestRestrictedAngles(Vector3 suggestedPos, Quaternion suggestedAng, Construction.Target target)
	{
		if (restrictPlacementAngle)
		{
			Quaternion rotation = Quaternion.Euler(0f, faceAngle, 0f) * suggestedAng;
			float num = target.ray.direction.XZ3D().DotDegrees(rotation * Vector3.forward);
			if (num > angleAllowed * 0.5f)
			{
				return false;
			}
			if (num < angleAllowed * -0.5f)
			{
				return false;
			}
		}
		return true;
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		if (!target.entity || !target.entity.transform)
		{
			return null;
		}
		if (!CanConnectToEntity(target))
		{
			return null;
		}
		ConstructionSocket constructionSocket = target.socket as ConstructionSocket;
		Vector3 worldPosition = target.GetWorldPosition();
		Quaternion worldRotation = target.GetWorldRotation(true);
		if (constructionSocket != null && !IsCompatible(constructionSocket))
		{
			return null;
		}
		if (rotationDegrees > 0 && (constructionSocket == null || !constructionSocket.restrictPlacementRotation))
		{
			Construction.Placement placement = new Construction.Placement();
			float num = float.MaxValue;
			float num2 = 0f;
			for (int i = 0; i < 360; i += rotationDegrees)
			{
				Quaternion lhs = Quaternion.Euler(0f, rotationOffset + i, 0f);
				Vector3 direction = target.ray.direction;
				Vector3 to = lhs * worldRotation * Vector3.up;
				float num3 = Vector3.Angle(direction, to);
				if (num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
			for (int j = 0; j < 360; j += rotationDegrees)
			{
				Quaternion rhs = worldRotation * Quaternion.Inverse(base.rotation);
				Quaternion lhs2 = Quaternion.Euler(target.rotation);
				Quaternion rhs2 = Quaternion.Euler(0f, (float)(rotationOffset + j) + num2, 0f);
				Quaternion rotation = lhs2 * rhs2 * rhs;
				Vector3 b = rotation * position;
				placement.position = worldPosition - b;
				placement.rotation = rotation;
				if (CheckSocketMods(placement))
				{
					return placement;
				}
			}
		}
		Construction.Placement placement2 = new Construction.Placement();
		Quaternion rotation2 = worldRotation * Quaternion.Inverse(base.rotation);
		Vector3 b2 = rotation2 * position;
		placement2.position = worldPosition - b2;
		placement2.rotation = rotation2;
		if (!TestRestrictedAngles(worldPosition, worldRotation, target))
		{
			return null;
		}
		return placement2;
	}

	protected virtual bool CanConnectToEntity(Construction.Target target)
	{
		return true;
	}
}
