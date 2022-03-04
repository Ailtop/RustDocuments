using UnityEngine;

public class RadialSpawnPoint : BaseSpawnPoint
{
	public float radius = 10f;

	public override void GetLocation(out Vector3 pos, out Quaternion rot)
	{
		Vector2 vector = Random.insideUnitCircle * radius;
		pos = base.transform.position + new Vector3(vector.x, 0f, vector.y);
		rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		DropToGround(ref pos, ref rot);
	}

	public override bool HasPlayersIntersecting()
	{
		return BaseNetworkable.HasCloseConnections(base.transform.position, radius + 1f);
	}

	public override void ObjectSpawned(SpawnPointInstance instance)
	{
	}

	public override void ObjectRetired(SpawnPointInstance instance)
	{
	}
}
