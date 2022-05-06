using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public static class TelephoneManager
{
	public const int MaxPhoneNumber = 99990000;

	public const int MinPhoneNumber = 10000000;

	[ServerVar]
	public static int MaxConcurrentCalls = 10;

	[ServerVar]
	public static int MaxCallLength = 120;

	public static Dictionary<int, PhoneController> allTelephones = new Dictionary<int, PhoneController>();

	public static int maxAssignedPhoneNumber = 99990000;

	public static int GetUnusedTelephoneNumber()
	{
		int num = Random.Range(10000000, 99990000);
		int num2 = 0;
		int num3 = 1000;
		while (allTelephones.ContainsKey(num) && num2 < num3)
		{
			num2++;
			num = Random.Range(10000000, 99990000);
		}
		if (num2 == num3)
		{
			num = maxAssignedPhoneNumber + 1;
		}
		maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, num);
		return num;
	}

	public static void RegisterTelephone(PhoneController t, bool checkPhoneNumber = false)
	{
		if (checkPhoneNumber && allTelephones.ContainsKey(t.PhoneNumber) && allTelephones[t.PhoneNumber] != t)
		{
			t.PhoneNumber = GetUnusedTelephoneNumber();
		}
		if (!allTelephones.ContainsKey(t.PhoneNumber) && t.PhoneNumber != 0)
		{
			allTelephones.Add(t.PhoneNumber, t);
			maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, t.PhoneNumber);
		}
	}

	public static void DeregisterTelephone(PhoneController t)
	{
		if (allTelephones.ContainsKey(t.PhoneNumber))
		{
			allTelephones.Remove(t.PhoneNumber);
		}
	}

	public static PhoneController GetTelephone(int number)
	{
		if (allTelephones.ContainsKey(number))
		{
			return allTelephones[number];
		}
		return null;
	}

	public static PhoneController GetRandomTelephone(int ignoreNumber)
	{
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.PhoneNumber != ignoreNumber)
			{
				return allTelephone.Value;
			}
		}
		return null;
	}

	public static int GetCurrentActiveCalls()
	{
		int num = 0;
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.serverState != 0)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return 0;
		}
		return num / 2;
	}

	public static void GetPhoneDirectory(int ignoreNumber, int page, int perPage, PhoneDirectory directory)
	{
		directory.entries = Pool.GetList<PhoneDirectory.DirectoryEntry>();
		int num = page * perPage;
		int num2 = 0;
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Key == ignoreNumber || string.IsNullOrEmpty(allTelephone.Value.PhoneName))
			{
				continue;
			}
			num2++;
			if (num2 >= num)
			{
				PhoneDirectory.DirectoryEntry directoryEntry = Pool.Get<PhoneDirectory.DirectoryEntry>();
				directoryEntry.phoneName = allTelephone.Value.GetDirectoryName();
				directoryEntry.phoneNumber = allTelephone.Value.PhoneNumber;
				directory.entries.Add(directoryEntry);
				if (directory.entries.Count >= perPage)
				{
					directory.atEnd = false;
					return;
				}
			}
		}
		directory.atEnd = true;
	}

	[ServerVar]
	public static void PrintAllPhones(ConsoleSystem.Arg arg)
	{
		TextTable textTable = new TextTable();
		textTable.AddColumns("Number", "Name", "Position");
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			Vector3 position = allTelephone.Value.transform.position;
			textTable.AddRow(allTelephone.Key.ToString(), allTelephone.Value.GetDirectoryName(), $"{position.x} {position.y} {position.z}");
		}
		arg.ReplyWith(textTable.ToString());
	}
}
