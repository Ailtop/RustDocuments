using System.IO;
using Network;
using ProtoBuf;
using UnityEngine;

namespace ConVar;

[Factory("demo")]
public class Demo : ConsoleSystem
{
	public class Header : DemoHeader, IDemoHeader
	{
		long IDemoHeader.Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
			}
		}

		public void Write(BinaryWriter writer)
		{
			byte[] array = ToProtoBytes();
			writer.Write("RUST DEMO FORMAT");
			writer.Write(array.Length);
			writer.Write(array);
			writer.Write('\0');
		}
	}

	public static uint Version = 3u;

	[ServerVar]
	public static float splitseconds = 3600f;

	[ServerVar]
	public static float splitmegabytes = 200f;

	[ServerVar(Saved = true)]
	public static string recordlist = "";

	private static int _recordListModeValue = 0;

	[ServerVar(Saved = true, Help = "Controls the behavior of recordlist, 0=whitelist, 1=blacklist")]
	public static int recordlistmode
	{
		get
		{
			return _recordListModeValue;
		}
		set
		{
			_recordListModeValue = Mathf.Clamp(value, 0, 1);
		}
	}

	[ServerVar]
	public static string record(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			return "Player not found";
		}
		if (playerOrSleeper.net.connection.IsRecording)
		{
			return "Player already recording a demo";
		}
		playerOrSleeper.StartDemoRecording();
		return null;
	}

	[ServerVar]
	public static string stop(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			return "Player not found";
		}
		if (!playerOrSleeper.net.connection.IsRecording)
		{
			return "Player not recording a demo";
		}
		playerOrSleeper.StopDemoRecording();
		return null;
	}
}
