using UnityEngine;

public class TriggerWakeAIZ : TriggerBase, IServerComponent
{
	public float SleepDelaySeconds = 30f;

	private AIInformationZone aiz;

	private void Awake()
	{
		Transform parent = base.transform.parent;
		if (parent == null)
		{
			parent = base.transform;
		}
		aiz = parent.GetComponentInChildren<AIInformationZone>();
		if (aiz != null)
		{
			aiz.SleepAI();
		}
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		BasePlayer basePlayer = baseEntity as BasePlayer;
		if (basePlayer != null && basePlayer.IsNpc)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (!(aiz == null))
		{
			CancelInvoke(SleepAI);
			aiz.WakeAI();
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (!(aiz == null) && (entityContents == null || entityContents.Count == 0))
		{
			DelayedSleepAI();
		}
	}

	private void DelayedSleepAI()
	{
		CancelInvoke(SleepAI);
		Invoke(SleepAI, SleepDelaySeconds);
	}

	private void SleepAI()
	{
		if (!(aiz == null))
		{
			aiz.SleepAI();
		}
	}
}
