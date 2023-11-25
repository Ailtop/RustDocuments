using System.Collections.Generic;
using UnityEngine;

public class TriggerSafeZone : TriggerBase
{
	public static List<TriggerSafeZone> allSafeZones = new List<TriggerSafeZone>();

	public float maxDepth = 20f;

	public float maxAltitude = -1f;

	public Collider triggerCollider { get; private set; }

	protected void Awake()
	{
		triggerCollider = GetComponent<Collider>();
		interestLayers.value |= 512;
	}

	protected void OnEnable()
	{
		allSafeZones.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		allSafeZones.Remove(this);
	}

	internal override GameObject InterestedInObject(GameObject obj)
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
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	public bool PassesHeightChecks(Vector3 entPos)
	{
		Vector3 position = base.transform.position;
		float num = Mathf.Abs(position.y - entPos.y);
		if (maxDepth != -1f && entPos.y < position.y && num > maxDepth)
		{
			return false;
		}
		if (maxAltitude != -1f && entPos.y > position.y && num > maxAltitude)
		{
			return false;
		}
		return true;
	}

	public float GetSafeLevel(Vector3 pos)
	{
		if (!PassesHeightChecks(pos))
		{
			return 0f;
		}
		return 1f;
	}
}
