using System.Collections.Generic;
using ConVar;
using Epic.OnlineServices;
using Epic.OnlineServices.Reports;
using Oxide.Core;
using UnityEngine;

public static class AntiHack
{
	private const int movement_mask = 429990145;

	private const int grounded_mask = 1503731969;

	private const int vehicle_mask = 8192;

	private const int player_mask = 131072;

	private static Collider[] buffer = new Collider[4];

	private static Dictionary<ulong, int> kicks = new Dictionary<ulong, int>();

	private static Dictionary<ulong, int> bans = new Dictionary<ulong, int>();

	private static RaycastHit buildRayHit;

	public static void ResetTimer(BasePlayer ply)
	{
		ply.lastViolationTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public static bool ShouldIgnore(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.ShouldIgnore"))
		{
			if (ply.IsFlying)
			{
				ply.lastAdminCheatTime = UnityEngine.Time.realtimeSinceStartup;
			}
			else if ((ply.IsAdmin || ply.IsDeveloper) && ply.lastAdminCheatTime == 0f)
			{
				ply.lastAdminCheatTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if (ply.IsAdmin)
			{
				if (ConVar.AntiHack.userlevel < 1)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsDeveloper)
			{
				if (ConVar.AntiHack.userlevel < 2)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsSpectating())
			{
				return true;
			}
			return false;
		}
	}

	public static bool ValidateMove(BasePlayer ply, TickInterpolator ticks, float deltaTime)
	{
		using (TimeWarning.New("AntiHack.ValidateMove"))
		{
			if (ShouldIgnore(ply))
			{
				return true;
			}
			bool flag = deltaTime > ConVar.AntiHack.maxdeltatime;
			if (IsNoClipping(ply, ticks, deltaTime))
			{
				if (flag)
				{
					return false;
				}
				AddViolation(ply, AntiHackType.NoClip, ConVar.AntiHack.noclip_penalty * ticks.Length);
				if (ConVar.AntiHack.noclip_reject)
				{
					return false;
				}
			}
			if (IsSpeeding(ply, ticks, deltaTime))
			{
				if (flag)
				{
					return false;
				}
				AddViolation(ply, AntiHackType.SpeedHack, ConVar.AntiHack.speedhack_penalty * ticks.Length);
				if (ConVar.AntiHack.speedhack_reject)
				{
					return false;
				}
			}
			if (IsFlying(ply, ticks, deltaTime))
			{
				if (flag)
				{
					return false;
				}
				AddViolation(ply, AntiHackType.FlyHack, ConVar.AntiHack.flyhack_penalty * ticks.Length);
				if (ConVar.AntiHack.flyhack_reject)
				{
					return false;
				}
			}
			return true;
		}
	}

	public static void ValidateEyeHistory(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.ValidateEyeHistory"))
		{
			for (int i = 0; i < ply.eyeHistory.Count; i++)
			{
				Vector3 point = ply.eyeHistory[i];
				if (ply.tickHistory.Distance(ply, point) > ConVar.AntiHack.eye_history_forgiveness)
				{
					AddViolation(ply, AntiHackType.EyeHack, ConVar.AntiHack.eye_history_penalty);
				}
			}
			ply.eyeHistory.Clear();
		}
	}

	public static bool IsInsideTerrain(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.IsInsideTerrain"))
		{
			return TestInsideTerrain(ply.transform.position);
		}
	}

	public static bool TestInsideTerrain(Vector3 pos)
	{
		if (!TerrainMeta.Terrain)
		{
			return false;
		}
		if (!TerrainMeta.HeightMap)
		{
			return false;
		}
		if (!TerrainMeta.Collision)
		{
			return false;
		}
		float terrain_padding = ConVar.AntiHack.terrain_padding;
		float height = TerrainMeta.HeightMap.GetHeight(pos);
		if (pos.y > height - terrain_padding)
		{
			return false;
		}
		float num = TerrainMeta.Position.y + TerrainMeta.Terrain.SampleHeight(pos);
		if (pos.y > num - terrain_padding)
		{
			return false;
		}
		if (TerrainMeta.Collision.GetIgnore(pos))
		{
			return false;
		}
		return true;
	}

	public static bool IsNoClipping(BasePlayer ply, TickInterpolator ticks, float deltaTime)
	{
		using (TimeWarning.New("AntiHack.IsNoClipping"))
		{
			ply.vehiclePauseTime = Mathf.Max(0f, ply.vehiclePauseTime - deltaTime);
			if (ConVar.AntiHack.noclip_protection <= 0)
			{
				return false;
			}
			ticks.Reset();
			if (!ticks.HasNext())
			{
				return false;
			}
			bool flag = ply.transform.parent == null;
			Matrix4x4 matrix4x = (flag ? Matrix4x4.identity : ply.transform.parent.localToWorldMatrix);
			Vector3 vector = (flag ? ticks.StartPoint : matrix4x.MultiplyPoint3x4(ticks.StartPoint));
			Vector3 vector2 = (flag ? ticks.EndPoint : matrix4x.MultiplyPoint3x4(ticks.EndPoint));
			Vector3 vector3 = ply.NoClipOffset();
			float radius = ply.NoClipRadius(ConVar.AntiHack.noclip_margin);
			float noclip_backtracking = ConVar.AntiHack.noclip_backtracking;
			bool vehicleLayer = ply.vehiclePauseTime <= 0f;
			if (ConVar.AntiHack.noclip_protection >= 3)
			{
				float b = Mathf.Max(ConVar.AntiHack.noclip_stepsize, 0.1f);
				int num = Mathf.Max(ConVar.AntiHack.noclip_maxsteps, 1);
				b = Mathf.Max(ticks.Length / (float)num, b);
				while (ticks.MoveNext(b))
				{
					vector2 = (flag ? ticks.CurrentPoint : matrix4x.MultiplyPoint3x4(ticks.CurrentPoint));
					if (TestNoClipping(ply, vector + vector3, vector2 + vector3, radius, noclip_backtracking, sphereCast: true, vehicleLayer))
					{
						return true;
					}
					vector = vector2;
				}
			}
			else if (ConVar.AntiHack.noclip_protection >= 2)
			{
				if (TestNoClipping(ply, vector + vector3, vector2 + vector3, radius, noclip_backtracking, sphereCast: true, vehicleLayer))
				{
					return true;
				}
			}
			else if (TestNoClipping(ply, vector + vector3, vector2 + vector3, radius, noclip_backtracking, sphereCast: false, vehicleLayer))
			{
				return true;
			}
			return false;
		}
	}

	public static bool TestNoClipping(BasePlayer ply, Vector3 oldPos, Vector3 newPos, float radius, float backtracking, bool sphereCast, bool vehicleLayer = false, BaseEntity ignoreEntity = null)
	{
		int num = 429990145;
		if (!vehicleLayer)
		{
			num &= -8193;
		}
		Vector3 normalized = (newPos - oldPos).normalized;
		Vector3 vector = oldPos - normalized * backtracking;
		float magnitude = (newPos - vector).magnitude;
		Ray ray = new Ray(vector, normalized);
		RaycastHit hitInfo;
		bool flag = ((ignoreEntity == null) ? UnityEngine.Physics.Raycast(ray, out hitInfo, magnitude + radius, num, QueryTriggerInteraction.Ignore) : GamePhysics.Trace(ray, 0f, out hitInfo, magnitude + radius, num, QueryTriggerInteraction.Ignore, ignoreEntity));
		if (!flag && sphereCast)
		{
			flag = ((ignoreEntity == null) ? UnityEngine.Physics.SphereCast(ray, radius, out hitInfo, magnitude, num, QueryTriggerInteraction.Ignore) : GamePhysics.Trace(ray, radius, out hitInfo, magnitude, num, QueryTriggerInteraction.Ignore, ignoreEntity));
		}
		if (flag)
		{
			return GamePhysics.Verify(hitInfo);
		}
		return false;
	}

	public static bool IsSpeeding(BasePlayer ply, TickInterpolator ticks, float deltaTime)
	{
		using (TimeWarning.New("AntiHack.IsSpeeding"))
		{
			ply.speedhackPauseTime = Mathf.Max(0f, ply.speedhackPauseTime - deltaTime);
			if (ConVar.AntiHack.speedhack_protection <= 0)
			{
				return false;
			}
			bool num = ply.transform.parent == null;
			Matrix4x4 matrix4x = (num ? Matrix4x4.identity : ply.transform.parent.localToWorldMatrix);
			Vector3 vector = (num ? ticks.StartPoint : matrix4x.MultiplyPoint3x4(ticks.StartPoint));
			Vector3 obj = (num ? ticks.EndPoint : matrix4x.MultiplyPoint3x4(ticks.EndPoint));
			float running = 1f;
			float ducking = 0f;
			float crawling = 0f;
			if (ConVar.AntiHack.speedhack_protection >= 2)
			{
				bool flag = ply.IsRunning();
				bool flag2 = ply.IsDucked();
				bool flag3 = ply.IsSwimming();
				bool num2 = ply.IsCrawling();
				running = (flag ? 1f : 0f);
				ducking = ((flag2 || flag3) ? 1f : 0f);
				crawling = (num2 ? 1f : 0f);
			}
			float speed = ply.GetSpeed(running, ducking, crawling);
			Vector3 v = obj - vector;
			float num3 = v.Magnitude2D();
			float num4 = deltaTime * speed;
			if (num3 > num4)
			{
				Vector3 v2 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetNormal(vector) : Vector3.up);
				float num5 = Mathf.Max(0f, Vector3.Dot(v2.XZ3D(), v.XZ3D())) * ConVar.AntiHack.speedhack_slopespeed * deltaTime;
				num3 = Mathf.Max(0f, num3 - num5);
			}
			float num6 = Mathf.Max((ply.speedhackPauseTime > 0f) ? ConVar.AntiHack.speedhack_forgiveness_inertia : ConVar.AntiHack.speedhack_forgiveness, 0.1f);
			float num7 = num6 + Mathf.Max(ConVar.AntiHack.speedhack_forgiveness, 0.1f);
			ply.speedhackDistance = Mathf.Clamp(ply.speedhackDistance, 0f - num7, num7);
			ply.speedhackDistance = Mathf.Clamp(ply.speedhackDistance - num4, 0f - num7, num7);
			if (ply.speedhackDistance > num6)
			{
				return true;
			}
			ply.speedhackDistance = Mathf.Clamp(ply.speedhackDistance + num3, 0f - num7, num7);
			if (ply.speedhackDistance > num6)
			{
				return true;
			}
			return false;
		}
	}

	public static bool IsFlying(BasePlayer ply, TickInterpolator ticks, float deltaTime)
	{
		using (TimeWarning.New("AntiHack.IsFlying"))
		{
			ply.flyhackPauseTime = Mathf.Max(0f, ply.flyhackPauseTime - deltaTime);
			if (ConVar.AntiHack.flyhack_protection <= 0)
			{
				return false;
			}
			ticks.Reset();
			if (!ticks.HasNext())
			{
				return false;
			}
			bool flag = ply.transform.parent == null;
			Matrix4x4 matrix4x = (flag ? Matrix4x4.identity : ply.transform.parent.localToWorldMatrix);
			Vector3 oldPos = (flag ? ticks.StartPoint : matrix4x.MultiplyPoint3x4(ticks.StartPoint));
			Vector3 newPos = (flag ? ticks.EndPoint : matrix4x.MultiplyPoint3x4(ticks.EndPoint));
			if (ConVar.AntiHack.flyhack_protection >= 3)
			{
				float b = Mathf.Max(ConVar.AntiHack.flyhack_stepsize, 0.1f);
				int num = Mathf.Max(ConVar.AntiHack.flyhack_maxsteps, 1);
				b = Mathf.Max(ticks.Length / (float)num, b);
				while (ticks.MoveNext(b))
				{
					newPos = (flag ? ticks.CurrentPoint : matrix4x.MultiplyPoint3x4(ticks.CurrentPoint));
					if (TestFlying(ply, oldPos, newPos, verifyGrounded: true))
					{
						return true;
					}
					oldPos = newPos;
				}
			}
			else if (ConVar.AntiHack.flyhack_protection >= 2)
			{
				if (TestFlying(ply, oldPos, newPos, verifyGrounded: true))
				{
					return true;
				}
			}
			else if (TestFlying(ply, oldPos, newPos, verifyGrounded: false))
			{
				return true;
			}
			return false;
		}
	}

	public static bool TestFlying(BasePlayer ply, Vector3 oldPos, Vector3 newPos, bool verifyGrounded)
	{
		ply.isInAir = false;
		ply.isOnPlayer = false;
		if (verifyGrounded)
		{
			float flyhack_extrusion = ConVar.AntiHack.flyhack_extrusion;
			Vector3 vector = (oldPos + newPos) * 0.5f;
			if (!ply.OnLadder() && !WaterLevel.Test(vector - new Vector3(0f, flyhack_extrusion, 0f), waves: true, ply) && (EnvironmentManager.Get(vector) & EnvironmentType.Elevator) == 0)
			{
				float flyhack_margin = ConVar.AntiHack.flyhack_margin;
				float radius = ply.GetRadius();
				float height = ply.GetHeight(ducked: false);
				Vector3 vector2 = vector + new Vector3(0f, radius - flyhack_extrusion, 0f);
				Vector3 vector3 = vector + new Vector3(0f, height - radius, 0f);
				float radius2 = radius - flyhack_margin;
				ply.isInAir = !UnityEngine.Physics.CheckCapsule(vector2, vector3, radius2, 1503731969, QueryTriggerInteraction.Ignore);
				if (ply.isInAir)
				{
					int num = UnityEngine.Physics.OverlapCapsuleNonAlloc(vector2, vector3, radius2, buffer, 131072, QueryTriggerInteraction.Ignore);
					for (int i = 0; i < num; i++)
					{
						BasePlayer basePlayer = GameObjectEx.ToBaseEntity(buffer[i].gameObject) as BasePlayer;
						if (!(basePlayer == null) && !(basePlayer == ply) && !basePlayer.isInAir && !basePlayer.isOnPlayer && !basePlayer.TriggeredAntiHack() && !basePlayer.IsSleeping())
						{
							ply.isOnPlayer = true;
							ply.isInAir = false;
							break;
						}
					}
					for (int j = 0; j < buffer.Length; j++)
					{
						buffer[j] = null;
					}
				}
			}
		}
		else
		{
			ply.isInAir = !ply.OnLadder() && !ply.IsSwimming() && !ply.IsOnGround();
		}
		if (ply.isInAir)
		{
			bool flag = false;
			Vector3 v = newPos - oldPos;
			float num2 = Mathf.Abs(v.y);
			float num3 = v.Magnitude2D();
			if (v.y >= 0f)
			{
				ply.flyhackDistanceVertical += v.y;
				flag = true;
			}
			if (num2 < num3)
			{
				ply.flyhackDistanceHorizontal += num3;
				flag = true;
			}
			if (flag)
			{
				float num4 = Mathf.Max((ply.flyhackPauseTime > 0f) ? ConVar.AntiHack.flyhack_forgiveness_vertical_inertia : ConVar.AntiHack.flyhack_forgiveness_vertical, 0f);
				float num5 = ply.GetJumpHeight() + num4;
				if (ply.flyhackDistanceVertical > num5)
				{
					return true;
				}
				float num6 = Mathf.Max((ply.flyhackPauseTime > 0f) ? ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia : ConVar.AntiHack.flyhack_forgiveness_horizontal, 0f);
				float num7 = 5f + num6;
				if (ply.flyhackDistanceHorizontal > num7)
				{
					return true;
				}
			}
		}
		else
		{
			ply.flyhackDistanceVertical = 0f;
			ply.flyhackDistanceHorizontal = 0f;
		}
		return false;
	}

	public static bool TestIsBuildingInsideSomething(Construction.Target target, Vector3 deployPos)
	{
		if (ConVar.AntiHack.build_inside_check <= 0)
		{
			return false;
		}
		bool queriesHitBackfaces = UnityEngine.Physics.queriesHitBackfaces;
		UnityEngine.Physics.queriesHitBackfaces = true;
		if (IsInside(deployPos) && IsInside(target.ray.origin))
		{
			LogToConsole(target.player, AntiHackType.InsideTerrain, "Tried to build while clipped inside " + buildRayHit.collider.name);
			if (ConVar.AntiHack.build_inside_check > 1)
			{
				return true;
			}
		}
		UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
		return false;
		static bool IsInside(Vector3 pos)
		{
			if (UnityEngine.Physics.Raycast(pos, Vector3.up, out buildRayHit, 50f, 65537))
			{
				return Vector3.Dot(Vector3.up, buildRayHit.normal) > 0f;
			}
			return false;
		}
	}

	public static void NoteAdminHack(BasePlayer ply)
	{
		Ban(ply, "Cheat Detected!");
	}

	public static void FadeViolations(BasePlayer ply, float deltaTime)
	{
		if (UnityEngine.Time.realtimeSinceStartup - ply.lastViolationTime > ConVar.AntiHack.relaxationpause)
		{
			ply.violationLevel = Mathf.Max(0f, ply.violationLevel - ConVar.AntiHack.relaxationrate * deltaTime);
		}
	}

	public static void EnforceViolations(BasePlayer ply)
	{
		if (ConVar.AntiHack.enforcementlevel > 0 && ply.violationLevel > ConVar.AntiHack.maxviolation)
		{
			if (ConVar.AntiHack.debuglevel >= 1)
			{
				LogToConsole(ply, ply.lastViolationType, "Enforcing (violation of " + ply.violationLevel + ")");
			}
			string reason = string.Concat(ply.lastViolationType, " Violation Level ", ply.violationLevel);
			if (ConVar.AntiHack.enforcementlevel > 1)
			{
				Kick(ply, reason);
			}
			else
			{
				Kick(ply, reason);
			}
		}
	}

	public static void Log(BasePlayer ply, AntiHackType type, string message)
	{
		if (ConVar.AntiHack.debuglevel > 1)
		{
			LogToConsole(ply, type, message);
		}
		LogToEAC(ply, type, message);
	}

	private static void LogToConsole(BasePlayer ply, AntiHackType type, string message)
	{
		Debug.LogWarning(string.Concat(ply, " ", type, ": ", message, " at ", ply.transform.position));
	}

	private static void LogToEAC(BasePlayer ply, AntiHackType type, string message)
	{
		if (ConVar.AntiHack.reporting && EACServer.Reports != null)
		{
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions = default(SendPlayerBehaviorReportOptions);
			sendPlayerBehaviorReportOptions.ReportedUserId = ProductUserId.FromString(ply.UserIDString);
			sendPlayerBehaviorReportOptions.Category = PlayerReportsCategory.Exploiting;
			sendPlayerBehaviorReportOptions.Message = string.Concat(type, ": ", message);
			SendPlayerBehaviorReportOptions options = sendPlayerBehaviorReportOptions;
			EACServer.Reports.SendPlayerBehaviorReport(ref options, null, null);
		}
	}

	public static void AddViolation(BasePlayer ply, AntiHackType type, float amount)
	{
		if (Interface.CallHook("OnPlayerViolation", ply, type, amount) != null)
		{
			return;
		}
		using (TimeWarning.New("AntiHack.AddViolation"))
		{
			ply.lastViolationType = type;
			ply.lastViolationTime = UnityEngine.Time.realtimeSinceStartup;
			ply.violationLevel += amount;
			if ((ConVar.AntiHack.debuglevel >= 2 && amount > 0f) || (ConVar.AntiHack.debuglevel >= 3 && type != AntiHackType.NoClip) || ConVar.AntiHack.debuglevel >= 4)
			{
				LogToConsole(ply, type, "Added violation of " + amount + " in frame " + UnityEngine.Time.frameCount + " (now has " + ply.violationLevel + ")");
			}
			EnforceViolations(ply);
		}
	}

	public static void Kick(BasePlayer ply, string reason)
	{
		AddRecord(ply, kicks);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "kick", ply.userID, reason);
	}

	public static void Ban(BasePlayer ply, string reason)
	{
		AddRecord(ply, bans);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "ban", ply.userID, reason);
	}

	private static void AddRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (records.ContainsKey(ply.userID))
		{
			records[ply.userID]++;
		}
		else
		{
			records.Add(ply.userID, 1);
		}
	}

	public static int GetKickRecord(BasePlayer ply)
	{
		return GetRecord(ply, kicks);
	}

	public static int GetBanRecord(BasePlayer ply)
	{
		return GetRecord(ply, bans);
	}

	private static int GetRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (!records.ContainsKey(ply.userID))
		{
			return 0;
		}
		return records[ply.userID];
	}
}
