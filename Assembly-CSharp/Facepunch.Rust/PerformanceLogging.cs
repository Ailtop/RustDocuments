using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Epic.OnlineServices.Version;
using Network;
using UnityEngine;

namespace Facepunch.Rust;

public class PerformanceLogging
{
	private struct LagSpike
	{
		public int FrameIndex;

		public TimeSpan Time;

		public bool WasGC;
	}

	private struct GarbageCollect
	{
		public int FrameIndex;

		public TimeSpan Time;
	}

	private class PerformancePool
	{
		public List<TimeSpan> Frametimes;

		public List<int> Ping;
	}

	private struct PluginInfo
	{
		public string Name;

		public string Author;

		public string Version;
	}

	private struct ProcessInfo
	{
		public string Name;

		public long WorkingSet;
	}

	public static PerformanceLogging server = new PerformanceLogging(client: false);

	public static PerformanceLogging client = new PerformanceLogging(client: true);

	private readonly TimeSpan ClientInterval = TimeSpan.FromMinutes(10.0);

	private readonly TimeSpan ServerInterval = TimeSpan.FromMinutes(1.0);

	private readonly TimeSpan PublicServerInterval = TimeSpan.FromHours(1.0);

	private readonly TimeSpan PingInterval = TimeSpan.FromSeconds(5.0);

	private List<TimeSpan> Frametimes = new List<TimeSpan>();

	private List<int> PingHistory = new List<int>();

	private List<LagSpike> lagSpikes = new List<LagSpike>();

	private List<GarbageCollect> garbageCollections = new List<GarbageCollect>();

	private bool isClient;

	private Stopwatch frameWatch = new Stopwatch();

	private DateTime nextPingTime;

	private DateTime nextFlushTime;

	private DateTime connectedTime;

	private int serverIndex;

	private Guid totalSessionId = Guid.NewGuid();

	private Guid sessionId;

	private int lastFrameGC;

	private ConcurrentQueue<PerformancePool> pool = new ConcurrentQueue<PerformancePool>();

	private Type oxideType;

	private bool hasOxideType;

	private List<TimeSpan> sortedList = new List<TimeSpan>();

	public PerformanceLogging(bool client)
	{
		isClient = client;
	}

	private TimeSpan GetLagSpikeThreshold()
	{
		if (!isClient)
		{
			return TimeSpan.FromMilliseconds(200.0);
		}
		return TimeSpan.FromMilliseconds(100.0);
	}

	public void OnFrame()
	{
		TimeSpan elapsed = frameWatch.Elapsed;
		Frametimes.Add(elapsed);
		frameWatch.Restart();
		DateTime utcNow = DateTime.UtcNow;
		int num = System.GC.CollectionCount(0);
		bool flag = lastFrameGC != num;
		lastFrameGC = num;
		if (flag)
		{
			garbageCollections.Add(new GarbageCollect
			{
				FrameIndex = Frametimes.Count - 1,
				Time = elapsed
			});
		}
		if (elapsed > GetLagSpikeThreshold())
		{
			lagSpikes.Add(new LagSpike
			{
				FrameIndex = Frametimes.Count - 1,
				Time = elapsed,
				WasGC = flag
			});
		}
		if (utcNow > nextFlushTime)
		{
			if (nextFlushTime == default(DateTime))
			{
				nextFlushTime = DateTime.UtcNow.Add(GetFlushInterval());
			}
			else
			{
				Flush();
			}
		}
	}

