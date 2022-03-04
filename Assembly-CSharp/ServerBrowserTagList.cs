using Facepunch;
using UnityEngine;

public class ServerBrowserTagList : MonoBehaviour
{
	private ServerBrowserTag[] _allTags;

	private void Initialize()
	{
		if (_allTags == null)
		{
			_allTags = GetComponentsInChildren<ServerBrowserTag>();
		}
	}

	public void Awake()
	{
		Initialize();
	}

	public bool Refresh(ServerInfo server)
	{
		Initialize();
		int num = 0;
		ServerBrowserTag[] allTags = _allTags;
		foreach (ServerBrowserTag serverBrowserTag in allTags)
		{
			if (num < 3 && serverBrowserTag.Test(ref server))
			{
				serverBrowserTag.SetActive(true);
				num++;
			}
			else
			{
				serverBrowserTag.SetActive(false);
			}
		}
		return num > 0;
	}
}
