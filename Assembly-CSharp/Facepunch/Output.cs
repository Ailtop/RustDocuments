using System;
using System.Collections.Generic;
using Facepunch.Math;
using UnityEngine;

namespace Facepunch;

public static class Output
{
	public struct Entry
	{
		public string Message;

		public string Stacktrace;

		public string Type;

		public int Time;
	}

	public static bool installed = false;

	public static List<Entry> HistoryOutput = new List<Entry>();

	public static event Action<string, string, LogType> OnMessage;

	public static void Install()
	{
		if (!installed)
		{
			UnityEngine.Application.logMessageReceived += LogHandler;
			installed = true;
		}
	}

	public static void LogHandler(string log, string stacktrace, LogType type)
	{
		if (Output.OnMessage == null || log.StartsWith("Kinematic body only supports Speculative Continuous collision detection") || log.StartsWith("Skipped frame because GfxDevice") || log.StartsWith("Your current multi-scene setup has inconsistent Lighting") || log.Contains("HandleD3DDeviceLost") || log.Contains("ResetD3DDevice") || log.Contains("dev->Reset") || log.Contains("D3Dwindow device not lost anymore") || log.Contains("D3D device reset") || log.Contains("group < 0xfff") || log.Contains("Mesh can not have more than 65000 vert") || log.Contains("Trying to add (Layout Rebuilder for)") || log.Contains("Coroutine continue failure") || log.Contains("No texture data available to upload") || log.Contains("Trying to reload asset from disk that is not") || log.Contains("Unable to find shaders used for the terrain engine.") || log.Contains("Canvas element contains more than 65535 vertices") || log.Contains("RectTransform.set_anchorMin") || log.Contains("FMOD failed to initialize the output device") || log.Contains("Cannot create FMOD::Sound") || log.Contains("invalid utf-16 sequence") || log.Contains("missing surrogate tail") || log.Contains("Failed to create agent because it is not close enough to the Nav") || log.Contains("user-provided triangle mesh descriptor is invalid") || log.Contains("Releasing render texture that is set as"))
		{
			return;
		}
		using (TimeWarning.New("Facepunch.Output.LogHandler"))
		{
			try
			{
				Output.OnMessage?.Invoke(log, stacktrace, type);
			}
			catch (Exception)
			{
			}
		}
		HistoryOutput.Add(new Entry
		{
			Message = log,
			Stacktrace = stacktrace,
			Type = type.ToString(),
			Time = Epoch.Current
		});
		while (HistoryOutput.Count > 65536)
		{
			HistoryOutput.RemoveAt(0);
		}
	}
}
