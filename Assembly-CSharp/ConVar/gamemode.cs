using UnityEngine;

namespace ConVar;

[Factory("gamemode")]
public class gamemode : ConsoleSystem
{
	[ServerUserVar]
	public static void setteam(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode)
		{
			int @int = arg.GetInt(0);
			if (@int >= 0 && @int < activeGameMode.GetNumTeams())
			{
				activeGameMode.ResetPlayerScores(basePlayer);
				activeGameMode.SetPlayerTeam(basePlayer, @int);
				basePlayer.Respawn();
			}
		}
	}

	[ServerVar]
	public static void set(Arg arg)
	{
		string @string = arg.GetString(0);
		if (string.IsNullOrEmpty(@string))
		{
			Debug.Log("Invalid gamemode");
		}
		BaseGameMode baseGameMode = null;
		GameObjectRef gameObjectRef = null;
		GameModeManifest gameModeManifest = GameModeManifest.Get();
		Debug.Log("total gamemodes : " + gameModeManifest.gameModePrefabs.Count);
		foreach (GameObjectRef gameModePrefab in gameModeManifest.gameModePrefabs)
		{
			BaseGameMode component = gameModePrefab.Get().GetComponent<BaseGameMode>();
			if (component.shortname == @string)
			{
				baseGameMode = component;
				gameObjectRef = gameModePrefab;
				Debug.Log("Found :" + component.shortname + " prefab name is :" + component.PrefabName + ": rpath is " + gameModePrefab.resourcePath + ":");
				break;
			}
			Debug.Log("search name " + @string + "searched against : " + component.shortname);
		}
		if (baseGameMode == null)
		{
			Debug.Log("Unknown gamemode : " + @string);
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode)
		{
			if (baseGameMode.shortname == activeGameMode.shortname)
			{
				Debug.Log("Same gamemode, resetting");
			}
			if (activeGameMode.permanent)
			{
				Debug.LogError("This game mode is permanent, you must reset the server to switch game modes.");
				return;
			}
			activeGameMode.ShutdownGame();
			activeGameMode.Kill();
			BaseGameMode.SetActiveGameMode(null, serverside: true);
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, Vector3.zero, Quaternion.identity);
		if ((bool)baseEntity)
		{
			Debug.Log("Spawning new game mode : " + baseGameMode.shortname);
			baseEntity.Spawn();
		}
		else
		{
			Debug.Log("Failed to create new game mode :" + baseGameMode.PrefabName);
		}
	}
}
