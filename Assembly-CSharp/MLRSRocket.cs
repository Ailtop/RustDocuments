using Rust;
using UnityEngine;

public class MLRSRocket : TimedExplosive, SamSite.ISamSiteTarget
{
	[SerializeField]
	private GameObjectRef mapMarkerPrefab;

	[SerializeField]
	private GameObjectRef launchBlastFXPrefab;

	[SerializeField]
	private GameObjectRef explosionGroundFXPrefab;

	[SerializeField]
	private ServerProjectile serverProjectile;

	private EntityRef mapMarkerInstanceRef;

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeMissile;

	public override void ServerInit()
	{
		base.ServerInit();
		CreateMapMarker();
		Effect.server.Run(launchBlastFXPrefab.resourcePath, PivotPoint(), base.transform.up, null, true);
	}

	public override void ProjectileImpact(RaycastHit info, Vector3 rayOrigin)
	{
		Explode(rayOrigin);
		if (Physics.Raycast(info.point + Vector3.up, Vector3.down, 4f, 1218511121, QueryTriggerInteraction.Ignore))
		{
			Effect.server.Run(explosionGroundFXPrefab.resourcePath, info.point, Vector3.up, null, true);
		}
	}

	private void CreateMapMarker()
	{
		BaseEntity baseEntity = mapMarkerInstanceRef.Get(base.isServer);
		if (BaseEntityEx.IsValid(baseEntity))
		{
			baseEntity.Kill();
		}
		BaseEntity baseEntity2 = GameManager.server.CreateEntity(mapMarkerPrefab?.resourcePath, base.transform.position, Quaternion.identity);
		baseEntity2.OwnerID = base.OwnerID;
		baseEntity2.Spawn();
		baseEntity2.SetParent(this, true);
		mapMarkerInstanceRef.Set(baseEntity2);
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		return true;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return serverProjectile.CurrentVelocity;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!ColliderEx.IsOnLayer(other, Layer.Trigger))
		{
			return;
		}
		if (other.CompareTag("MLRSRocketTrigger"))
		{
			Explode();
			TimedExplosive componentInParent = other.GetComponentInParent<TimedExplosive>();
			if (componentInParent != null)
			{
				componentInParent.Explode();
			}
		}
		else if (other.GetComponent<TriggerSafeZone>() != null)
		{
			Kill();
		}
	}
}
