using UnityEngine;

public class Socket_Specific : Socket_Base
{
	public bool useFemaleRotation = true;

	public string targetSocketName;

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.2f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(Vector3.zero, Vector3.right * 0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.1f);
		Gizmos.DrawIcon(base.transform.position, "light_circle_green.png", false);
	}

	public override bool TestTarget(Construction.Target target)
	{
		if (!base.TestTarget(target))
		{
			return false;
		}
		Socket_Specific_Female socket_Specific_Female = target.socket as Socket_Specific_Female;
		if (socket_Specific_Female == null)
		{
			return false;
		}
		return socket_Specific_Female.CanAccept(this);
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		Quaternion rhs = target.socket.rotation;
		if (target.socket.male && target.socket.female)
		{
			rhs = target.socket.rotation * Quaternion.Euler(180f, 0f, 180f);
		}
		Transform transform = target.entity.transform;
		Vector3 a = transform.localToWorldMatrix.MultiplyPoint3x4(target.socket.position);
		Quaternion lhs;
		if (useFemaleRotation)
		{
			lhs = transform.rotation * rhs;
		}
		else
		{
			Vector3 a2 = new Vector3(a.x, 0f, a.z);
			Vector3 b = new Vector3(target.player.eyes.position.x, 0f, target.player.eyes.position.z);
			lhs = Quaternion.LookRotation((a2 - b).normalized) * rhs;
		}
		Construction.Placement placement = new Construction.Placement();
		Quaternion rotation = lhs * Quaternion.Inverse(base.rotation);
		Vector3 b2 = rotation * position;
		placement.position = a - b2;
		placement.rotation = rotation;
		return placement;
	}
}
