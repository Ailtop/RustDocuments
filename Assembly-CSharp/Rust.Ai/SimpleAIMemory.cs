using System.Collections.Generic;
using ConVar;
using Oxide.Core;
using UnityEngine;

namespace Rust.AI;

public class SimpleAIMemory
{
	public struct SeenInfo
	{
		public BaseEntity Entity;

		public Vector3 Position;

		public float Timestamp;

		public float Danger;
	}

	public static HashSet<BasePlayer> PlayerIgnoreList = new HashSet<BasePlayer>();

	public List<SeenInfo> All = new List<SeenInfo>();

	public List<BaseEntity> Players = new List<BaseEntity>();

	public HashSet<BaseEntity> LOS = new HashSet<BaseEntity>();

	public List<BaseEntity> Targets = new List<BaseEntity>();

	public List<BaseEntity> Threats = new List<BaseEntity>();

	public List<BaseEntity> Friendlies = new List<BaseEntity>();

	public void SetKnown(BaseEntity ent, BaseEntity owner, AIBrainSenses brainSenses)
	{
		if (Interface.CallHook("OnNpcTargetSense", owner, ent, brainSenses) != null)
		{
			return;
		}
		IAISenses iAISenses = owner as IAISenses;
		BasePlayer basePlayer = ent as BasePlayer;
		if (basePlayer != null && PlayerIgnoreList.Contains(basePlayer))
		{
			return;
		}
		bool flag = false;
		if (iAISenses != null && iAISenses.IsThreat(ent))
		{
			flag = true;
			if (brainSenses != null)
			{
				brainSenses.LastThreatTimestamp = UnityEngine.Time.realtimeSinceStartup;
			}
		}
		for (int i = 0; i < All.Count; i++)
		{
			if (All[i].Entity == ent)
			{
				SeenInfo value = All[i];
				value.Position = ent.transform.position;
				value.Timestamp = Mathf.Max(UnityEngine.Time.realtimeSinceStartup, value.Timestamp);
				All[i] = value;
				return;
			}
		}
		if (basePlayer != null)
		{
			if (ConVar.AI.ignoreplayers && !basePlayer.IsNpc)
			{
				return;
			}
			Players.Add(ent);
		}
		if (iAISenses != null)
		{
			if (iAISenses.IsTarget(ent))
			{
				Targets.Add(ent);
			}
			if (iAISenses.IsFriendly(ent))
			{
				Friendlies.Add(ent);
			}
			if (flag)
			{
				Threats.Add(ent);
			}
		}
		All.Add(new SeenInfo
		{
			Entity = ent,
			Position = ent.transform.position,
			Timestamp = UnityEngine.Time.realtimeSinceStartup
		});
	}

	public void SetLOS(BaseEntity ent, bool flag)
	{
		if (!(ent == null))
		{
			if (flag)
			{
				LOS.Add(ent);
			}
			else
			{
				LOS.Remove(ent);
			}
		}
	}

	public bool IsLOS(BaseEntity ent)
	{
		return LOS.Contains(ent);
	}

	public bool IsPlayerKnown(BasePlayer player)
	{
		return Players.Contains(player);
	}

	public void Forget(float secondsOld)
	{
		for (int i = 0; i < All.Count; i++)
		{
			if (!(UnityEngine.Time.realtimeSinceStartup - All[i].Timestamp >= secondsOld))
			{
				continue;
			}
			BaseEntity entity = All[i].Entity;
			if (entity != null)
			{
				if (entity is BasePlayer)
				{
					Players.Remove(entity);
				}
				Targets.Remove(entity);
				Threats.Remove(entity);
				Friendlies.Remove(entity);
				LOS.Remove(entity);
			}
			All.RemoveAt(i);
			i--;
		}
	}

	public static void AddIgnorePlayer(BasePlayer player)
	{
		if (!PlayerIgnoreList.Contains(player))
		{
			PlayerIgnoreList.Add(player);
		}
	}

	public static void RemoveIgnorePlayer(BasePlayer player)
	{
		PlayerIgnoreList.Remove(player);
	}

	public static void ClearIgnoredPlayers()
	{
		PlayerIgnoreList.Clear();
	}

	public static string GetIgnoredPlayers()
	{
		TextTable textTable = new TextTable();
		textTable.AddColumns("Name", "Steam ID");
		foreach (BasePlayer playerIgnore in PlayerIgnoreList)
		{
			textTable.AddRow(playerIgnore.displayName, playerIgnore.userID.ToString());
		}
		return textTable.ToString();
	}
}
