using Oxide.Core;
using UnityEngine;

public class FlameExplosive : TimedExplosive
{
	public GameObjectRef createOnExplode;

	public float numToCreate = 10f;

	public float minVelocity = 2f;

	public float maxVelocity = 5f;

	public float spreadAngle = 90f;

	public bool forceUpForExplosion;

	public AnimationCurve velocityCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve spreadCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public override void Explode()
	{
		FlameExplode(forceUpForExplosion ? Vector3.up : (-base.transform.forward));
	}

	public void FlameExplode(Vector3 surfaceNormal)
	{
		if (!base.isServer)
		{
			return;
		}
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		for (int i = 0; (float)i < numToCreate; i++)
		{
			Vector3 position = base.transform.position;
			BaseEntity baseEntity = GameManager.server.CreateEntity(createOnExplode.resourcePath, position);
			if ((bool)baseEntity)
			{
				float num = (float)i / numToCreate;
				Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(spreadAngle * spreadCurve.Evaluate(num), surfaceNormal);
				baseEntity.transform.SetPositionAndRotation(position, Quaternion.LookRotation(modifiedAimConeDirection));
				baseEntity.creatorEntity = ((creatorEntity == null) ? baseEntity : creatorEntity);
				baseEntity.Spawn();
				Interface.CallHook("OnFlameExplosion", this, i);
				Vector3 vector = modifiedAimConeDirection.normalized * UnityEngine.Random.Range(minVelocity, maxVelocity) * velocityCurve.Evaluate(num * UnityEngine.Random.Range(1f, 1.1f));
				FireBall component2 = baseEntity.GetComponent<FireBall>();
				if (component2 != null)
				{
					component2.SetDelayedVelocity(vector);
				}
				else
				{
					baseEntity.SetVelocity(vector);
				}
			}
		}
		base.Explode();
	}

	public override void ProjectileImpact(RaycastHit info, Vector3 rayOrigin)
	{
		FlameExplode(info.normal);
	}
}
