using ConVar;
using Rust;
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
			if ((bool)(Object)owner && !owner.IsDead())
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
				ownerEntity.Hurt(Mathf.InverseLerp(20f, 0f, calories.value) * delta * (1f / 12f), DamageType.Hunger);
			}
		}
		if (hydration.value <= 20f)
		{
			using (TimeWarning.New("Hyration Hurt"))
			{
				ownerEntity.Hurt(Mathf.InverseLerp(20f, 0f, hydration.value) * delta * (2f / 15f), DamageType.Thirst);
			}
		}
	}

	protected virtual void RunMetabolism(BaseCombatEntity ownerEntity, float delta)
	{
		if (calories.value > 200f)
		{
			ownerEntity.Heal(Mathf.InverseLerp(200f, 1000f, calories.value) * delta * (1f / 60f));
		}
		if (hydration.value > 200f)
		{
			ownerEntity.Heal(Mathf.InverseLerp(200f, 1000f, hydration.value) * delta * (1f / 60f));
		}
		hydration.MoveTowards(0f, delta * (1f / 120f));
		calories.MoveTowards(0f, delta * (1f / 60f));
		heartrate.MoveTowards(0.05f, delta * (1f / 60f));
	}

	public void ApplyChange(MetabolismAttribute.Type type, float amount, float time)
	{
		FindAttribute(type)?.Add(amount);
	}

	public bool ShouldDie()
	{
		if ((bool)(Object)owner)
		{
			return owner.Health() <= 0f;
		}
		return false;
	}

	public virtual MetabolismAttribute FindAttribute(MetabolismAttribute.Type type)
	{
		return type switch
		{
			MetabolismAttribute.Type.Calories => calories, 
			MetabolismAttribute.Type.Hydration => hydration, 
			MetabolismAttribute.Type.Heartrate => heartrate, 
			_ => null, 
		};
	}
}
