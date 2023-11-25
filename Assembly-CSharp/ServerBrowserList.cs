using System;
using Facepunch;
using UnityEngine;

public class ServerBrowserList : ServerBrowserListBase, VirtualScroll.IDataSource, VirtualScroll.IVisualUpdate
{
	[Serializable]
	public struct Rules
	{
		public string tag;

		public ServerBrowserList serverList;
	}

	public enum QueryType
	{
		RegularInternet = 0,
		Friends = 1,
		History = 2,
		LAN = 3,
		Favourites = 4,
		None = 5
	}

	[Serializable]
	public struct ServerKeyvalues
	{
		public string key;

		public string value;
	}

	public bool startActive;

	public Transform listTransform;

	public int refreshOrder;

	public bool UseOfficialServers;

	public VirtualScroll VirtualScroll;

	public Rules[] rules;

	public bool hideOfficialServers;

	public bool excludeEmptyServersUsingQuery;

	public bool alwaysIncludeEmptyServers;

	public bool clampPlayerCountsToTrustedValues;

	public QueryType queryType;

	public static string VersionTag = "v" + 2511;

	public ServerKeyvalues[] keyValues = new ServerKeyvalues[0];

	public int GetItemCount()
	{
		return 0;
	}

	public void OnVisualUpdate(int i, GameObject obj)
	{
	}

	public void SetItemData(int i, GameObject obj)
	{
	}
}
