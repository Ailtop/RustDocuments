using Rust;
using UnityEngine;

public class SpawnPointInstance : MonoBehaviour
{
	public ISpawnPointUser parentSpawnPointUser;

	public BaseSpawnPoint parentSpawnPoint;

	public void Notify()
	{
		if (!parentSpawnPointUser.IsUnityNull())
		{
			parentSpawnPointUser.ObjectSpawned(this);
		}
		if ((bool)parentSpawnPoint)
		{
			parentSpawnPoint.ObjectSpawned(this);
		}
	}

	protected void OnDestroy()
	{
		if (!Rust.Application.isQuitting)
		{
			if (!parentSpawnPointUser.IsUnityNull())
			{
				parentSpawnPointUser.ObjectRetired(this);
			}
			if ((bool)parentSpawnPoint)
			{
				parentSpawnPoint.ObjectRetired(this);
			}
		}
	}
}
