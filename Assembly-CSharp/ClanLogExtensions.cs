using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

public static class ClanLogExtensions
{
	public static ClanLog ToProto(this ClanLogs clanLogs)
	{
		List<ClanLog.Entry> list = Pool.GetList<ClanLog.Entry>();
		foreach (ClanLogEntry entry2 in clanLogs.Entries)
		{
			ClanLog.Entry entry = Pool.Get<ClanLog.Entry>();
			entry.timestamp = entry2.Timestamp;
			entry.eventKey = entry2.EventKey;
			entry.arg1 = entry2.Arg1;
			entry.arg2 = entry2.Arg2;
			entry.arg3 = entry2.Arg3;
			entry.arg4 = entry2.Arg4;
			list.Add(entry);
		}
		ClanLog clanLog = Pool.Get<ClanLog>();
		clanLog.clanId = clanLogs.ClanId;
		clanLog.logEntries = list;
		return clanLog;
	}
}
