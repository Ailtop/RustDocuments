using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_Attraction : SocketMod
{
	public float outerRadius = 1f;

	public float innerRadius = 0.1f;

	public string groupName = "wallbottom";

	public bool lockRotation;

	public bool ignoreRotationForRadiusCheck;

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
		Gizmos.DrawSphere(Vector3.zero, outerRadius);
		Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
		Gizmos.DrawSphere(Vector3.zero, innerRadius);
	}

	public override bool DoCheck(Construction.Placement place)
	{
		return true;
	}

	public override void ModifyPlacement(Construction.Placement place)
	{
		Vector3 vector = place.position + place.rotation * worldPosition;
		List<BaseEntity> obj = Pool.GetList<BaseEntity>();
		Vis.Entities(vector, outerRadius * 2f, obj);
		Vector3 position = Vector3.zero;
		float num = float.MaxValue;
		Vector3 position2 = place.position;
		Quaternion rotation = Quaternion.identity;
		foreach (BaseEntity item in obj)
		{
			if (item.isServer != isServer)
			{
				continue;
			}
			AttractionPoint[] array = prefabAttribute.FindAll<AttractionPoint>(item.prefabID);
			if (array == null)
			{
				continue;
			}
			AttractionPoint[] array2 = array;
			foreach (AttractionPoint attractionPoint in array2)
			{
				if (attractionPoint.groupName != groupName)
				{
					continue;
				}
				Vector3 vector2 = item.transform.position + item.transform.rotation * attractionPoint.worldPosition;
				float magnitude = (vector2 - vector).magnitude;
				if (ignoreRotationForRadiusCheck)
				{
					Vector3 vector3 = item.transform.TransformPoint(Vector3.LerpUnclamped(Vector3.zero, attractionPoint.worldPosition.WithY(0f), 2f));
					float num2 = Vector3.Distance(vector3, position2);
					if (num2 < num)
					{
						num = num2;
						position = vector3;
						rotation = item.transform.rotation;
					}
				}
				if (!(magnitude > outerRadius))
				{
					Quaternion b = QuaternionEx.LookRotationWithOffset(worldPosition, vector2 - place.position, Vector3.up);
					float num3 = Mathf.InverseLerp(outerRadius, innerRadius, magnitude);
					if (lockRotation)
					{
						num3 = 1f;
					}
					if (lockRotation)
					{
						Vector3 eulerAngles = place.rotation.eulerAngles;
						eulerAngles -= new Vector3(eulerAngles.x % 90f, eulerAngles.y % 90f, eulerAngles.z % 90f);
						place.rotation = Quaternion.Euler(eulerAngles + item.transform.eulerAngles);
					}
					else
					{
						place.rotation = Quaternion.Lerp(place.rotation, b, num3);
					}
					vector = place.position + place.rotation * worldPosition;
					Vector3 vector4 = vector2 - vector;
					place.position += vector4 * num3;
				}
			}
		}
		if (num < float.MaxValue && ignoreRotationForRadiusCheck)
		{
			place.position = position;
			place.rotation = rotation;
		}
		Pool.FreeList(ref obj);
	}
}
