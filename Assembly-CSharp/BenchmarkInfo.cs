using System;
using System.Text;
using Facepunch;
using Rust.Workshop;
using TMPro;
using UnityEngine;

public class BenchmarkInfo : SingletonComponent<BenchmarkInfo>
{
	public static string BenchmarkTitle;

	public static string BenchmarkSubtitle;

	public TextMeshProUGUI PerformanceText;

	public TextMeshProUGUI SystemInfoText;

	private StringBuilder sb = new StringBuilder();

	private RealTimeSince timeSinceUpdated;

	private void Start()
	{
		string str = Environment.MachineName + "\n";
		str = str + SystemInfo.operatingSystem + "\n";
		str += $"{(double)SystemInfo.systemMemorySize / 1024.0:0}GB RAM\n";
		str = str + SystemInfo.processorType + "\n";
		str += $"{SystemInfo.graphicsDeviceName} ({(double)SystemInfo.graphicsMemorySize / 1024.0:0}GB)\n";
		str += "\n";
		str = str + BuildInfo.Current.Build.Node + " / " + BuildInfo.Current.Scm.Date + "\n";
		str = str + BuildInfo.Current.Scm.Repo + "/" + BuildInfo.Current.Scm.Branch + "#" + BuildInfo.Current.Scm.ChangeId + "\n";
		str = str + BuildInfo.Current.Scm.Author + " - " + BuildInfo.Current.Scm.Comment + "\n";
		SystemInfoText.text = str;
	}

	private void Update()
	{
		if (!((float)timeSinceUpdated < 0.25f))
		{
			timeSinceUpdated = 0f;
			sb.Clear();
			sb.AppendLine(BenchmarkTitle);
			sb.AppendLine(BenchmarkSubtitle);
			sb.AppendLine();
			sb.Append(Performance.current.frameRate).Append(" FPS");
			sb.Append(" / ").Append(Performance.current.frameTime.ToString("0.0")).Append("ms");
			sb.AppendLine().Append(Performance.current.memoryAllocations).Append(" MB");
			sb.Append(" / ").Append(Performance.current.memoryCollections).Append(" GC");
			sb.AppendLine().Append(Performance.current.memoryUsageSystem).Append(" RAM");
			sb.AppendLine().Append(Performance.current.loadBalancerTasks).Append(" TASKS");
			sb.Append(" / ").Append(Rust.Workshop.WorkshopSkin.QueuedCount).Append(" SKINS");
			sb.Append(" / ").Append(Performance.current.invokeHandlerTasks).Append(" INVOKES");
			sb.AppendLine();
			sb.AppendLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString());
			PerformanceText.text = sb.ToString();
		}
	}
}
