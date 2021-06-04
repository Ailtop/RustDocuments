using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class BaseGameMode : BaseEntity
{
	[Serializable]
	public class GameModeTeam
	{
		public string name;

		public PlayerInventoryProperties[] teamloadouts;
	}

	private GameMode gameModeScores;

	public string[] scoreColumns;

	public const Flags Flag_Warmup = Flags.Reserved1;

	public const Flags Flag_GameOver = Flags.Reserved2;

	public const Flags Flag_WaitingForPlayers = Flags.Reserved3;

	public string shortname = "vanilla";

	public float matchDuration = -1f;

	public float warmupDuration = 10f;

	public float timeBetweenMatches = 10f;

	public int minPlayersToStart = 1;

	public bool useCustomSpawns = true;

	public string victoryScoreName = "kills";

	public string teamScoreName = "kills";

	public int numScoreForVictory = 10;

	public GameObjectRef startingWeapon;

	public string gamemodeTitle;

	public SoundDefinition[] warmupMusics;

	public SoundDefinition[] lossMusics;

	public SoundDefinition[] winMusics;

	[NonSerialized]
	private float warmupStartTime;

	[NonSerialized]
	private float matchStartTime = -1f;

	[NonSerialized]
	private float matchEndTime;

	public string[] gameModeTags;

	public bool permanent = true;

	public bool limitTeamAuths;

	public static BaseGameMode svActiveGameMode = null;

	public static List<BaseGameMode> svGameModeManifest = new List<BaseGameMode>();

	[NonSerialized]
	private GameObject[] allspawns;

	[NonSerialized]
	private GameModeSpawnGroup[] gameModeSpawnGroups;

	public PlayerInventoryProperties[] loadouts;

	public GameModeTeam[] teams;

	private static bool isResetting = false;

	public static event Action<BaseGameMode> GameModeChanged;

	public GameMode GetGameScores()
	{
		return gameModeScores;
	}

	public int ScoreColumnIndex(string scoreName)
	{
		for (int i = 0; i < scoreColumns.Length; i++)
		{
			if (scoreColumns[i] == scoreName)
			{
				return i;
			}
		}
		return -1;
	}

	public void InitScores()
	{
		gameModeScores = new GameMode();
		gameModeScores.scoreColumns = new List<GameMode.ScoreColumn>();
		gameModeScores.playerScores = new List<GameMode.PlayerScore>();
		gameModeScores.teams = new List<GameMode.TeamInfo>();
		GameModeTeam[] array = teams;
		for (int i = 0; i < array.Length; i++)
		{
			GameModeTeam gameModeTeam = array[i];
			GameMode.TeamInfo teamInfo = new GameMode.TeamInfo();
			teamInfo.score = 0;
			teamInfo.ShouldPool = false;
			gameModeScores.teams.Add(teamInfo);
		}
		string[] array2 = scoreColumns;
		foreach (string text in array2)
		{
			GameMode.ScoreColumn scoreColumn = new GameMode.ScoreColumn();
			scoreColumn.name = text;
			scoreColumn.ShouldPool = false;
			gameModeScores.scoreColumns.Add(scoreColumn);
		}
		gameModeScores.ShouldPool = false;
	}

	public void CopyGameModeScores(GameMode from, GameMode to)
	{
		to.teams.Clear();
		to.scoreColumns.Clear();
		to.playerScores.Clear();
		foreach (GameMode.TeamInfo team in from.teams)
		{
			GameMode.TeamInfo teamInfo = new GameMode.TeamInfo();
			teamInfo.score = team.score;
			to.teams.Add(teamInfo);
		}
		foreach (GameMode.ScoreColumn scoreColumn2 in from.scoreColumns)
		{
			GameMode.ScoreColumn scoreColumn = new GameMode.ScoreColumn();
			scoreColumn.name = scoreColumn2.name;
			to.scoreColumns.Add(scoreColumn);
		}
		foreach (GameMode.PlayerScore playerScore2 in from.playerScores)
		{
			GameMode.PlayerScore playerScore = new GameMode.PlayerScore();
			playerScore.playerName = playerScore2.playerName;
			playerScore.userid = playerScore2.userid;
			playerScore.team = playerScore2.team;
			playerScore.scores = new List<int>();
			foreach (int score in playerScore2.scores)
			{
				playerScore.scores.Add(score);
			}
			to.playerScores.Add(playerScore);
		}
	}

	public GameMode.PlayerScore GetPlayerScoreForPlayer(BasePlayer player)
	{
		GameMode.PlayerScore playerScore = null;
		foreach (GameMode.PlayerScore playerScore2 in gameModeScores.playerScores)
		{
			if (playerScore2.userid == player.userID)
			{
				playerScore = playerScore2;
				break;
			}
		}
		if (playerScore == null)
		{
			playerScore = new GameMode.PlayerScore();
			playerScore.ShouldPool = false;
			playerScore.playerName = player.displayName;
			playerScore.userid = player.userID;
			playerScore.scores = new List<int>();
			string[] array = scoreColumns;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				playerScore.scores.Add(0);
			}
			gameModeScores.playerScores.Add(playerScore);
		}
		return playerScore;
	}

	public int GetScoreIndexByName(string name)
	{
		for (int i = 0; i < scoreColumns.Length; i++)
		{
			if (scoreColumns[i] == name)
			{
				return i;
			}
		}
		Debug.LogWarning("No score colum named : " + name + "returning default");
		return 0;
	}

	public virtual bool IsDraw()
	{
		if (IsTeamGame())
		{
			int num = -1;
			int num2 = 1000000;
			for (int i = 0; i < teams.Length; i++)
			{
				int teamScore = GetTeamScore(i);
				if (teamScore < num2)
				{
					num2 = teamScore;
				}
				if (teamScore > num)
				{
					num = teamScore;
				}
			}
			if (num == num2)
			{
				return true;
			}
			return false;
		}
		int num3 = -1;
		int num4 = 0;
		int num5 = ScoreColumnIndex(victoryScoreName);
		if (num5 != -1)
		{
			for (int j = 0; j < gameModeScores.playerScores.Count; j++)
			{
				GameMode.PlayerScore playerScore = gameModeScores.playerScores[j];
				if (playerScore.scores[num5] > num3)
				{
					num3 = playerScore.scores[num5];
					num4 = 1;
				}
				else if (playerScore.scores[num5] == num3)
				{
					num4++;
				}
			}
		}
		if (num3 != 0)
		{
			return num4 > 1;
		}
		return true;
	}

	public virtual string GetWinnerName()
	{
		int num = -1;
		int num2 = -1;
		if (IsTeamGame())
		{
			for (int i = 0; i < teams.Length; i++)
			{
				int teamScore = GetTeamScore(i);
				if (teamScore > num)
				{
					num = teamScore;
					num2 = i;
				}
			}
			if (num2 == -1)
			{
				return "NO ONE";
			}
			return teams[num2].name;
		}
		int num3 = ScoreColumnIndex(victoryScoreName);
		if (num3 != -1)
		{
			for (int j = 0; j < gameModeScores.playerScores.Count; j++)
			{
				GameMode.PlayerScore playerScore = gameModeScores.playerScores[j];
				if (playerScore.scores[num3] > num)
				{
					num = playerScore.scores[num3];
					num2 = j;
				}
			}
		}
		if (num2 != -1)
		{
			return gameModeScores.playerScores[num2].playerName;
		}
		return "";
	}

	public virtual int GetPlayerTeamPosition(BasePlayer player)
	{
		return 0;
	}

	public virtual int GetPlayerRank(BasePlayer player)
	{
		int num = ScoreColumnIndex(victoryScoreName);
		if (num == -1)
		{
			return 10;
		}
		int num2 = GetPlayerScoreForPlayer(player).scores[num];
		int num3 = 0;
		foreach (GameMode.PlayerScore playerScore in gameModeScores.playerScores)
		{
			if (playerScore.scores[num] > num2 && playerScore.userid != player.userID)
			{
				num3++;
			}
		}
		return num3 + 1;
	}

	public int GetWinningTeamIndex()
	{
		int num = -1;
		int num2 = -1;
		if (IsTeamGame())
		{
			for (int i = 0; i < teams.Length; i++)
			{
				int teamScore = GetTeamScore(i);
				if (teamScore > num)
				{
					num = teamScore;
					num2 = i;
				}
			}
			if (num2 == -1)
			{
				return -1;
			}
			return num2;
		}
		return -1;
	}

	public virtual bool DidPlayerWin(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (IsDraw())
		{
			return false;
		}
		if (IsTeamGame())
		{
			GameMode.PlayerScore playerScoreForPlayer = GetPlayerScoreForPlayer(player);
			if (playerScoreForPlayer.team == -1)
			{
				return false;
			}
			return playerScoreForPlayer.team == GetWinningTeamIndex();
		}
		return GetPlayerRank(player) == 1;
	}

	public bool IsTeamGame()
	{
		return teams.Length > 1;
	}

	public bool KeepScores()
	{
		return scoreColumns.Length != 0;
	}

	public void ModifyTeamScore(int teamIndex, int modifyAmount)
	{
		if (KeepScores())
		{
			gameModeScores.teams[teamIndex].score += modifyAmount;
			SendNetworkUpdate();
			CheckGameConditions();
		}
	}

	public void SetTeamScore(int teamIndex, int score)
	{
		gameModeScores.teams[teamIndex].score = score;
		SendNetworkUpdate();
	}

	public virtual void ResetPlayerScores(BasePlayer player)
	{
		if (!base.isClient)
		{
			for (int i = 0; i < scoreColumns.Length; i++)
			{
				SetPlayerGameScore(player, i, 0);
			}
		}
	}

	public void ModifyPlayerGameScore(BasePlayer player, string scoreName, int modifyAmount)
	{
		if (KeepScores())
		{
			int scoreIndexByName = GetScoreIndexByName(scoreName);
			ModifyPlayerGameScore(player, scoreIndexByName, modifyAmount);
		}
	}

	public void ModifyPlayerGameScore(BasePlayer player, int scoreIndex, int modifyAmount)
	{
		if (KeepScores())
		{
			GetPlayerScoreForPlayer(player);
			int playerGameScore = GetPlayerGameScore(player, scoreIndex);
			if (IsTeamGame() && player.gamemodeteam >= 0 && scoreIndex == GetScoreIndexByName(teamScoreName))
			{
				gameModeScores.teams[player.gamemodeteam].score = gameModeScores.teams[player.gamemodeteam].score + modifyAmount;
			}
			SetPlayerGameScore(player, scoreIndex, playerGameScore + modifyAmount);
		}
	}

	public int GetPlayerGameScore(BasePlayer player, int scoreIndex)
	{
		return GetPlayerScoreForPlayer(player).scores[scoreIndex];
	}

	public void SetPlayerTeam(BasePlayer player, int newTeam)
	{
		player.gamemodeteam = newTeam;
		GetPlayerScoreForPlayer(player).team = newTeam;
		SendNetworkUpdate();
	}

	public void SetPlayerGameScore(BasePlayer player, int scoreIndex, int scoreValue)
	{
		if (!base.isClient && KeepScores())
		{
			GetPlayerScoreForPlayer(player).scores[scoreIndex] = scoreValue;
			SendNetworkUpdate();
			CheckGameConditions();
		}
	}

	public bool HasAnyGameModeTag(string[] tags)
	{
		for (int i = 0; i < gameModeTags.Length; i++)
		{
			for (int j = 0; j < tags.Length; j++)
			{
				if (tags[j] == gameModeTags[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasGameModeTag(string tag)
	{
		for (int i = 0; i < gameModeTags.Length; i++)
		{
			if (gameModeTags[i] == tag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLoadouts()
	{
		return loadouts.Length != 0;
	}

	public int GetNumTeams()
	{
		if (teams.Length > 1)
		{
			return teams.Length;
		}
		return 1;
	}

	public int GetTeamScore(int teamIndex)
	{
		return gameModeScores.teams[teamIndex].score;
	}

	public static void CreateGameMode(string overrideMode = "")
	{
		BaseGameMode activeGameMode = GetActiveGameMode(true);
		if ((bool)activeGameMode)
		{
			activeGameMode.ShutdownGame();
			activeGameMode.Kill();
			SetActiveGameMode(null, true);
		}
		string text = Server.gamemode;
		Debug.Log("Gamemode Convar :" + text);
		if (!string.IsNullOrEmpty(overrideMode))
		{
			text = overrideMode;
		}
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log("No Gamemode.");
			if (BaseGameMode.GameModeChanged != null)
			{
				BaseGameMode.GameModeChanged(null);
			}
		}
		else
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/gamemodes/" + text + ".prefab", Vector3.zero, Quaternion.identity);
			if ((bool)baseEntity)
			{
				baseEntity.Spawn();
			}
			else
			{
				Debug.Log("Failed to create gamemode : " + text);
			}
		}
	}

	public static void SetActiveGameMode(BaseGameMode newActive, bool serverside)
	{
		if ((bool)newActive)
		{
			newActive.InitScores();
		}
		if (BaseGameMode.GameModeChanged != null)
		{
			BaseGameMode.GameModeChanged(newActive);
		}
		if (serverside)
		{
			svActiveGameMode = newActive;
		}
	}

	public static BaseGameMode GetActiveGameMode(bool serverside)
	{
		return svActiveGameMode;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.gameMode != null)
		{
			CopyGameModeScores(info.msg.gameMode, gameModeScores);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.gameMode = Facepunch.Pool.Get<GameMode>();
		info.msg.gameMode.scoreColumns = Facepunch.Pool.GetList<GameMode.ScoreColumn>();
		info.msg.gameMode.playerScores = Facepunch.Pool.GetList<GameMode.PlayerScore>();
		info.msg.gameMode.teams = Facepunch.Pool.GetList<GameMode.TeamInfo>();
		CopyGameModeScores(gameModeScores, info.msg.gameMode);
		info.msg.gameMode.ShouldPool = true;
	}

	public virtual float CorpseRemovalTime(BaseCorpse corpse)
	{
		return Server.corpsedespawn;
	}

	public virtual bool InWarmup()
	{
		return HasFlag(Flags.Reserved1);
	}

	public virtual bool IsWaitingForPlayers()
	{
		return HasFlag(Flags.Reserved3);
	}

	public virtual bool IsMatchOver()
	{
		return HasFlag(Flags.Reserved2);
	}

	public virtual bool IsMatchActive()
	{
		if (!InWarmup() && !IsWaitingForPlayers() && !IsMatchOver())
		{
			return matchStartTime != -1f;
		}
		return false;
	}

	public override void InitShared()
	{
		base.InitShared();
		if (GetActiveGameMode(base.isServer) != null && GetActiveGameMode(base.isServer) != this)
		{
			Debug.LogError("Already an active game mode! was : " + GetActiveGameMode(base.isServer).name);
			UnityEngine.Object.Destroy(GetActiveGameMode(base.isServer).gameObject);
		}
		SetActiveGameMode(this, base.isServer);
		OnCreated();
	}

	public override void DestroyShared()
	{
		if (GetActiveGameMode(base.isServer) == this)
		{
			SetActiveGameMode(null, base.isServer);
		}
		base.DestroyShared();
	}

	protected virtual void OnCreated()
	{
		if (base.isServer)
		{
			gameModeSpawnGroups = UnityEngine.Object.FindObjectsOfType<GameModeSpawnGroup>();
			UnassignAllPlayers();
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				AutoAssignTeam(activePlayer);
			}
			InstallSpawnpoints();
			ResetMatch();
		}
		Debug.Log("Game created! type was : " + base.name);
	}

	protected virtual void OnMatchBegin()
	{
		matchStartTime = UnityEngine.Time.realtimeSinceStartup;
		SetFlag(Flags.Reserved3, false);
		SetFlag(Flags.Reserved1, false);
		SetFlag(Flags.Reserved2, false);
	}

	public virtual void ResetMatch()
	{
		if (IsWaitingForPlayers())
		{
			return;
		}
		isResetting = true;
		SetFlag(Flags.Reserved1, true, false, false);
		SetFlag(Flags.Reserved2, false);
		ResetTeamScores();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			ResetPlayerScores(activePlayer);
			activePlayer.Respawn();
		}
		GameModeSpawnGroup[] array = gameModeSpawnGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResetSpawnGroup();
		}
		matchStartTime = -1f;
		Invoke(OnMatchBegin, warmupDuration);
		isResetting = false;
	}

	public virtual void ResetTeamScores()
	{
		for (int i = 0; i < teams.Length; i++)
		{
			SetTeamScore(i, 0);
		}
	}

	public virtual void ShutdownGame()
	{
		ResetTeamScores();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			SetPlayerTeam(activePlayer, -1);
		}
	}

	private void Update()
	{
		if (!base.isClient)
		{
			OnThink(UnityEngine.Time.deltaTime);
		}
	}

	protected virtual void OnThink(float delta)
	{
		if (matchStartTime != -1f)
		{
			float num = UnityEngine.Time.realtimeSinceStartup - matchStartTime;
			if (IsMatchActive() && matchDuration > 0f && num >= matchDuration)
			{
				OnMatchEnd();
			}
		}
		int num2 = 0;
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (activePlayer.IsConnected)
			{
				num2++;
			}
		}
		if (num2 < minPlayersToStart && !IsWaitingForPlayers())
		{
			if (IsMatchActive())
			{
				OnMatchEnd();
				return;
			}
			SetFlag(Flags.Reserved3, true);
			SetFlag(Flags.Reserved2, false);
			SetFlag(Flags.Reserved1, false);
		}
		else if (IsWaitingForPlayers() && num2 >= minPlayersToStart)
		{
			SetFlag(Flags.Reserved3, false);
			CancelInvoke(ResetMatch);
			ResetMatch();
		}
	}

	public virtual void OnMatchEnd()
	{
		matchEndTime = UnityEngine.Time.time;
		Debug.Log("Match over!");
		SetFlag(Flags.Reserved2, true);
		Invoke(ResetMatch, timeBetweenMatches);
	}

	public virtual void OnNewPlayer(BasePlayer player)
	{
		player.Respawn();
	}

	public virtual void OnPlayerConnected(BasePlayer player)
	{
		AutoAssignTeam(player);
		ResetPlayerScores(player);
	}

	public virtual void UnassignAllPlayers()
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			SetPlayerTeam(activePlayer, -1);
		}
	}

	public void AutoAssignTeam(BasePlayer player)
	{
		int newTeam = 0;
		int[] array = new int[teams.Length];
		int num = UnityEngine.Random.Range(0, teams.Length);
		int num2 = 0;
		if (teams.Length > 1)
		{
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				if (activePlayer.gamemodeteam >= 0 && activePlayer.gamemodeteam < teams.Length)
				{
					array[activePlayer.gamemodeteam]++;
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] < num2)
				{
					num = i;
				}
			}
			newTeam = num;
		}
		SetPlayerTeam(player, newTeam);
	}

	public virtual void OnPlayerDisconnected(BasePlayer player)
	{
		if (gameModeScores == null || base.isClient)
		{
			return;
		}
		GameMode.PlayerScore playerScore = null;
		foreach (GameMode.PlayerScore playerScore2 in gameModeScores.playerScores)
		{
			if (playerScore2.userid == player.userID)
			{
				playerScore = playerScore2;
				break;
			}
		}
		if (playerScore != null)
		{
			gameModeScores.playerScores.Remove(playerScore);
		}
	}

	public virtual void OnPlayerWounded(BasePlayer instigator, BasePlayer victim, HitInfo info)
	{
	}

	public virtual void OnPlayerRevived(BasePlayer instigator, BasePlayer victim)
	{
	}

	public virtual void OnPlayerDeath(BasePlayer instigator, BasePlayer victim, HitInfo deathInfo = null)
	{
		if (IsMatchActive())
		{
			if (victim != null && victim.IsConnected && !victim.IsNpc)
			{
				ModifyPlayerGameScore(victim, "deaths", 1);
			}
			bool flag = IsTeamGame() && instigator != null && victim != null && instigator.gamemodeteam == victim.gamemodeteam;
			if (instigator != null && victim != instigator && !flag && !instigator.IsNpc)
			{
				ModifyPlayerGameScore(instigator, "kills", 1);
			}
			CheckGameConditions(true);
		}
	}

	public virtual bool CanPlayerRespawn(BasePlayer player)
	{
		if (IsMatchOver() && !IsWaitingForPlayers())
		{
			return isResetting;
		}
		return true;
	}

	public virtual void OnPlayerRespawn(BasePlayer player)
	{
	}

	public virtual void CheckGameConditions(bool force = false)
	{
		if (!IsMatchActive())
		{
			return;
		}
		if (IsTeamGame())
		{
			for (int i = 0; i < teams.Length; i++)
			{
				if (GetTeamScore(i) >= numScoreForVictory)
				{
					OnMatchEnd();
				}
			}
			return;
		}
		int num = ScoreColumnIndex(victoryScoreName);
		if (num == -1)
		{
			return;
		}
		foreach (GameMode.PlayerScore playerScore in gameModeScores.playerScores)
		{
			if (playerScore.scores[num] >= numScoreForVictory)
			{
				OnMatchEnd();
			}
		}
	}

	public virtual void LoadoutPlayer(BasePlayer player)
	{
		PlayerInventoryProperties playerInventoryProperties;
		if (IsTeamGame())
		{
			if (player.gamemodeteam == -1)
			{
				Debug.LogWarning("Player loading out without team assigned, auto assigning!");
				AutoAssignTeam(player);
			}
			playerInventoryProperties = teams[player.gamemodeteam].teamloadouts[SeedRandom.Range((uint)player.userID, 0, teams[player.gamemodeteam].teamloadouts.Length)];
		}
		else
		{
			playerInventoryProperties = loadouts[SeedRandom.Range((uint)player.userID, 0, loadouts.Length)];
		}
		if ((bool)playerInventoryProperties)
		{
			playerInventoryProperties.GiveToPlayer(player);
		}
		else
		{
			player.inventory.GiveItem(ItemManager.CreateByName("hazmatsuit", 1, 0uL), player.inventory.containerWear);
		}
	}

	public virtual void InstallSpawnpoints()
	{
		allspawns = GameObject.FindGameObjectsWithTag("spawnpoint");
	}

	public virtual BasePlayer.SpawnPoint GetPlayerSpawn(BasePlayer forPlayer)
	{
		if (allspawns == null)
		{
			InstallSpawnpoints();
		}
		float num = 0f;
		int num2 = UnityEngine.Random.Range(0, allspawns.Length);
		if (allspawns.Length != 0 && forPlayer != null)
		{
			for (int i = 0; i < allspawns.Length; i++)
			{
				GameObject gameObject = allspawns[i];
				float num3 = 0f;
				for (int j = 0; j < BasePlayer.activePlayerList.Count; j++)
				{
					BasePlayer basePlayer = BasePlayer.activePlayerList[j];
					if (!(basePlayer == null) && basePlayer.IsAlive() && !(basePlayer == forPlayer))
					{
						float num4 = Vector3.Distance(basePlayer.transform.position, gameObject.transform.position);
						num3 -= 100f * (1f - Mathf.InverseLerp(2f, 6f, num4));
						if (!IsTeamGame() || basePlayer.gamemodeteam != forPlayer.gamemodeteam)
						{
							num3 += num4;
						}
					}
				}
				float value = Vector3.Distance((forPlayer.ServerCurrentDeathNote == null) ? allspawns[UnityEngine.Random.Range(0, allspawns.Length)].transform.position : forPlayer.ServerCurrentDeathNote.worldPosition, gameObject.transform.position);
				float num5 = Mathf.InverseLerp(8f, 12f, value);
				num3 *= num5;
				if (num3 > num)
				{
					num2 = i;
					num = num3;
				}
			}
		}
		GameObject gameObject2 = allspawns[num2];
		return new BasePlayer.SpawnPoint
		{
			pos = gameObject2.transform.position,
			rot = gameObject2.transform.rotation
		};
	}

	public virtual int GetMaxRelationshipTeamSize()
	{
		return RelationshipManager.maxTeamSize;
	}

	public virtual SleepingBag[] FindSleepingBagsForPlayer(ulong playerID, bool ignoreTimers)
	{
		return SleepingBag.FindForPlayer(playerID, ignoreTimers);
	}

	public virtual bool CanMoveItemsFrom(PlayerInventory inv, BaseEntity source, Item item)
	{
		return true;
	}
}
