using System.Collections.Generic;
using System.Linq;
using Rust;
using UnityEngine;

public class TriggerHurtEx : TriggerBase, IServerComponent, IHurtTrigger
{
	public enum HurtType
	{
		Simple,
		IncludeBleedingAndScreenShake
	}

	public class EntityTriggerInfo
	{
		public Vector3 lastPosition;
	}

	public float repeatRate = 0.1f;

	[Header("On Enter")]
	public List<DamageTypeEntry> damageOnEnter;

	public GameObjectRef effectOnEnter;

	public HurtType hurtTypeOnEnter;

	[Header("On Timer (damage per second)")]
	public List<DamageTypeEntry> damageOnTimer;

	public GameObjectRef effectOnTimer;

	public HurtType hurtTypeOnTimer;

	[Header("On Move (damage per meter)")]
	public List<DamageTypeEntry> damageOnMove;

	public GameObjectRef effectOnMove;

	public HurtType hurtTypeOnMove;

	[Header("On Leave")]
	public List<DamageTypeEntry> damageOnLeave;

	public GameObjectRef effectOnLeave;

	public HurtType hurtTypeOnLeave;

	public bool damageEnabled = true;

	internal Dictionary<BaseEntity, EntityTriggerInfo> entityInfo;

	internal List<BaseEntity> entityAddList;

	internal List<BaseEntity> entityLeaveList;

	public override GameObject InterestedInObject(GameObject obj)
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
		if (!(baseEntity is BaseCombatEntity))
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	internal void DoDamage(BaseEntity ent, HurtType type, List<DamageTypeEntry> damage, GameObjectRef effect, float multiply = 1f)
	{
		if (!damageEnabled)
		{
			return;
		}
		using (TimeWarning.New("TriggerHurtEx.DoDamage"))
		{
			if (damage != null && damage.Count > 0)
			{
				BaseCombatEntity baseCombatEntity = ent as BaseCombatEntity;
				if ((bool)baseCombatEntity)
				{
					HitInfo hitInfo = new HitInfo();
					hitInfo.damageTypes.Add(damage);
					hitInfo.damageTypes.ScaleAll(multiply);
					hitInfo.DoHitEffects = true;
					hitInfo.DidHit = true;
					hitInfo.Initiator = GameObjectEx.ToBaseEntity(base.gameObject);
					hitInfo.PointStart = base.transform.position;
					hitInfo.PointEnd = baseCombatEntity.transform.position;
					if (type == HurtType.Simple)
					{
						baseCombatEntity.Hurt(hitInfo);
					}
					else
					{
						baseCombatEntity.OnAttacked(hitInfo);
					}
				}
			}
			if (effect.isValid)
			{
				Effect.server.Run(effect.resourcePath, ent, StringPool.closest, base.transform.position, Vector3.up);
			}
		}
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (!(ent == null))
		{
			if (entityAddList == null)
			{
				entityAddList = new List<BaseEntity>();
			}
			entityAddList.Add(ent);
			Invoke(ProcessQueues, 0.1f);
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (!(ent == null))
		{
			if (entityLeaveList == null)
			{
				entityLeaveList = new List<BaseEntity>();
			}
			entityLeaveList.Add(ent);
			Invoke(ProcessQueues, 0.1f);
		}
	}

	public override void OnObjects()
	{
		InvokeRepeating(OnTick, repeatRate, repeatRate);
	}

	public override void OnEmpty()
	{
		CancelInvoke(OnTick);
	}

	private void OnTick()
	{
		ProcessQueues();
		if (entityInfo == null)
		{
			return;
		}
		KeyValuePair<BaseEntity, EntityTriggerInfo>[] array = entityInfo.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<BaseEntity, EntityTriggerInfo> keyValuePair = array[i];
			if (BaseEntityEx.IsValid(keyValuePair.Key))
			{
				Vector3 position = keyValuePair.Key.transform.position;
				float magnitude = (position - keyValuePair.Value.lastPosition).magnitude;
				if (magnitude > 0.01f)
				{
					keyValuePair.Value.lastPosition = position;
					DoDamage(keyValuePair.Key, hurtTypeOnMove, damageOnMove, effectOnMove, magnitude);
				}
				DoDamage(keyValuePair.Key, hurtTypeOnTimer, damageOnTimer, effectOnTimer, repeatRate);
			}
		}
	}

	private void ProcessQueues()
	{
		if (entityAddList != null)
		{
			foreach (BaseEntity entityAdd in entityAddList)
			{
				if (BaseEntityEx.IsValid(entityAdd))
				{
					DoDamage(entityAdd, hurtTypeOnEnter, damageOnEnter, effectOnEnter);
					if (entityInfo == null)
					{
						entityInfo = new Dictionary<BaseEntity, EntityTriggerInfo>();
					}
					if (!entityInfo.ContainsKey(entityAdd))
					{
						entityInfo.Add(entityAdd, new EntityTriggerInfo
						{
							lastPosition = entityAdd.transform.position
						});
					}
				}
			}
			entityAddList = null;
		}
		if (entityLeaveList == null)
		{
			return;
		}
		foreach (BaseEntity entityLeave in entityLeaveList)
		{
			if (!BaseEntityEx.IsValid(entityLeave))
			{
				continue;
			}
			DoDamage(entityLeave, hurtTypeOnLeave, damageOnLeave, effectOnLeave);
			if (entityInfo != null)
			{
				entityInfo.Remove(entityLeave);
				if (entityInfo.Count == 0)
				{
					entityInfo = null;
				}
			}
		}
		entityLeaveList.Clear();
	}
}
