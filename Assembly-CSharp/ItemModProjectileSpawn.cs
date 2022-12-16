using ConVar;
using UnityEngine;

public class ItemModProjectileSpawn : ItemModProjectile
{
	public float createOnImpactChance;

	public GameObjectRef createOnImpact = new GameObjectRef();

	public float spreadAngle = 30f;

	public float spreadVelocityMin = 1f;

	public float spreadVelocityMax = 3f;

	public int numToCreateChances = 1;

	public override void ServerProjectileHit(HitInfo info)
	{
		for (int i = 0; i < numToCreateChances; i++)
		{
			if (!createOnImpact.isValid || !(Random.Range(0f, 1f) < createOnImpactChance))
			{
				continue;
			}
			Vector3 hitPositionWorld = info.HitPositionWorld;
			Vector3 pointStart = info.PointStart;
			Vector3 normalized = info.ProjectileVelocity.normalized;
			Vector3 normalized2 = info.HitNormalWorld.normalized;
			Vector3 vector = hitPositionWorld - normalized * 0.1f;
			Quaternion rotation = Quaternion.LookRotation(-normalized);
			int layerMask = (ConVar.AntiHack.projectile_terraincheck ? 10551296 : 2162688);
			if (!GamePhysics.LineOfSight(pointStart, vector, layerMask))
			{
				continue;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(createOnImpact.resourcePath);
			if ((bool)baseEntity)
			{
				baseEntity.transform.position = vector;
				baseEntity.transform.rotation = rotation;
				baseEntity.Spawn();
				if (spreadAngle > 0f)
				{
					Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(spreadAngle, normalized2);
					baseEntity.SetVelocity(modifiedAimConeDirection * Random.Range(1f, 3f));
				}
			}
		}
		base.ServerProjectileHit(info);
	}
}
