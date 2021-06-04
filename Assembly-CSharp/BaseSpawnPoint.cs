using UnityEngine;

public abstract class BaseSpawnPoint : MonoBehaviour, IServerComponent
{
	public abstract void GetLocation(out Vector3 pos, out Quaternion rot);

	public abstract void ObjectSpawned(SpawnPointInstance instance);

	public abstract void ObjectRetired(SpawnPointInstance instance);

	public virtual bool IsAvailableTo(GameObjectRef prefabRef)
	{
		return base.gameObject.activeSelf;
	}

	protected void DropToGround(ref Vector3 pos, ref Quaternion rot)
	{
		if ((bool)TerrainMeta.HeightMap && (bool)TerrainMeta.Collision && !TerrainMeta.Collision.GetIgnore(pos))
		{
			float height = TerrainMeta.HeightMap.GetHeight(pos);
			pos.y = Mathf.Max(pos.y, height);
		}
		RaycastHit hitOut;
		if (TransformUtil.GetGroundInfo(pos, out hitOut, 20f, 1235288065))
		{
			pos = hitOut.point;
			rot = Quaternion.LookRotation(rot * Vector3.forward, hitOut.normal);
		}
	}
}
