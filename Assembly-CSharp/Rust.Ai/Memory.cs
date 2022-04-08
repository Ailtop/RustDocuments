using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai;

public class Memory
{
	public struct SeenInfo
	{
		public BaseEntity Entity;

		public Vector3 Position;

		public float Timestamp;

		public float Danger;
	}

	public struct ExtendedInfo
	{
		public BaseEntity Entity;

		public Vector3 Direction;

		public float Dot;

		public float DistanceSqr;

		public byte LineOfSight;

		public float LastHurtUsTime;
	}

	public List<BaseEntity> Visible = new List<BaseEntity>();

	public List<SeenInfo> All = new List<SeenInfo>();

	public List<ExtendedInfo> AllExtended = new List<ExtendedInfo>();

	public SeenInfo Update(BaseEntity entity, float score, Vector3 direction, float dot, float distanceSqr, byte lineOfSight, bool updateLastHurtUsTime, float lastHurtUsTime, out ExtendedInfo extendedInfo)
	{
		return Update(entity, entity.ServerPosition, score, direction, dot, distanceSqr, lineOfSight, updateLastHurtUsTime, lastHurtUsTime, out extendedInfo);
	}

	public SeenInfo Update(BaseEntity entity, Vector3 position, float score, Vector3 direction, float dot, float distanceSqr, byte lineOfSight, bool updateLastHurtUsTime, float lastHurtUsTime, out ExtendedInfo extendedInfo)
	{
		extendedInfo = default(ExtendedInfo);
		bool flag = false;
		for (int i = 0; i < AllExtended.Count; i++)
		{
			if (AllExtended[i].Entity == entity)
			{
				ExtendedInfo extendedInfo2 = AllExtended[i];
				extendedInfo2.Direction = direction;
				extendedInfo2.Dot = dot;
				extendedInfo2.DistanceSqr = distanceSqr;
				extendedInfo2.LineOfSight = lineOfSight;
				if (updateLastHurtUsTime)
				{
					extendedInfo2.LastHurtUsTime = lastHurtUsTime;
				}
				AllExtended[i] = extendedInfo2;
				extendedInfo = extendedInfo2;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			if (updateLastHurtUsTime)
			{
				ExtendedInfo extendedInfo3 = default(ExtendedInfo);
				extendedInfo3.Entity = entity;
				extendedInfo3.Direction = direction;
				extendedInfo3.Dot = dot;
				extendedInfo3.DistanceSqr = distanceSqr;
				extendedInfo3.LineOfSight = lineOfSight;
				extendedInfo3.LastHurtUsTime = lastHurtUsTime;
				ExtendedInfo extendedInfo4 = extendedInfo3;
				AllExtended.Add(extendedInfo4);
				extendedInfo = extendedInfo4;
			}
			else
			{
				ExtendedInfo extendedInfo3 = default(ExtendedInfo);
				extendedInfo3.Entity = entity;
				extendedInfo3.Direction = direction;
				extendedInfo3.Dot = dot;
				extendedInfo3.DistanceSqr = distanceSqr;
				extendedInfo3.LineOfSight = lineOfSight;
				ExtendedInfo extendedInfo5 = extendedInfo3;
				AllExtended.Add(extendedInfo5);
				extendedInfo = extendedInfo5;
			}
		}
		return Update(entity, position, score);
	}

	public SeenInfo Update(BaseEntity ent, float danger = 0f)
	{
		return Update(ent, ent.ServerPosition, danger);
	}

	public SeenInfo Update(BaseEntity ent, Vector3 position, float danger = 0f)
	{
		for (int i = 0; i < All.Count; i++)
		{
			if (All[i].Entity == ent)
			{
				SeenInfo seenInfo = All[i];
				seenInfo.Position = position;
				seenInfo.Timestamp = Time.realtimeSinceStartup;
				seenInfo.Danger += danger;
				All[i] = seenInfo;
				return seenInfo;
			}
		}
		SeenInfo seenInfo2 = default(SeenInfo);
		seenInfo2.Entity = ent;
		seenInfo2.Position = position;
		seenInfo2.Timestamp = Time.realtimeSinceStartup;
		seenInfo2.Danger = danger;
		SeenInfo seenInfo3 = seenInfo2;
		All.Add(seenInfo3);
		Visible.Add(ent);
		return seenInfo3;
	}

	public void AddDanger(Vector3 position, float amount)
	{
		for (int i = 0; i < All.Count; i++)
		{
			if (Mathf.Approximately(All[i].Position.x, position.x) && Mathf.Approximately(All[i].Position.y, position.y) && Mathf.Approximately(All[i].Position.z, position.z))
			{
				SeenInfo value = All[i];
				value.Danger = amount;
				All[i] = value;
				return;
			}
		}
		All.Add(new SeenInfo
		{
			Position = position,
			Timestamp = Time.realtimeSinceStartup,
			Danger = amount
		});
	}

	public SeenInfo GetInfo(BaseEntity entity)
	{
		foreach (SeenInfo item in All)
		{
			if (item.Entity == entity)
			{
				return item;
			}
		}
		return default(SeenInfo);
	}

	public SeenInfo GetInfo(Vector3 position)
	{
		foreach (SeenInfo item in All)
		{
			if ((item.Position - position).sqrMagnitude < 1f)
			{
				return item;
			}
		}
		return default(SeenInfo);
	}

	public ExtendedInfo GetExtendedInfo(BaseEntity entity)
	{
		foreach (ExtendedInfo item in AllExtended)
		{
			if (item.Entity == entity)
			{
				return item;
			}
		}
		return default(ExtendedInfo);
	}

	internal void Forget(float maxSecondsOld)
	{
		for (int i = 0; i < All.Count; i++)
		{
			float num = Time.realtimeSinceStartup - All[i].Timestamp;
			if (num > maxSecondsOld)
			{
				if (All[i].Entity != null)
				{
					Visible.Remove(All[i].Entity);
					for (int j = 0; j < AllExtended.Count; j++)
					{
						if (AllExtended[j].Entity == All[i].Entity)
						{
							AllExtended.RemoveAt(j);
							break;
						}
					}
				}
				All.RemoveAt(i);
				i--;
			}
			else
			{
				if (!(num > 0f))
				{
					continue;
				}
				float num2 = num / maxSecondsOld;
				if (All[i].Danger > 0f)
				{
					SeenInfo value = All[i];
					value.Danger -= num2;
					All[i] = value;
				}
				if (!(num >= 1f))
				{
					continue;
				}
				for (int k = 0; k < AllExtended.Count; k++)
				{
					if (AllExtended[k].Entity == All[i].Entity)
					{
						ExtendedInfo value2 = AllExtended[k];
						value2.LineOfSight = 0;
						AllExtended[k] = value2;
						break;
					}
				}
			}
		}
		for (int l = 0; l < Visible.Count; l++)
		{
			if (Visible[l] == null)
			{
				Visible.RemoveAt(l);
				l--;
			}
		}
		for (int m = 0; m < AllExtended.Count; m++)
		{
			if (AllExtended[m].Entity == null)
			{
				AllExtended.RemoveAt(m);
				m--;
			}
		}
	}
}
