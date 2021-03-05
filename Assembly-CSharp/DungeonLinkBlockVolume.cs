using UnityEngine;

public class DungeonLinkBlockVolume : MonoBehaviour
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	public OBB GetBounds(Vector3 position, Quaternion rotation)
	{
		position += rotation * (base.transform.localRotation * bounds.center + base.transform.localPosition);
		return new OBB(position, bounds.size, rotation * base.transform.localRotation);
	}
}
