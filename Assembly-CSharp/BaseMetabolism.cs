using ConVar;
using Rust;
using System;
using UnityEngine;

public static class BaseMetabolism
{
	public const float targetHeartRate = 0.05f;
}
public abstract class BaseMetabolism<T> : EntityComponent<T> where T : BaseCombatEntity
{
	protected T owner;

	public MetabolismAttribute calories = new MetabolismAttribute();

	public MetabolismAttribute hydration = new MetabolismAttribute();

	public MetabolismAttribute heartrate = new MetabolismAttribute();

	protected float timeSinceLastMetabolism;

	public virtual void Reset()
	{
		calories.Reset();
		hydration.Reset();
		heartrate.Reset();
	}

	protected virtual void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			owner = null;
		}
	}

	public virtual void ServerInit(T owner)
	{
		Reset();
		this.owner = owner;
	}

	public virtual void ServerUpdate(BaseCombatEntity ownerEntity, float delta)
	{
		timeSinceLastMetabolism += delta;
		if (!(timeSinceLastMetabolism <= ConVar.Server.metabolismtick))
		{
			if ((bool)(UnityEngine.Object)owner && !owner.IsDead())
			{
				RunMetabolism(ownerEntity, timeSinceLastMetabolism);
				DoMetabolismDamage(ownerEntity, timeSinceLastMetabolism);
			}
			timeSinceLastMetabolism = 0f;
		}
	}

	protected virtual void DoMetabolismDamage(BaseCombatEntity ownerEntity, float delta)
	{
		if (calories.value <= 20f)
		{
			using (TimeWarning.New("Calories Hurt"))
			{
				ownerEntity.Hurt(Mathf.InverseLerp(20f, 0f, calories.value) * delta * 0.0833333358f, DamageType.Hunger);
			}
		}
		if (hydration.value <= 20f)
		{
			using (TimeWarning.New("Hyration Hurt"))
			{
				ownerEntity.Hurt(Mathf.InverseLerp(20f, 0f, hydration.value) * delta * (142f / (339f * (float)Math.PI)), DamageType.Thirst);
			}
		}
	}

	protected virtual void RunMetabolism(BaseCombatEntity ownerEntity, float delta)
	{
		if (calories.value > 200f)
		{
			ownerEntity.Heal(Mathf.InverseLerp(200f, 1000f, calories.value) * delta * 0.0166666675f);
		}
		if (hydration.value > 200f)
		{
			ownerEntity.Heal(Mathf.InverseLerp(200f, 1000f, hydration.value) * delta * 0.0166666675f);
		}
		hydration.MoveTowards(0f, delta * 0.008333334f);
		calories.MoveTowards(0f, delta * 0.0166666675f);
		heartrate.MoveTowards(0.05f, delta * 0.0166666675f);
	}

	public void ApplyChange(MetabolismAttribute.Type type, float amount, float time)
	{
		FindAttribute(type)?.Add(amount);
	}

	public bool ShouldDie()
	{
		if ((bool)(UnityEngine.Object)owner)
		{
			return owner.Health() <= 0f;
		}
		return false;
	}

	public virtual MetabolismAttribute FindAttribute(MetabolismAttribute.Type type)
	{
		switch (type)
		{
		case MetabolismAttribute.Type.Calories:
			return calories;
		case MetabolismAttribute.Type.Hydration:
			return hydration;
		case MetabolismAttribute.Type.Heartrate:
			return heartrate;
		default:
			return null;
		}
	}
}
