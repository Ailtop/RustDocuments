using Rust;
using UnityEngine;

public class TriggerPlayerTimer : TriggerBase
{
	public BaseEntity TargetEntity;

	public float DamageAmount = 20f;

	public float TimeToDamage = 3f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj != null)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			BasePlayer basePlayer;
			if ((object)(basePlayer = (baseEntity as BasePlayer)) != null && baseEntity.isServer && !basePlayer.isMounted)
			{
				return baseEntity.gameObject;
			}
		}
		return obj;
	}

	internal override void OnObjects()
	{
		base.OnObjects();
		Invoke(DamageTarget, TimeToDamage);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvoke(DamageTarget);
	}

	private void DamageTarget()
	{
		bool flag = false;
		foreach (BaseEntity entityContent in entityContents)
		{
			BasePlayer basePlayer;
			if ((object)(basePlayer = (entityContent as BasePlayer)) != null && !basePlayer.isMounted)
			{
				flag = true;
			}
		}
		if (flag && TargetEntity != null)
		{
			TargetEntity.OnAttacked(new HitInfo(null, TargetEntity, DamageType.Generic, DamageAmount));
		}
		Invoke(DamageTarget, TimeToDamage);
	}
}
