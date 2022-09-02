using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SpawnableBoundsBlocker : MonoBehaviour
{
	public BoundsCheck.BlockType BlockType;

	public BoxCollider BoxCollider;

	public void ClearTrees()
	{
		List<TreeEntity> obj = Pool.GetList<TreeEntity>();
		if (BoxCollider != null)
		{
			GamePhysics.OverlapOBB(new OBB(base.transform.TransformPoint(BoxCollider.center), BoxCollider.size, base.transform.rotation), obj, 262144, QueryTriggerInteraction.Collide);
		}
		foreach (TreeEntity item in obj)
		{
			BoundsCheck boundsCheck = PrefabAttribute.server.Find<BoundsCheck>(item.prefabID);
			if (boundsCheck != null && boundsCheck.IsType == BlockType)
			{
				item.Kill();
			}
		}
		Pool.FreeList(ref obj);
	}
}
