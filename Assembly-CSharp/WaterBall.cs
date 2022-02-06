using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class WaterBall : BaseEntity
{
	public ItemDefinition liquidType;

	public int waterAmount;

	public GameObjectRef waterExplosion;

	public Rigidbody myRigidBody;

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(Extinguish, 10f);
	}

	public void Extinguish()
	{
		CancelInvoke(Extinguish);
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public void FixedUpdate()
	{
		if (base.isServer)
		{
			GetComponent<Rigidbody>().AddForce(Physics.gravity, ForceMode.Acceleration);
		}
	}

	public static bool DoSplash(Vector3 position, float radius, ItemDefinition liquidDef, int amount)
	{
		List<BaseEntity> obj = Pool.GetList<BaseEntity>();
		Vis.Entities(position, radius, obj, 1219701523);
		int num = 0;
		int num2 = amount;
		while (amount > 0 && num < 3)
		{
			List<ISplashable> obj2 = Pool.GetList<ISplashable>();
			foreach (BaseEntity item in obj)
			{
				if (!item.isClient)
				{
					ISplashable splashable = item as ISplashable;
					if (splashable != null && !obj2.Contains(splashable) && splashable.WantsSplash(liquidDef, amount))
					{
						obj2.Add(splashable);
					}
				}
			}
			if (obj2.Count == 0)
			{
				break;
			}
			int b = Mathf.CeilToInt(amount / obj2.Count);
			foreach (ISplashable item2 in obj2)
			{
				int num3 = item2.DoSplash(liquidDef, Mathf.Min(amount, b));
				amount -= num3;
				if (amount <= 0)
				{
					break;
				}
			}
			Pool.FreeList(ref obj2);
			num++;
		}
		Pool.FreeList(ref obj);
		return amount < num2;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!base.isClient && !myRigidBody.isKinematic)
		{
			float num = 2.5f;
			DoSplash(base.transform.position + new Vector3(0f, num * 0.75f, 0f), num, liquidType, waterAmount);
			Effect.server.Run(waterExplosion.resourcePath, base.transform.position + new Vector3(0f, 0f, 0f), Vector3.up);
			myRigidBody.isKinematic = true;
			Invoke(Extinguish, 2f);
		}
	}
}
