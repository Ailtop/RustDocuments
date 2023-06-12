using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace ConVar;

[ConsoleSystem.Factory("system")]
public static class SystemCommands
{
	[ServerVar]
	[ClientVar]
	public static void cpu_affinity(ConsoleSystem.Arg arg)
	{
		long num = 0L;
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'cpu_affinity {core,core1-core2,etc}'");
			return;
		}
		string[] array = arg.GetString(0).Split(',');
		HashSet<int> hashSet = new HashSet<int>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (int.TryParse(text, out var result))
			{
				hashSet.Add(result);
			}
			else
			{
				if (!text.Contains('-'))
				{
					continue;
				}
				string[] array3 = text.Split('-');
				if (array3.Length != 2)
				{
					arg.ReplyWith("Failed to parse section " + text + ", format should be '0-15'");
					continue;
				}
				if (!int.TryParse(array3[0], out var result2) || !int.TryParse(array3[1], out var result3))
				{
					arg.ReplyWith("Core range in section " + text + " are not valid numbers, format should be '0-15'");
					continue;
				}
				if (result2 > result3)
				{
					arg.ReplyWith("Core range in section " + text + " are not ordered from least to greatest, format should be '0-15'");
					continue;
				}
				if (result3 - result2 > 64)
				{
					arg.ReplyWith("Core range in section " + text + " are too big of a range, must be <64");
					return;
				}
				for (int j = result2; j <= result3; j++)
				{
					hashSet.Add(j);
				}
			}
		}
		if (hashSet.Any((int x) => x < 0 || x > 63))
		{
			arg.ReplyWith("Cores provided out of range! Must be in between 0 and 63");
			return;
		}
		for (int k = 0; k < 64; k++)
		{
			if (hashSet.Contains(k))
			{
				num |= 1L << k;
			}
		}
		if (num == 0L)
		{
			arg.ReplyWith("No cores provided (bitmask empty)! Format is 'cpu_affinity {core,core1-core2,etc}'");
			return;
		}
		try
		{
			WindowsAffinityShim.SetProcessAffinityMask(Process.GetCurrentProcess().Handle, new IntPtr(num));
		}
		catch (Exception arg2)
		{
			UnityEngine.Debug.LogWarning($"Unable to set cpu affinity: {arg2}");
			return;
		}
		arg.ReplyWith("Successfully changed cpu affinity");
	}

	[ServerVar]
	[ClientVar]
	public static void cpu_priority(ConsoleSystem.Arg arg)
	{
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			arg.ReplyWith("OSX is not a supported platform");
			return;
		}
		string @string = arg.GetString(0);
		ProcessPriorityClass mask;
		switch (@string.Replace("-", "").Replace("_", ""))
		{
		case "belownormal":
			mask = ProcessPriorityClass.BelowNormal;
			break;
		case "normal":
			mask = ProcessPriorityClass.Normal;
			break;
		case "abovenormal":
			mask = ProcessPriorityClass.AboveNormal;
			break;
		case "high":
			mask = ProcessPriorityClass.High;
			break;
		default:
			arg.ReplyWith("Unknown priority '" + @string + "', possible values: below_normal, normal, above_normal, high");
			return;
		}
		try
		{
			WindowsAffinityShim.SetPriorityClass(Process.GetCurrentProcess().Handle, (uint)mask);
		}
		catch (Exception arg2)
		{
			UnityEngine.Debug.LogWarning($"Unable to set cpu priority: {arg2}");
			return;
		}
		arg.ReplyWith("Successfully changed cpu priority to " + mask);
	}
}
