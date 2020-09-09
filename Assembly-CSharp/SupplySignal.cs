using System;
using UnityEngine;

public class SupplySignal : TimedExplosive
{
	public GameObjectRef smokeEffectPrefab;

	public GameObjectRef EntityToCreate;

	[NonSerialized]
	public GameObject smokeEffect;

	public override void Explode()
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(EntityToCreate.resourcePath);
		if ((bool)baseEntity)
		{
			Vector3 b = new Vector3(UnityEngine.Random.Range(-20f, 20f), 0f, UnityEngine.Random.Range(-20f, 20f));
			baseEntity.SendMessage("InitDropPosition", base.transform.position + b, SendMessageOptions.DontRequireReceiver);
			baseEntity.Spawn();
		}
		Invoke(FinishUp, 210f);
		SetFlag(Flags.On, true);
		SendNetworkUpdateImmediate();
	}

	public void FinishUp()
	{
		Kill();
	}
}
