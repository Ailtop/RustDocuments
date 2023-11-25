using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_EntityType : SocketMod
{
	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public QueryTriggerInteraction queryTriggers;

	public BaseEntity searchType;

	public bool wantsCollide;

	public static Translate.Phrase ErrorPhrase = new Translate.Phrase("error_entitytype", "Invalid entity type");

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = (wantsCollide ? new Color(0f, 1f, 0f, 0.7f) : new Color(1f, 0f, 0f, 0.7f));
		Gizmos.DrawSphere(Vector3.zero, sphereRadius);
	}

	public override bool DoCheck(Construction.Placement place)
	{
		bool flag = !wantsCollide;
		Vector3 position = place.position + place.rotation * worldPosition;
		List<BaseEntity> obj = Pool.GetList<BaseEntity>();
		Vis.Entities(position, sphereRadius, obj, layerMask.value, queryTriggers);
		foreach (BaseEntity item in obj)
		{
			bool flag2 = item.GetType().IsAssignableFrom(searchType.GetType());
			if (flag2 && wantsCollide)
			{
				flag = true;
				break;
			}
			if (flag2 && !wantsCollide)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			Construction.lastPlacementError = ErrorPhrase.translated;
		}
		Pool.FreeList(ref obj);
		return flag;
	}
}
