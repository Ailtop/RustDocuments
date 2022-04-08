using System.Collections.Generic;
using System.Linq;
using ConVar;
using UnityEngine;

public class CombatLog
{
	public struct Event
	{
		public float time;

		public uint attacker_id;

		public uint target_id;

		public string attacker;

		public string target;

		public string weapon;

		public string ammo;

		public string bone;

		public HitArea area;

		public float distance;

		public float health_old;

		public float health_new;

		public string info;

		public int proj_hits;

		public float proj_integrity;

		public float proj_travel;

		public float proj_mismatch;
	}

	private const string selfname = "you";

	private const string noname = "N/A";

	private BasePlayer player;

	private Queue<Event> storage;

	private static Dictionary<ulong, Queue<Event>> players = new Dictionary<ulong, Queue<Event>>();

	public float LastActive { get; private set; }

	public CombatLog(BasePlayer player)
	{
		this.player = player;
	}

	public void Init()
	{
		storage = Get(player.userID);
		LastActive = storage.LastOrDefault().time;
	}

	public void Save()
	{
	}

	public void Log(AttackEntity weapon, string description = null)
	{
		Log(weapon, null, description);
	}

	public void Log(AttackEntity weapon, Projectile projectile, string description = null)
	{
		Event val = default(Event);
		val.time = UnityEngine.Time.realtimeSinceStartup;
		val.attacker_id = (((bool)player && player.net != null) ? player.net.ID : 0u);
		val.target_id = 0u;
		val.attacker = "you";
		val.target = "N/A";
		val.weapon = (weapon ? weapon.name : "N/A");
		val.ammo = (projectile ? projectile.name : "N/A");
		val.bone = "N/A";
		val.area = (HitArea)0;
		val.distance = 0f;
		val.health_old = 0f;
		val.health_new = 0f;
		val.info = ((description != null) ? description : string.Empty);
		Log(val);
	}

	public void Log(HitInfo info, string description = null)
	{
		float num = (info.HitEntity ? info.HitEntity.Health() : 0f);
		Log(info, num, num, description);
	}

	public void Log(HitInfo info, float health_old, float health_new, string description = null)
	{
		Event val = default(Event);
		val.time = UnityEngine.Time.realtimeSinceStartup;
		val.attacker_id = (((bool)info.Initiator && info.Initiator.net != null) ? info.Initiator.net.ID : 0u);
		val.target_id = (((bool)info.HitEntity && info.HitEntity.net != null) ? info.HitEntity.net.ID : 0u);
		val.attacker = ((player == info.Initiator) ? "you" : (info.Initiator ? info.Initiator.ShortPrefabName : "N/A"));
		val.target = ((player == info.HitEntity) ? "you" : (info.HitEntity ? info.HitEntity.ShortPrefabName : "N/A"));
		val.weapon = (info.WeaponPrefab ? info.WeaponPrefab.name : "N/A");
		val.ammo = (info.ProjectilePrefab ? info.ProjectilePrefab.name : "N/A");
		val.bone = info.boneName;
		val.area = info.boneArea;
		val.distance = (info.IsProjectile() ? info.ProjectileDistance : Vector3.Distance(info.PointStart, info.HitPositionWorld));
		val.health_old = health_old;
		val.health_new = health_new;
		val.info = ((description != null) ? description : string.Empty);
		val.proj_hits = info.ProjectileHits;
		val.proj_integrity = info.ProjectileIntegrity;
		val.proj_travel = info.ProjectileTravelTime;
		val.proj_mismatch = info.ProjectileTrajectoryMismatch;
		Log(val);
	}

	public void Log(Event val)
	{
		LastActive = UnityEngine.Time.realtimeSinceStartup;
		if (storage != null)
		{
			storage.Enqueue(val);
			int num = Mathf.Max(0, Server.combatlogsize);
			while (storage.Count > num)
			{
				storage.Dequeue();
			}
		}
	}

	public string Get(int count, uint filterByAttacker = 0u)
	{
		if (storage == null)
		{
			return string.Empty;
		}
		if (storage.Count == 0)
		{
			return "Combat log empty.";
		}
		TextTable textTable = new TextTable();
		textTable.AddColumn("time");
		textTable.AddColumn("attacker");
		textTable.AddColumn("id");
		textTable.AddColumn("target");
		textTable.AddColumn("id");
		textTable.AddColumn("weapon");
		textTable.AddColumn("ammo");
		textTable.AddColumn("area");
		textTable.AddColumn("distance");
		textTable.AddColumn("old_hp");
		textTable.AddColumn("new_hp");
		textTable.AddColumn("info");
		textTable.AddColumn("hits");
		textTable.AddColumn("integrity");
		textTable.AddColumn("travel");
		textTable.AddColumn("mismatch");
		int num = storage.Count - count;
		int combatlogdelay = Server.combatlogdelay;
		int num2 = 0;
		foreach (Event item in storage)
		{
			if (num > 0)
			{
				num--;
			}
			else if (filterByAttacker == 0 || item.attacker_id == filterByAttacker)
			{
				float num3 = UnityEngine.Time.realtimeSinceStartup - item.time;
				if (num3 >= (float)combatlogdelay)
				{
					string text = num3.ToString("0.00s");
					string attacker = item.attacker;
					uint attacker_id = item.attacker_id;
					string text2 = attacker_id.ToString();
					string target = item.target;
					attacker_id = item.target_id;
					string text3 = attacker_id.ToString();
					string weapon = item.weapon;
					string ammo = item.ammo;
					string text4 = HitAreaUtil.Format(item.area).ToLower();
					float distance = item.distance;
					string text5 = distance.ToString("0.0m");
					distance = item.health_old;
					string text6 = distance.ToString("0.0");
					distance = item.health_new;
					string text7 = distance.ToString("0.0");
					string info = item.info;
					int proj_hits = item.proj_hits;
					string text8 = proj_hits.ToString();
					distance = item.proj_integrity;
					string text9 = distance.ToString("0.00");
					distance = item.proj_travel;
					string text10 = distance.ToString("0.00s");
					distance = item.proj_mismatch;
					string text11 = distance.ToString("0.00m");
					textTable.AddRow(text, attacker, text2, target, text3, weapon, ammo, text4, text5, text6, text7, info, text8, text9, text10, text11);
				}
				else
				{
					num2++;
				}
			}
		}
		string text12 = textTable.ToString();
		if (num2 > 0)
		{
			text12 = text12 + "+ " + num2 + " " + ((num2 > 1) ? "events" : "event");
			text12 = text12 + " in the last " + combatlogdelay + " " + ((combatlogdelay > 1) ? "seconds" : "second");
		}
		return text12;
	}

	public static Queue<Event> Get(ulong id)
	{
		if (players.TryGetValue(id, out var value))
		{
			return value;
		}
		value = new Queue<Event>();
		players.Add(id, value);
		return value;
	}
}
