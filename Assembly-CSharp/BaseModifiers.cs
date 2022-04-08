using System.Collections.Generic;
using ConVar;
using Rust;
using UnityEngine;

public static class BaseModifiers
{
}
public abstract class BaseModifiers<T> : EntityComponent<T> where T : BaseCombatEntity
{
	public List<Modifier> All = new List<Modifier>();

	protected Dictionary<Modifier.ModifierType, float> totalValues = new Dictionary<Modifier.ModifierType, float>();

	protected Dictionary<Modifier.ModifierType, float> modifierVariables = new Dictionary<Modifier.ModifierType, float>();

	protected T owner;

	protected bool dirty = true;

	protected float timeSinceLastTick;

	protected float lastTickTime;

	public int ActiveModifierCoount => All.Count;

	public void Add(List<ModifierDefintion> modDefs)
	{
		foreach (ModifierDefintion modDef in modDefs)
		{
			Add(modDef);
		}
	}

	protected void Add(ModifierDefintion def)
	{
		Modifier modifier = new Modifier();
		modifier.Init(def.type, def.source, def.value, def.duration, def.duration);
		Add(modifier);
	}

	protected void Add(Modifier modifier)
	{
		if (!CanAdd(modifier))
		{
			return;
		}
		int maxModifiersForSourceType = GetMaxModifiersForSourceType(modifier.Source);
		if (GetTypeSourceCount(modifier.Type, modifier.Source) >= maxModifiersForSourceType)
		{
			Modifier shortestLifeModifier = GetShortestLifeModifier(modifier.Type, modifier.Source);
			if (shortestLifeModifier == null)
			{
				return;
			}
			Remove(shortestLifeModifier);
		}
		All.Add(modifier);
		if (!totalValues.ContainsKey(modifier.Type))
		{
			totalValues.Add(modifier.Type, modifier.Value);
		}
		else
		{
			totalValues[modifier.Type] += modifier.Value;
		}
		SetDirty(flag: true);
	}

	private bool CanAdd(Modifier modifier)
	{
		if (All.Contains(modifier))
		{
			return false;
		}
		return true;
	}

	private int GetMaxModifiersForSourceType(Modifier.ModifierSource source)
	{
		if (source == Modifier.ModifierSource.Tea)
		{
			return 1;
		}
		return int.MaxValue;
	}

	private int GetTypeSourceCount(Modifier.ModifierType type, Modifier.ModifierSource source)
	{
		int num = 0;
		foreach (Modifier item in All)
		{
			if (item.Type == type && item.Source == source)
			{
				num++;
			}
		}
		return num;
	}

	private Modifier GetShortestLifeModifier(Modifier.ModifierType type, Modifier.ModifierSource source)
	{
		Modifier modifier = null;
		foreach (Modifier item in All)
		{
			if (item.Type == type && item.Source == source)
			{
				if (modifier == null)
				{
					modifier = item;
				}
				else if (item.TimeRemaining < modifier.TimeRemaining)
				{
					modifier = item;
				}
			}
		}
		return modifier;
	}

	private void Remove(Modifier modifier)
	{
		if (All.Contains(modifier))
		{
			All.Remove(modifier);
			totalValues[modifier.Type] -= modifier.Value;
			SetDirty(flag: true);
		}
	}

	public void RemoveAll()
	{
		All.Clear();
		totalValues.Clear();
		SetDirty(flag: true);
	}

	public float GetValue(Modifier.ModifierType type, float defaultValue = 0f)
	{
		if (totalValues.TryGetValue(type, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public float GetVariableValue(Modifier.ModifierType type, float defaultValue)
	{
		if (modifierVariables.TryGetValue(type, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public void SetVariableValue(Modifier.ModifierType type, float value)
	{
		if (modifierVariables.TryGetValue(type, out var _))
		{
			modifierVariables[type] = value;
		}
		else
		{
			modifierVariables.Add(type, value);
		}
	}

	public void RemoveVariable(Modifier.ModifierType type)
	{
		modifierVariables.Remove(type);
	}

	protected virtual void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			owner = null;
		}
	}

	protected void SetDirty(bool flag)
	{
		dirty = flag;
	}

	public virtual void ServerInit(T owner)
	{
		this.owner = owner;
		ResetTicking();
		RemoveAll();
	}

	public void ResetTicking()
	{
		lastTickTime = UnityEngine.Time.realtimeSinceStartup;
		timeSinceLastTick = 0f;
	}

	public virtual void ServerUpdate(BaseCombatEntity ownerEntity)
	{
		float num = UnityEngine.Time.realtimeSinceStartup - lastTickTime;
		lastTickTime = UnityEngine.Time.realtimeSinceStartup;
		timeSinceLastTick += num;
		if (!(timeSinceLastTick <= ConVar.Server.modifierTickRate))
		{
			if ((Object)owner != (Object)null && !owner.IsDead())
			{
				TickModifiers(ownerEntity, timeSinceLastTick);
			}
			timeSinceLastTick = 0f;
		}
	}

	protected virtual void TickModifiers(BaseCombatEntity ownerEntity, float delta)
	{
		for (int num = All.Count - 1; num >= 0; num--)
		{
			Modifier modifier = All[num];
			modifier.Tick(ownerEntity, delta);
			if (modifier.Expired)
			{
				Remove(modifier);
			}
		}
	}
}
