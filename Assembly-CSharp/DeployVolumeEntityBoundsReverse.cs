using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class DeployVolumeEntityBoundsReverse : DeployVolume
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	public int layer;

	protected override bool Check(Vector3 position, Quaternion rotation, int mask = -1)
	{
		position += rotation * bounds.center;
		OBB test = new OBB(position, bounds.size, rotation);
		List<BaseEntity> obj = Pool.GetList<BaseEntity>();
		Vis.Entities(position, test.extents.magnitude, obj, (int)layers & mask);
		foreach (BaseEntity item in obj)
		{
			DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(item.prefabID);
			if (DeployVolume.Check(item.transform.position, item.transform.rotation, volumes, test, 1 << layer))
			{
				Pool.FreeList(ref obj);
				return true;
			}
		}
		Pool.FreeList(ref obj);
		return false;
	}

	protected override bool Check(Vector3 position, Quaternion rotation, OBB test, int mask = -1)
	{
		return false;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
		layer = rootObj.layer;
	}
}
