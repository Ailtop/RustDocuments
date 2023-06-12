using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConVar;
using Cronos;
using Facepunch;
using Facepunch.Math;
using Newtonsoft.Json;
using ProtoBuf;
using TimeZoneConverter;
using UnityEngine;

public class WipeTimer : BaseEntity
{
	public enum WipeFrequency
	{
		Monthly = 0,
		Weekly = 1,
		BiWeekly = 2
	}

	[ServerVar(Help = "0=sun,1=mon,2=tues,3=wed,4=thur,5=fri,6=sat")]
	public static int wipeDayOfWeek = 4;

	[ServerVar(Help = "Which hour to wipe? 14.5 = 2:30pm")]
	public static float wipeHourOfDay = 19f;

	[ServerVar(Help = "The timezone to use for wipes. Defaults to the server's time zone if not set or invalid. Value should be a TZ identifier as seen here: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones")]
	public static string wipeTimezone = "Europe/London";

	[ServerVar(Help = "Unix timestamp (seconds) for the upcoming wipe. Overrides all other convars if set to a time in the future.")]
	public static long wipeUnixTimestampOverride = 0L;

	[ServerVar(Help = "Custom cron expression for the wipe schedule. Overrides all other convars (except wipeUnixTimestampOverride) if set. Uses Cronos as a parser: https://github.com/HangfireIO/Cronos/")]
	public static string wipeCronOverride = "";

	public bool useWipeDayOverride;

	public DayOfWeek wipeDayOfWeekOverride = DayOfWeek.Thursday;

	public WipeFrequency wipeFrequency;

	[ServerVar(Name = "days_to_add_test")]
	public static int daysToAddTest = 0;

	[ServerVar(Name = "hours_to_add_test")]
	public static float hoursToAddTest = 0f;

	public static WipeTimer serverinstance;

	public static WipeTimer clientinstance;

	private string oldTags = "";

	private static string cronExprCacheKey = null;

	private static CronExpression cronExprCache = null;

	private static (WipeFrequency, int, float)? cronCacheKey = null;

	private static string cronCache = null;

	private static string timezoneCacheKey = null;