	public void Flush()
	{
		nextFlushTime = DateTime.UtcNow.Add(GetFlushInterval());
		if (!isClient && BasePlayer.activePlayerList.Count == 0 && !Analytics.Azure.Stats)
		{
			ResetMeasurements();
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		EventRecord record = EventRecord.New(isClient ? "client_performance" : "server_performance", !isClient);
		record.AddField("lag_spike_count", lagSpikes.Count).AddField("lag_spike_threshold", GetLagSpikeThreshold()).AddField("gc_count", garbageCollections.Count)
			.AddField("ram_managed", System.GC.GetTotalMemory(forceFullCollection: false))
			.AddField("ram_total", SystemInfoEx.systemMemoryUsed)
			.AddField("total_session_id", totalSessionId.ToString("N"))
			.AddField("uptime", (int)UnityEngine.Time.realtimeSinceStartup)
			.AddField("map_url", World.Url)
			.AddField("world_size", World.Size)
			.AddField("world_seed", World.Seed)
			.AddField("active_scene", LevelManager.CurrentLevelName);
		if (!isClient && !isClient)
		{
			int value = (int)((global::Network.Net.sv != null) ? global::Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond) : 0);
			int value2 = (int)((global::Network.Net.sv != null) ? global::Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond) : 0);
			record.AddField("is_official", ConVar.Server.official && ConVar.Server.stats).AddField("bot_count", BasePlayer.bots.Count).AddField("player_count", BasePlayer.activePlayerList.Count)
				.AddField("max_players", ConVar.Server.maxplayers)
				.AddField("ent_count", BaseNetworkable.serverEntities.Count)
				.AddField("hostname", ConVar.Server.hostname)
				.AddField("net_in", value)
				.AddField("net_out", value2);
		}
		if (!isClient)
		{
			try
			{
				if (!hasOxideType)
				{
					oxideType = Type.GetType("Oxide.Core.Interface,Oxide.Core");
					hasOxideType = true;
				}
				if (oxideType != null)
				{
					record.AddField("is_oxide", value: true);
					object obj = oxideType.GetProperty("Oxide", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
					if (obj != null)
					{
						object obj2 = obj.GetType().GetProperty("RootPluginManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
						if (obj2 != null)
						{
							List<PluginInfo> list = new List<PluginInfo>();
							foreach (object item in obj2.GetType().GetMethod("GetPlugins")?.Invoke(obj2, null) as IEnumerable)
							{
								if (item != null)
								{
									string name = item.GetType().GetProperty("Name")?.GetValue(item) as string;
									string author = item.GetType().GetProperty("Author")?.GetValue(item) as string;
									string version = item.GetType().GetProperty("Version")?.GetValue(item)?.ToString();
									list.Add(new PluginInfo
									{
										Name = name,
										Author = author,
										Version = version
									});
								}
							}
							record.AddObject("oxide_plugins", list);
							record.AddField("oxide_plugin_count", list.Count);
						}
					}
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Failed to get oxide when flushing server performance: {arg}");
			}
			try
			{
				List<ProcessInfo> list2 = new List<ProcessInfo>();
				Process[] processes = Process.GetProcesses();
				Process currentProcess = Process.GetCurrentProcess();
				Process[] array = processes;
				foreach (Process process in array)
				{
					try
					{
						if (currentProcess.Id != process.Id && process.ProcessName.Contains("RustDedicated"))
						{
							list2.Add(new ProcessInfo
							{
								Name = process.ProcessName,
								WorkingSet = process.WorkingSet64
							});
						}
					}
					catch (Exception ex)
					{
						if (!(ex is InvalidOperationException))
						{
							UnityEngine.Debug.LogWarning($"Failed to get memory from process when flushing performance info: {ex}");
							list2.Add(new ProcessInfo
							{
								Name = process.ProcessName,
								WorkingSet = -1L
							});
						}
					}
				}
				record.AddObject("other_servers", list2);
				record.AddField("other_server_count", list2.Count);
			}
			catch (Exception arg2)
			{
				UnityEngine.Debug.LogError($"Failed to log processes when flushing performance info: {arg2}");
			}
		}
		if (!isClient)
		{
			IEnumerable<HarmonyModInfo> harmonyMods = HarmonyLoader.GetHarmonyMods();
			record.AddObject("harmony_mods", harmonyMods);
			record.AddField("harmony_mod_count", harmonyMods.Count());
		}
		string value3;
		using (SHA256 sHA = SHA256.Create())
		{
			value3 = Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier)));
		}
		Dictionary<string, string> data = new Dictionary<string, string>
		{
			["device_name"] = SystemInfo.deviceName,
			["device_hash"] = value3,
			["gpu_name"] = SystemInfo.graphicsDeviceName,
			["gpu_ram"] = SystemInfo.graphicsMemorySize.ToString(),
			["gpu_vendor"] = SystemInfo.graphicsDeviceVendor,
			["gpu_version"] = SystemInfo.graphicsDeviceVersion,
			["cpu_cores"] = SystemInfo.processorCount.ToString(),
			["cpu_frequency"] = SystemInfo.processorFrequency.ToString(),
			["cpu_name"] = SystemInfo.processorType.Trim(),
			["system_memory"] = SystemInfo.systemMemorySize.ToString(),
			["os"] = SystemInfo.operatingSystem
		};
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			["unity"] = UnityEngine.Application.unityVersion ?? "editor",
			["changeset"] = BuildInfo.Current?.Scm.ChangeId ?? "editor",
			["branch"] = BuildInfo.Current?.Scm.Branch ?? "editor",
			["network_version"] = 2392.ToString()
		};
		dictionary["eos_sdk"] = VersionInterface.GetVersion()?.ToString() ?? "disabled";
		record.AddObject("hardware", data).AddObject("application", dictionary);
		stopwatch.Stop();
		record.AddField("flush_ms", stopwatch.ElapsedMilliseconds);
		List<TimeSpan> frametimes = Frametimes;
		List<int> ping = PingHistory;
		Task.Run(async delegate
		{
			try
			{
				await ProcessPerformanceData(record, frametimes, ping);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		});
		ResetMeasurements();
	}

	private TimeSpan GetFlushInterval()
	{
		if (!isClient)
		{
			if (Analytics.Azure.Stats)
			{
				return ServerInterval;
			}
			return PublicServerInterval;
		}
		return TimeSpan.FromHours(1.0);
	}

	private void ResetMeasurements()
	{
		nextFlushTime = DateTime.UtcNow.Add(GetFlushInterval());
		if (Frametimes.Count != 0)
		{
			PerformancePool result;
			while (pool.TryDequeue(out result))
			{
				Pool.FreeList(ref result.Frametimes);
				Pool.FreeList(ref result.Ping);
			}
			Frametimes = Pool.GetList<TimeSpan>();
			PingHistory = Pool.GetList<int>();
			garbageCollections.Clear();
		}
	}

	private Task ProcessPerformanceData(EventRecord record, List<TimeSpan> frametimes, List<int> ping)
	{
		if (frametimes.Count <= 1)
		{
			return Task.CompletedTask;
		}
		sortedList.Clear();
		sortedList.AddRange(frametimes);
		sortedList.Sort();
		int count = frametimes.Count;
		Mathf.Max(1, frametimes.Count / 100);
		Mathf.Max(1, frametimes.Count / 1000);
		TimeSpan value = default(TimeSpan);
		for (int i = 0; i < count; i++)
		{
			TimeSpan timeSpan = sortedList[i];
			value += timeSpan;
		}
		double frametime_average = value.TotalMilliseconds / (double)count;
		double value2 = System.Math.Sqrt(sortedList.Sum((TimeSpan x) => System.Math.Pow(x.TotalMilliseconds - frametime_average, 2.0)) / (double)sortedList.Count - 1.0);
		record.AddField("total_time", value).AddField("frames", count).AddField("frametime_average", value.TotalSeconds / (double)count)
			.AddField("frametime_99_9", sortedList[Mathf.Clamp(count - count / 1000, 0, count - 1)])
			.AddField("frametime_99", sortedList[Mathf.Clamp(count - count / 100, 0, count - 1)])
			.AddField("frametime_90", sortedList[Mathf.Clamp(count - count / 10, 0, count - 1)])
			.AddField("frametime_75", sortedList[Mathf.Clamp(count - count / 4, 0, count - 1)])
			.AddField("frametime_50", sortedList[count / 2])
			.AddField("frametime_25", sortedList[count / 4])
			.AddField("frametime_10", sortedList[count / 10])
			.AddField("frametime_1", sortedList[count / 100])
			.AddField("frametime_0_1", sortedList[count / 1000])
			.AddField("frametime_std_dev", value2)
			.AddField("gc_generations", System.GC.MaxGeneration)
			.AddField("gc_total", System.GC.CollectionCount(System.GC.MaxGeneration));
		if (isClient)
		{
			record.AddField("ping_average", (ping.Count != 0) ? ((int)ping.Average()) : 0).AddField("ping_count", ping.Count);
		}
		record.Submit();
		frametimes.Clear();
		ping.Clear();
		pool.Enqueue(new PerformancePool
		{
			Frametimes = frametimes,
			Ping = ping
		});
		return Task.CompletedTask;
	}
}
