using ProtoBuf;
using UnityEngine;

public class MapMarkerCH47 : MapMarker
{
	private GameObject createdMarker;

	private float GetRotation()
	{
		BaseEntity parentEntity = GetParentEntity();
		if (!parentEntity)
		{
			return 0f;
		}
		Vector3 forward = parentEntity.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		return Mathf.Atan2(forward.x, 0f - forward.z) * 57.29578f + 180f;
	}

	public override AppMarker GetAppMarkerData()
	{
		AppMarker appMarkerData = base.GetAppMarkerData();
		appMarkerData.rotation = GetRotation();
		return appMarkerData;
	}
}
