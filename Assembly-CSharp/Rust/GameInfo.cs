using ConVar;
using UnityEngine;

namespace Rust;

internal static class GameInfo
{
	internal static bool IsOfficialServer
	{
		get
		{
			if (UnityEngine.Application.isEditor)
			{
				return true;
			}
			return ConVar.Server.official;
		}
	}

	internal static bool HasAchievements => IsOfficialServer;
}
