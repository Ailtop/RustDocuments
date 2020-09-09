using Facepunch;
using System.Collections.Generic;
using UnityEngine;

public class SocketMod_Attraction : SocketMod
{
	public float outerRadius = 1f;

	public float innerRadius = 0.1f;

	public string groupName = "wallbottom";

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
		foreach (BaseEntity item in obj)
		{
			if (item.isServer == isServer)
			{
				AttractionPoint[] array = prefabAttribute.FindAll<AttractionPoint>(item.prefabID);
				if (array != null)
				{
					AttractionPoint[] array2 = array;
					foreach (AttractionPoint attractionPoint in array2)
					{
						if (!(attractionPoint.groupName != groupName))
						{
							Vector3 a = item.transform.position + item.transform.rotation * attractionPoint.worldPosition;
							float magnitude = (a - vector).magnitude;
							if (!(magnitude > outerRadius))
							{
								Quaternion b = QuaternionEx.LookRotationWithOffset(worldPosition, a - place.position, Vector3.up);
								float num = Mathf.InverseLerp(outerRadius, innerRadius, magnitude);
								place.rotation = Quaternion.Lerp(place.rotation, b, num);
								vector = place.position + place.rotation * worldPosition;
								Vector3 a2 = a - vector;
								place.position += a2 * num;
							}
						}
					}
				}
			}
		}
		Pool.FreeList(ref obj);
	}
}
