using System;
using System.Collections.Generic;
using Rust.Ai.HTN.Sensors;
using UnityEngine;

namespace Rust.Ai.HTN.Bear.Sensors
{
	[Serializable]
	public class BearEnemyPlayersLineOfSightSensor : INpcSensor
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			TickStatic(npc);
		}

		public static void TickStatic(IHTNAgent npc)
		{
			npc.AiDomain.NpcContext.EnemyPlayersInLineOfSight.Clear();
			List<NpcPlayerInfo> enemyPlayersInRange = npc.AiDomain.NpcContext.EnemyPlayersInRange;
			for (int i = 0; i < enemyPlayersInRange.Count; i++)
			{
				NpcPlayerInfo info = enemyPlayersInRange[i];
				if (info.Player == null || info.Player.transform == null || info.Player.IsDestroyed || info.Player.IsDead())
				{
					enemyPlayersInRange.RemoveAt(i);
					i--;
				}
				else
				{
					TickLineOfSightTest(npc, ref info);
				}
			}
		}

		public static void TickLineOfSightTest(IHTNAgent npc, ref NpcPlayerInfo info)
		{
			BearDomain bearDomain = npc.AiDomain as BearDomain;
			if (bearDomain == null)
			{
				return;
			}
			bool isStanding = bearDomain.BearContext.IsFact(Facts.IsStandingUp);
			info.HeadVisible = false;
			info.BodyVisible = false;
			if (!(info.SqrDistance < bearDomain.BearDefinition.SqrAggroRange(isStanding)) || !(info.ForwardDotDir > npc.AiDefinition.Sensory.FieldOfViewRadians))
			{
				return;
			}
			float maxDistance = bearDomain.BearDefinition.AggroRange(isStanding);
			Ray ray = AimAtBody(npc, ref info);
			if (info.Player.IsVisible(ray, 1218519041, maxDistance))
			{
				info.BodyVisible = true;
				npc.AiDomain.NpcContext.EnemyPlayersInLineOfSight.Add(info);
				return;
			}
			ray = AimAtHead(npc, ref info);
			if (info.Player.IsVisible(ray, 1218519041, maxDistance))
			{
				info.HeadVisible = true;
				npc.AiDomain.NpcContext.EnemyPlayersInLineOfSight.Add(info);
			}
		}

		public static Ray AimAtBody(IHTNAgent npc, ref NpcPlayerInfo info)
		{
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (hTNPlayer != null)
			{
				return AimAtBody(hTNPlayer, ref info);
			}
			HTNAnimal hTNAnimal = npc as HTNAnimal;
			if (hTNAnimal != null)
			{
				return AimAtBody(hTNAnimal, ref info);
			}
			return default(Ray);
		}

		public static Ray AimAtHead(IHTNAgent npc, ref NpcPlayerInfo info)
		{
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (hTNPlayer != null)
			{
				return AimAtHead(hTNPlayer, ref info);
			}
			HTNAnimal hTNAnimal = npc as HTNAnimal;
			if (hTNAnimal != null)
			{
				return AimAtHead(hTNAnimal, ref info);
			}
			return default(Ray);
		}

		public static Ray AimAtBody(HTNPlayer npc, ref NpcPlayerInfo info)
		{
			return new Ray(npc.eyes.position, (info.Player.CenterPoint() - npc.CenterPoint()).normalized);
		}

		public static Ray AimAtHead(HTNPlayer npc, ref NpcPlayerInfo info)
		{
			return new Ray(npc.eyes.position, (info.Player.eyes.position - npc.CenterPoint()).normalized);
		}

		public static Ray AimAtBody(HTNAnimal npc, ref NpcPlayerInfo info)
		{
			return new Ray(npc.CenterPoint(), (info.Player.CenterPoint() - npc.CenterPoint()).normalized);
		}

		public static Ray AimAtHead(HTNAnimal npc, ref NpcPlayerInfo info)
		{
			return new Ray(npc.CenterPoint(), (info.Player.eyes.position - npc.CenterPoint()).normalized);
		}
	}
}
