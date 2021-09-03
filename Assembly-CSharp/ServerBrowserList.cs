using System;
using Facepunch;
using UnityEngine;

public class ServerBrowserList : BaseMonoBehaviour, VirtualScroll.IDataSource
{
	public enum QueryType
	{
		RegularInternet,
		Friends,
		History,
		LAN,
		Favourites,
		None
	}

	[Serializable]
	public struct ServerKeyvalues
	{
		public string key;

		public string value;
	}

	[Serializable]
	public struct Rules
	{
		public string tag;

		public ServerBrowserList serverList;
	}

	public QueryType queryType;

	public static string VersionTag = "v" + 2314;

	public ServerKeyvalues[] keyValues = new ServerKeyvalues[0];

	public ServerBrowserCategory categoryButton;

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

	public int GetItemCount()
	{
		return 0;
	}

	public void SetItemData(int i, GameObject obj)
	{
	}
}
