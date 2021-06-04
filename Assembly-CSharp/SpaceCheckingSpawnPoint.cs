using UnityEngine;

public class SpaceCheckingSpawnPoint : GenericSpawnPoint
{
	[SerializeField]
	private bool useCustomBoundsCheckMask;

	[SerializeField]
	private LayerMask customBoundsCheckMask;

	public override bool IsAvailableTo(GameObjectRef prefabRef)
	{
		if (!base.IsAvailableTo(prefabRef))
		{
			return false;
		}
		if (useCustomBoundsCheckMask)
		{
			return SpawnHandler.CheckBounds(prefabRef.Get(), base.transform.position, base.transform.rotation, Vector3.one, customBoundsCheckMask);
		}
		return SingletonComponent<SpawnHandler>.Instance.CheckBounds(prefabRef.Get(), base.transform.position, base.transform.rotation, Vector3.one);
	}
}
