using System.Collections.Generic;
using UnityEngine;

public class TriggerWakeAIZ : TriggerBase, IServerComponent
{
	public float SleepDelaySeconds = 30f;

	public List<AIInformationZone> zones = new List<AIInformationZone>();

	private AIInformationZone aiz;

	public void Init(AIInformationZone zone = null)
	{
		if (zone != null)
		{
			aiz = zone;
		}
		else if (zones == null || zones.Count == 0)
		{
			Transform parent = base.transform.parent;
			if (parent == null)
			{
				parent = base.transform;
			}
			aiz = parent.GetComponentInChildren<AIInformationZone>();
		}
		SetZonesSleeping(flag: true);
	}

	private void Awake()
	{
		Init();
	}

	private void SetZonesSleeping(bool flag)
	{
		if (aiz != null)
		{
			if (flag)
			{
				aiz.SleepAI();
			}
			else
			{
				aiz.WakeAI();
			}
		}
		if (zones == null || zones.Count <= 0)
		{
			return;
		}
		foreach (AIInformationZone zone in zones)
		{
			if (zone != null)
			{
				if (flag)
				{
					zone.SleepAI();
				}
				else
				{
					zone.WakeAI();
				}
			}
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
		if (!(aiz == null) || (zones != null && zones.Count != 0))
		{
			CancelInvoke(SleepAI);
			SetZonesSleeping(flag: false);
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if ((!(aiz == null) || (zones != null && zones.Count != 0)) && (entityContents == null || entityContents.Count == 0))
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
		SetZonesSleeping(flag: true);
	}
}
