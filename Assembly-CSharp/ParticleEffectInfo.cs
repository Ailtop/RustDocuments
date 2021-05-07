using Characters.Movements;
using Data;
using Level;
using UnityEngine;

[CreateAssetMenu]
public class ParticleEffectInfo : ScriptableObject
{
	[SerializeField]
	private float multiplier = 1f;

	[SerializeField]
	private DroppedParts[] _parts;

	public void Emit(Vector2 position, Bounds bounds, Push push)
	{
		Vector2 force = Vector2.zero;
		if (push != null && !push.expired)
		{
			force = push.direction * push.totalForce;
		}
		Emit(position, bounds, force);
	}

	public void Emit(Vector2 position, Bounds bounds, Vector2 force, bool interpolate = true)
	{
		if (GameData.Settings.particleQuality == 0)
		{
			return;
		}
		DroppedParts[] parts = _parts;
		foreach (DroppedParts droppedParts in parts)
		{
			if (droppedParts == null)
			{
				Debug.LogError(base.name + " : A parts is missing!");
				continue;
			}
			bool flag = droppedParts.collideWithTerrain;
			if (GameData.Settings.particleQuality == 2)
			{
				if (droppedParts.priority == DroppedParts.Priority.Low)
				{
					flag = false;
				}
			}
			else if (GameData.Settings.particleQuality == 1 && droppedParts.priority == DroppedParts.Priority.Low)
			{
				flag = false;
			}
			int num = Random.Range(droppedParts.count.x, droppedParts.count.y + 1);
			int layer = (flag ? ((GameData.Settings.particleQuality > 1) ? 11 : 27) : ((droppedParts.gameObject.layer != 11) ? droppedParts.gameObject.layer : 0));
			for (int j = 0; j < num; j++)
			{
				Vector2 vector;
				Quaternion rotation;
				if (droppedParts.randomize)
				{
					vector = MMMaths.RandomPointWithinBounds(bounds) + (Vector2)droppedParts.transform.position;
					rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
				}
				else
				{
					vector = position + (Vector2)droppedParts.transform.position;
					rotation = droppedParts.transform.rotation;
				}
				PoolObject poolObject = droppedParts.poolObject.Spawn(vector, rotation);
				poolObject.GetComponent<DroppedParts>().Initialize(force, multiplier * 3f, interpolate);
				poolObject.gameObject.layer = layer;
			}
		}
	}
}
