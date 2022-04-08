using UnityEngine;

public class TerrainCollisionTrigger : EnvironmentVolumeTrigger
{
	protected void OnTriggerEnter(Collider other)
	{
		if ((bool)TerrainMeta.Collision && !other.isTrigger)
		{
			UpdateCollider(other, state: true);
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if ((bool)TerrainMeta.Collision && !other.isTrigger)
		{
			UpdateCollider(other, state: false);
		}
	}

	private void UpdateCollider(Collider other, bool state)
	{
		TerrainMeta.Collision.SetIgnore(other, base.volume.trigger, state);
		TerrainCollisionProxy component = other.GetComponent<TerrainCollisionProxy>();
		if ((bool)component)
		{
			for (int i = 0; i < component.colliders.Length; i++)
			{
				TerrainMeta.Collision.SetIgnore(component.colliders[i], base.volume.trigger, state);
			}
		}
	}
}
