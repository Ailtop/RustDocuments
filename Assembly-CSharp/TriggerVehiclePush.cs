using UnityEngine;

public class TriggerVehiclePush : TriggerBase, IServerComponent
{
	public BaseEntity thisEntity;

	public float maxPushVelocity = 10f;

	public float minRadius;

	public float maxRadius;

	public bool snapToAxis;

	public Vector3 axisToSnapTo = Vector3.right;

	public bool allowParentRigidbody;

	public bool useRigidbodyPosition;

	public bool useCentreOfMass;

	public int ContentsCount => entityContents?.Count ?? 0;

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity is BuildingBlock)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	public void FixedUpdate()
	{
		if (thisEntity == null || entityContents == null)
		{
			return;
		}
		Vector3 position = base.transform.position;
		foreach (BaseEntity entityContent in entityContents)
		{
			if (!BaseNetworkableEx.IsValid(entityContent) || entityContent.EqualNetID(thisEntity))
			{
				continue;
			}
			Rigidbody rigidbody = entityContent.GetComponent<Rigidbody>();
			if (rigidbody == null && allowParentRigidbody)
			{
				rigidbody = entityContent.GetComponentInParent<Rigidbody>();
			}
			if ((bool)rigidbody && !rigidbody.isKinematic)
			{
				float value = Vector3Ex.Distance2D(useRigidbodyPosition ? rigidbody.transform.position : entityContent.transform.position, base.transform.position);
				float num = 1f - Mathf.InverseLerp(minRadius, maxRadius, value);
				float num2 = 1f - Mathf.InverseLerp(minRadius - 1f, minRadius, value);
				Vector3 vector = entityContent.ClosestPoint(position);
				Vector3 vector2 = Vector3Ex.Direction2D(vector, position);
				vector2 = Vector3Ex.Direction2D(useCentreOfMass ? rigidbody.worldCenterOfMass : vector, position);
				if (snapToAxis)
				{
					Vector3 from = base.transform.InverseTransformDirection(vector2);
					vector2 = ((!(Vector3.Angle(from, axisToSnapTo) < Vector3.Angle(from, -axisToSnapTo))) ? (-base.transform.TransformDirection(axisToSnapTo)) : base.transform.TransformDirection(axisToSnapTo));
				}
				rigidbody.AddForceAtPosition(vector2 * maxPushVelocity * num, vector, ForceMode.Acceleration);
				if (num2 > 0f)
				{
					rigidbody.AddForceAtPosition(vector2 * 1f * num2, vector, ForceMode.VelocityChange);
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, minRadius);
		Gizmos.color = new Color(0.5f, 0f, 0f, 1f);
		Gizmos.DrawWireSphere(base.transform.position, maxRadius);
		if (snapToAxis)
		{
			Gizmos.color = Color.cyan;
			Vector3 vector = base.transform.TransformDirection(axisToSnapTo);
			Gizmos.DrawLine(base.transform.position + vector, base.transform.position - vector);
		}
	}
}
