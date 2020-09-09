using ConVar;
using Facepunch;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decay : PrefabAttribute, IServerComponent
{
	private const float hours = 3600f;

	protected float GetDecayDelay(BuildingGrade.Enum grade)
	{
		if (ConVar.Decay.upkeep)
		{
			if (ConVar.Decay.delay_override > 0f)
			{
				return ConVar.Decay.delay_override;
			}
			switch (grade)
			{
			default:
				return ConVar.Decay.delay_twig * 3600f;
			case BuildingGrade.Enum.Wood:
				return ConVar.Decay.delay_wood * 3600f;
			case BuildingGrade.Enum.Stone:
				return ConVar.Decay.delay_stone * 3600f;
			case BuildingGrade.Enum.Metal:
				return ConVar.Decay.delay_metal * 3600f;
			case BuildingGrade.Enum.TopTier:
				return ConVar.Decay.delay_toptier * 3600f;
			}
		}
		switch (grade)
		{
		default:
			return 3600f;
		case BuildingGrade.Enum.Wood:
			return 64800f;
		case BuildingGrade.Enum.Stone:
			return 64800f;
		case BuildingGrade.Enum.Metal:
			return 64800f;
		case BuildingGrade.Enum.TopTier:
			return 86400f;
		}
	}

	protected float GetDecayDuration(BuildingGrade.Enum grade)
	{
		if (ConVar.Decay.upkeep)
		{
			if (ConVar.Decay.duration_override > 0f)
			{
				return ConVar.Decay.duration_override;
			}
			switch (grade)
			{
			default:
				return ConVar.Decay.duration_twig * 3600f;
			case BuildingGrade.Enum.Wood:
				return ConVar.Decay.duration_wood * 3600f;
			case BuildingGrade.Enum.Stone:
				return ConVar.Decay.duration_stone * 3600f;
			case BuildingGrade.Enum.Metal:
				return ConVar.Decay.duration_metal * 3600f;
			case BuildingGrade.Enum.TopTier:
				return ConVar.Decay.duration_toptier * 3600f;
			}
		}
		switch (grade)
		{
		default:
			return 3600f;
		case BuildingGrade.Enum.Wood:
			return 86400f;
		case BuildingGrade.Enum.Stone:
			return 172800f;
		case BuildingGrade.Enum.Metal:
			return 259200f;
		case BuildingGrade.Enum.TopTier:
			return 432000f;
		}
	}

	public static void BuildingDecayTouch(BuildingBlock buildingBlock)
	{
		if (ConVar.Decay.upkeep)
		{
			return;
		}
		List<DecayEntity> obj = Facepunch.Pool.GetList<DecayEntity>();
		Vis.Entities(buildingBlock.transform.position, 40f, obj, 2097408);
		for (int i = 0; i < obj.Count; i++)
		{
			DecayEntity decayEntity = obj[i];
			BuildingBlock buildingBlock2 = decayEntity as BuildingBlock;
			if (!buildingBlock2 || buildingBlock2.buildingID == buildingBlock.buildingID)
			{
				decayEntity.DecayTouch();
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public static void EntityLinkDecayTouch(BaseEntity ent)
	{
		if (!ConVar.Decay.upkeep)
		{
			ent.EntityLinkBroadcast(delegate(DecayEntity decayEnt)
			{
				decayEnt.DecayTouch();
			});
		}
	}

	public static void RadialDecayTouch(Vector3 pos, float radius, int mask)
	{
		if (!ConVar.Decay.upkeep)
		{
			List<DecayEntity> obj = Facepunch.Pool.GetList<DecayEntity>();
			Vis.Entities(pos, radius, obj, mask);
			for (int i = 0; i < obj.Count; i++)
			{
				obj[i].DecayTouch();
			}
			Facepunch.Pool.FreeList(ref obj);
		}
	}

	public virtual bool ShouldDecay(BaseEntity entity)
	{
		return true;
	}

	public abstract float GetDecayDelay(BaseEntity entity);

	public abstract float GetDecayDuration(BaseEntity entity);

	protected override Type GetIndexedType()
	{
		return typeof(Decay);
	}
}
