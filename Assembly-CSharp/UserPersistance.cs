using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Math;
using Facepunch.Sqlite;
using ProtoBuf;
using UnityEngine;

public class UserPersistance : IDisposable
{
	public static Facepunch.Sqlite.Database blueprints;

	public static Facepunch.Sqlite.Database deaths;

	private static Facepunch.Sqlite.Database identities;

	private static Facepunch.Sqlite.Database tokens;

	private static Facepunch.Sqlite.Database playerState;

	private static Dictionary<ulong, string> nameCache;

	private static MruDictionary<ulong, int> tokenCache;

	public UserPersistance(string strFolder)
	{
		blueprints = new Facepunch.Sqlite.Database();
		blueprints.Open(strFolder + "/player.blueprints." + 4 + ".db");
		if (!blueprints.TableExists("data"))
		{
			blueprints.Execute("CREATE TABLE data ( userid TEXT PRIMARY KEY, info BLOB, updated INTEGER )");
		}
		deaths = new Facepunch.Sqlite.Database();
		deaths.Open(strFolder + "/player.deaths." + 4 + ".db");
		if (!deaths.TableExists("data"))
		{
			deaths.Execute("CREATE TABLE data ( userid TEXT, born INTEGER, died INTEGER, info BLOB )");
			deaths.Execute("CREATE INDEX IF NOT EXISTS userindex ON data ( userid )");
			deaths.Execute("CREATE INDEX IF NOT EXISTS diedindex ON data ( died )");
		}
		identities = new Facepunch.Sqlite.Database();
		identities.Open(strFolder + "/player.identities." + 4 + ".db");
		if (!identities.TableExists("data"))
		{
			identities.Execute("CREATE TABLE data ( userid INT PRIMARY KEY, username TEXT )");
		}
		tokens = new Facepunch.Sqlite.Database();
		tokens.Open(strFolder + "/player.tokens.db");
		if (!tokens.TableExists("data"))
		{
			tokens.Execute("CREATE TABLE data ( userid INT PRIMARY KEY, token INT )");
		}
		playerState = new Facepunch.Sqlite.Database();
		playerState.Open(strFolder + "/player.states." + 217 + ".db");
		if (!playerState.TableExists("data"))
		{
			playerState.Execute("CREATE TABLE data ( userid INT PRIMARY KEY, state BLOB )");
		}
		nameCache = new Dictionary<ulong, string>();
		tokenCache = new MruDictionary<ulong, int>(500);
	}

	public virtual void Dispose()
	{
		if (blueprints != null)
		{
			blueprints.Close();
			blueprints = null;
		}
		if (deaths != null)
		{
			deaths.Close();
			deaths = null;
		}
		if (identities != null)
		{
			identities.Close();
			identities = null;
		}
		if (tokens != null)
		{
			tokens.Close();
			tokens = null;
		}
		if (playerState != null)
		{
			playerState.Close();
			playerState = null;
		}
	}

	public PersistantPlayer GetPlayerInfo(ulong playerID)
	{
		PersistantPlayer persistantPlayer = FetchFromDatabase(playerID);
		if (persistantPlayer == null)
		{
			persistantPlayer = Pool.Get<PersistantPlayer>();
		}
		if (persistantPlayer.unlockedItems == null)
		{
			persistantPlayer.unlockedItems = Pool.GetList<int>();
		}
		return persistantPlayer;
	}