	private static TimeZoneInfo timezoneCache = null;

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			serverinstance = this;
		}
		if (base.isClient)
		{
			clientinstance = this;
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			serverinstance = null;
		}
		if (base.isClient)
		{
			clientinstance = null;
		}
		base.DestroyShared();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		RecalculateWipeFrequency();
		InvokeRepeating(TryAndUpdate, 1f, 4f);
	}

	public void RecalculateWipeFrequency()
	{
		string tags = Server.tags;
		if (tags.Contains("monthly"))
		{
			wipeFrequency = WipeFrequency.Monthly;
		}
		else if (tags.Contains("biweekly"))
		{
			wipeFrequency = WipeFrequency.BiWeekly;
		}
		else if (tags.Contains("weekly"))
		{
			wipeFrequency = WipeFrequency.Weekly;
		}
		else
		{
			wipeFrequency = WipeFrequency.Monthly;
		}
	}

	public void TryAndUpdate()
	{
		if (Server.tags != oldTags)
		{
			RecalculateWipeFrequency();
			oldTags = Server.tags;
		}
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk && info.msg.landmine == null)
		{
			info.msg.landmine = Facepunch.Pool.Get<ProtoBuf.Landmine>();
			info.msg.landmine.triggeredID = (ulong)GetTicksUntilWipe();
		}
	}

	public TimeSpan GetTimeSpanUntilWipe()
	{
		DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddDays(daysToAddTest).AddHours(hoursToAddTest);
		return GetWipeTime(dateTimeOffset) - dateTimeOffset;
	}

	public long GetTicksUntilWipe()
	{
		return GetTimeSpanUntilWipe().Ticks;
	}

	[ServerVar]
	public static void PrintWipe(ConsoleSystem.Arg arg)
	{
		if (serverinstance == null)
		{
			arg.ReplyWith("WipeTimer not found!");
			return;
		}
		serverinstance.RecalculateWipeFrequency();
		serverinstance.TryAndUpdate();
		TimeZoneInfo timeZone = GetTimeZone();
		string ianaTimeZoneName;
		string text = (TZConvert.TryWindowsToIana(timeZone.Id, out ianaTimeZoneName) ? ianaTimeZoneName : timeZone.Id);
		DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddDays(daysToAddTest).AddHours(hoursToAddTest);
		DateTimeOffset wipeTime = serverinstance.GetWipeTime(dateTimeOffset);
		TimeSpan timeSpan = wipeTime - dateTimeOffset;
		string cronString = GetCronString(serverinstance.wipeFrequency, serverinstance.useWipeDayOverride ? ((int)serverinstance.wipeDayOfWeekOverride) : wipeDayOfWeek, wipeHourOfDay);
		CronExpression cronExpression = GetCronExpression(serverinstance.wipeFrequency, serverinstance.useWipeDayOverride ? ((int)serverinstance.wipeDayOfWeekOverride) : wipeDayOfWeek, wipeHourOfDay);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Frequency: {serverinstance.wipeFrequency}");
		stringBuilder.AppendLine("Timezone: " + timeZone.StandardName + " (ID=" + timeZone.Id + ", IANA=" + text + ")");
		stringBuilder.AppendLine($"Wipe day of week: {(DayOfWeek)wipeDayOfWeek}");
		stringBuilder.AppendLine($"Wipe hour: {wipeHourOfDay}");
		stringBuilder.AppendLine($"Test time: {dateTimeOffset:O}");
		stringBuilder.AppendLine($"Wipe time: {wipeTime:O}");
		stringBuilder.AppendLine($"Time until wipe: {timeSpan:g}");
		stringBuilder.AppendLine($"Ticks until wipe: {timeSpan.Ticks}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Cron: " + cronString);
		stringBuilder.AppendLine("Next 10 occurrences:");
		int num = 0;
		foreach (DateTimeOffset item in cronExpression.GetOccurrences(dateTimeOffset, dateTimeOffset.AddYears(2), timeZone).Take(10))
		{
			stringBuilder.AppendLine($"  {num}. {item:O}");
			num++;
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar]
	public static void PrintTimeZones(ConsoleSystem.Arg arg)
	{
		List<string> systemTzs = (from z in TimeZoneInfo.GetSystemTimeZones()
			select z.Id).ToList();
		IReadOnlyCollection<string> knownWindowsTimeZoneIds = TZConvert.KnownWindowsTimeZoneIds;
		IReadOnlyCollection<string> knownIanaTimeZoneNames = TZConvert.KnownIanaTimeZoneNames;
		arg.ReplyWith(JsonConvert.SerializeObject(new
		{
			systemTzs = systemTzs,
			windowsTzs = knownWindowsTimeZoneIds,
			ianaTzs = knownIanaTimeZoneNames
		}));
	}

	public DateTimeOffset GetWipeTime(DateTimeOffset nowTime)
	{
		if (wipeUnixTimestampOverride > 0 && wipeUnixTimestampOverride > Epoch.Current)
		{
			return Epoch.ToDateTime(wipeUnixTimestampOverride);
		}
		try
		{
			return GetCronExpression(wipeFrequency, useWipeDayOverride ? ((int)wipeDayOfWeekOverride) : wipeDayOfWeek, wipeHourOfDay).GetNextOccurrence(nowTime, GetTimeZone()) ?? DateTimeOffset.MaxValue;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return DateTimeOffset.MaxValue;
		}
	}

	private static CronExpression GetCronExpression(WipeFrequency frequency, int dayOfWeek, float hourOfDay)
	{
		string cronString = GetCronString(frequency, dayOfWeek, hourOfDay);
		if (cronString == cronExprCacheKey && cronExprCache != null)
		{
			return cronExprCache;
		}
		cronExprCache = CronExpression.Parse(cronString);
		cronExprCacheKey = cronString;
		return cronExprCache;
	}

	private static string GetCronString(WipeFrequency frequency, int dayOfWeek, float hourOfDay)
	{
		if (!string.IsNullOrWhiteSpace(wipeCronOverride))
		{
			return wipeCronOverride;
		}
		(WipeFrequency, int, float) tuple = (frequency, dayOfWeek, hourOfDay);
		(WipeFrequency, int, float) tuple2 = tuple;
		(WipeFrequency, int, float)? tuple3 = cronCacheKey;
		if (tuple3.HasValue)
		{
			(WipeFrequency, int, float) valueOrDefault = tuple3.GetValueOrDefault();
			if (tuple2.Item1 == valueOrDefault.Item1 && tuple2.Item2 == valueOrDefault.Item2 && tuple2.Item3 == valueOrDefault.Item3 && cronCache != null)
			{
				return cronCache;
			}
		}
		cronCache = BuildCronString(frequency, dayOfWeek, hourOfDay);
		cronCacheKey = tuple;
		return cronCache;
	}

	private static string BuildCronString(WipeFrequency frequency, int dayOfWeek, float hourOfDay)
	{
		int num = Mathf.FloorToInt(hourOfDay);
		int num2 = Mathf.FloorToInt((hourOfDay - (float)num) * 60f);
		return frequency switch
		{
			WipeFrequency.Weekly => $"{num2} {num} * * {dayOfWeek}", 
			WipeFrequency.BiWeekly => $"{num2} {num} 1-7,15-21,29-31 * {dayOfWeek}", 
			WipeFrequency.Monthly => $"{num2} {num} * * {dayOfWeek}#1", 
			_ => throw new NotSupportedException($"WipeFrequency {frequency}"), 
		};
	}

	private static TimeZoneInfo GetTimeZone()
	{
		if (string.IsNullOrWhiteSpace(wipeTimezone))
		{
			return TimeZoneInfo.Local;
		}
		if (wipeTimezone == timezoneCacheKey && timezoneCache != null)
		{
			return timezoneCache;
		}
		if (TZConvert.TryGetTimeZoneInfo(wipeTimezone, out timezoneCache))
		{
			timezoneCacheKey = wipeTimezone;
			return timezoneCache;
		}
		return TimeZoneInfo.Local;
	}
}
