using System;
using System.Collections.Generic;
using System.IO;
using ConVar;
using Facepunch;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;

public class BoomBox : EntityComponent<BaseEntity>, INotifyLOD
{
	public static Dictionary<string, string> ValidStations;

	public static Dictionary<string, string> ServerValidStations;

	[ReplicatedVar(Saved = true, Help = "A list of radio stations that are valid on this server. Format: NAME,URL,NAME,URL,etc")]
	public static string ServerUrlList = string.Empty;

	private static string lastParsedServerList;

	public ShoutcastStreamer ShoutcastStreamer;

	public GameObjectRef RadioIpDialog;

	public ulong AssignedRadioBy;

	public AudioSource SoundSource;

	public float ConditionLossRate = 0.25f;

	public ItemDefinition[] ValidCassettes;

	public SoundDefinition PlaySfx;

	public SoundDefinition StopSfx;

	public const BaseEntity.Flags HasCassette = BaseEntity.Flags.Reserved1;

	[ServerVar(Saved = true)]
	public static int BacktrackLength = 30;

	public Action<float> HurtCallback;

	public string CurrentRadioIp { get; private set; } = "rustradio.facepunch.com";


	public BaseEntity BaseEntity => base.baseEntity;

	private bool isClient => base.baseEntity.isClient;

	[ServerVar]
	public static void ClearRadioByUser(ConsoleSystem.Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			DeployableBoomBox deployableBoomBox;
			HeldBoomBox heldBoomBox;
			if ((object)(deployableBoomBox = serverEntity as DeployableBoomBox) != null)
			{
				if (deployableBoomBox.ClearRadioByUserId(uInt))
				{
					num++;
				}
			}
			else if ((object)(heldBoomBox = serverEntity as HeldBoomBox) != null && heldBoomBox.ClearRadioByUserId(uInt))
			{
				num++;
			}
		}
		arg.ReplyWith($"Stopped and cleared saved URL of {num} boom boxes");
	}

	public static void LoadStations()
	{
		if (ValidStations != null)
		{
			return;
		}
		string stationData = GetStationData();
		if (!string.IsNullOrEmpty(stationData))
		{
			try
			{
				ValidStations = JsonConvert.DeserializeObject<Dictionary<string, string>>(stationData);
			}
			catch (Exception arg)
			{
				Debug.Log($"Unable to deserialize global station list: {arg}");
			}
		}
		if (ValidStations == null)
		{
			ValidStations = new Dictionary<string, string>();
		}
		ParseServerUrlList();
	}

	private static string GetStationData()
	{
		string path = UnityEngine.Application.streamingAssetsPath + "/RadioList.txt";
		if (File.Exists(path))
		{
			return File.ReadAllText(path);
		}
		return string.Empty;
	}

	private static bool IsStationValid(string url)
	{
		ParseServerUrlList();
		if (ValidStations == null || !ValidStations.ContainsValue(url))
		{
			if (ServerValidStations != null)
			{
				return ServerValidStations.ContainsValue(url);
			}
			return false;
		}
		return true;
	}

	public static void ParseServerUrlList()
	{
		if (ServerValidStations == null)
		{
			ServerValidStations = new Dictionary<string, string>();
		}
		if (lastParsedServerList == ServerUrlList)
		{
			return;
		}
		ServerValidStations.Clear();
		if (!string.IsNullOrEmpty(ServerUrlList))
		{
			string[] array = ServerUrlList.Split(',');
			if (array.Length % 2 != 0)
			{
				Debug.Log("Invalid number of stations in BoomBox.ServerUrlList, ensure you always have a name and a url");
				return;
			}
			for (int i = 0; i < array.Length; i += 2)
			{
				if (ServerValidStations.ContainsKey(array[i]))
				{
					Debug.Log("Duplicate station name detected in BoomBox.ServerUrlList, all station names must be unique: " + array[i]);
				}
				else
				{
					ServerValidStations.Add(array[i], array[i + 1]);
				}
			}
		}
		lastParsedServerList = ServerUrlList;
	}

	public void Server_UpdateRadioIP(BaseEntity.RPCMessage msg)
	{
		string text = msg.read.String();
		if (IsStationValid(text))
		{
			if (msg.player != null)
			{
				ulong num = (AssignedRadioBy = msg.player.userID);
			}
			CurrentRadioIp = text;
			base.baseEntity.ClientRPC(null, "OnRadioIPChanged", CurrentRadioIp);
			if (IsOn())
			{
				ServerTogglePlay(false);
			}
		}
	}

	public void Save(BaseNetworkable.SaveInfo info)
	{
		if (info.msg.boomBox == null)
		{
			info.msg.boomBox = Facepunch.Pool.Get<ProtoBuf.BoomBox>();
		}
		info.msg.boomBox.radioIp = CurrentRadioIp;
		info.msg.boomBox.assignedRadioBy = AssignedRadioBy;
	}

	public bool ClearRadioByUserId(ulong id)
	{
		if (AssignedRadioBy == id)
		{
			CurrentRadioIp = string.Empty;
			AssignedRadioBy = 0uL;
			if (HasFlag(BaseEntity.Flags.On))
			{
				ServerTogglePlay(false);
			}
			return true;
		}
		return false;
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		if (info.msg.boomBox != null)
		{
			CurrentRadioIp = info.msg.boomBox.radioIp;
			AssignedRadioBy = info.msg.boomBox.assignedRadioBy;
		}
	}

	public void ServerTogglePlay(BaseEntity.RPCMessage msg)
	{
		if (IsPowered())
		{
			bool play = msg.read.ReadByte() == 1;
			ServerTogglePlay(play);
		}
	}

	private void DeductCondition()
	{
		HurtCallback?.Invoke(ConditionLossRate * ConVar.Decay.scale);
	}

	public void ServerTogglePlay(bool play)
	{
		SetFlag(BaseEntity.Flags.On, play);
		IOEntity iOEntity;
		if ((object)(iOEntity = base.baseEntity as IOEntity) != null)
		{
			iOEntity.SendChangedToRoot(true);
			iOEntity.MarkDirtyForceUpdateOutputs();
		}
		if (play && !IsInvoking(DeductCondition))
		{
			InvokeRepeating(DeductCondition, 1f, 1f);
		}
		else if (IsInvoking(DeductCondition))
		{
			CancelInvoke(DeductCondition);
		}
	}

	public void OnCassetteInserted(Cassette c)
	{
		base.baseEntity.ClientRPC(null, "Client_OnCassetteInserted", c.net.ID);
		ServerTogglePlay(false);
		base.baseEntity.SendNetworkUpdate();
		SetFlag(BaseEntity.Flags.Reserved1, true);
	}

	public void OnCassetteRemoved(Cassette c)
	{
		base.baseEntity.ClientRPC(null, "Client_OnCassetteRemoved");
		ServerTogglePlay(false);
		SetFlag(BaseEntity.Flags.Reserved1, false);
	}

	private bool IsPowered()
	{
		if (!base.baseEntity.HasFlag(BaseEntity.Flags.Reserved8))
		{
			return base.baseEntity is HeldBoomBox;
		}
		return true;
	}

	private bool IsOn()
	{
		return base.baseEntity.IsOn();
	}

	private bool HasFlag(BaseEntity.Flags f)
	{
		return base.baseEntity.HasFlag(f);
	}

	private void SetFlag(BaseEntity.Flags f, bool state)
	{
		base.baseEntity.SetFlag(f, state);
	}
}