	private PersistantPlayer FetchFromDatabase(ulong playerID)
	{
		try
		{
			byte[] array = blueprints.QueryBlob("SELECT info FROM data WHERE userid = ?", playerID.ToString());
			if (array != null)
			{
				return PersistantPlayer.Deserialize(array);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading player blueprints: (" + ex.Message + ")");
		}
		return null;
	}

	public void SetPlayerInfo(ulong playerID, PersistantPlayer info)
	{
		using (TimeWarning.New("SetPlayerInfo"))
		{
			byte[] arg;
			using (TimeWarning.New("ToProtoBytes"))
			{
				arg = info.ToProtoBytes();
			}
			blueprints.Execute("INSERT OR REPLACE INTO data ( userid, info, updated ) VALUES ( ?, ?, ? )", playerID.ToString(), arg, Epoch.Current);
		}
	}

	public void AddLifeStory(ulong playerID, PlayerLifeStory lifeStory)
	{
		if (deaths == null || lifeStory == null)
		{
			return;
		}
		using (TimeWarning.New("AddLifeStory"))
		{
			byte[] arg;
			using (TimeWarning.New("ToProtoBytes"))
			{
				arg = lifeStory.ToProtoBytes();
			}
			deaths.Execute("INSERT INTO data ( userid, born, died, info ) VALUES ( ?, ?, ?, ? )", playerID.ToString(), (int)lifeStory.timeBorn, (int)lifeStory.timeDied, arg);
		}
	}

	public PlayerLifeStory GetLastLifeStory(ulong playerID)
	{
		if (deaths == null)
		{
			return null;
		}
		using (TimeWarning.New("GetLastLifeStory"))
		{
			try
			{
				byte[] array = deaths.QueryBlob("SELECT info FROM data WHERE userid = ? ORDER BY died DESC LIMIT 1", playerID.ToString());
				if (array == null)
				{
					return null;
				}
				PlayerLifeStory playerLifeStory = PlayerLifeStory.Deserialize(array);
				playerLifeStory.ShouldPool = false;
				return playerLifeStory;
			}
			catch (Exception ex)
			{
				Debug.LogError("Error loading lifestory from database: (" + ex.Message + ")");
			}
			return null;
		}
	}

	public string GetPlayerName(ulong playerID)
	{
		if (playerID == 0L)
		{
			return null;
		}
		string value;
		if (nameCache.TryGetValue(playerID, out value))
		{
			return value;
		}
		string text = identities.QueryString("SELECT username FROM data WHERE userid = ?", playerID);
		nameCache[playerID] = text;
		return text;
	}

	public void SetPlayerName(ulong playerID, string name)
	{
		if (playerID != 0L && !string.IsNullOrEmpty(name))
		{
			if (string.IsNullOrEmpty(GetPlayerName(playerID)))
			{
				identities.Execute("INSERT INTO data ( userid, username ) VALUES ( ?, ? )", playerID, name);
			}
			else
			{
				identities.Execute("UPDATE data SET username = ? WHERE userid = ?", name, playerID);
			}
			nameCache[playerID] = name;
		}
	}

	public int GetOrGenerateAppToken(ulong playerID)
	{
		if (tokens == null)
		{
			return 0;
		}
		using (TimeWarning.New("GetOrGenerateAppToken"))
		{
			int value;
			if (tokenCache.TryGetValue(playerID, out value))
			{
				return value;
			}
			int num = tokens.QueryInt("SELECT token FROM data WHERE userid = ?", playerID);
			if (num != 0)
			{
				tokenCache.Add(playerID, num);
				return num;
			}
			int num2 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			tokens.Execute("INSERT INTO data ( userid, token ) VALUES ( ?, ? )", playerID, num2);
			tokenCache.Add(playerID, num2);
			return num2;
		}
	}

	public byte[] GetPlayerState(ulong playerID)
	{
		if (playerID == 0L)
		{
			return null;
		}
		return playerState.QueryBlob("SELECT state FROM data WHERE userid = ?", playerID);
	}

	public void SetPlayerState(ulong playerID, byte[] state)
	{
		if (playerID != 0L && state != null)
		{
			playerState.Execute("INSERT OR REPLACE INTO data ( userid, state ) VALUES ( ?, ? )", playerID, state);
		}
	}

	public void ResetPlayerState(ulong playerID)
	{
		if (playerID != 0L)
		{
			playerState.Execute("DELETE FROM data WHERE userid = ?", playerID);
		}
	}
}
