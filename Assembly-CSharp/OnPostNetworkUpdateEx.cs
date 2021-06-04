using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class OnPostNetworkUpdateEx
{
	public static void BroadcastOnPostNetworkUpdate(this GameObject go, BaseEntity entity)
	{
		List<IOnPostNetworkUpdate> obj = Pool.GetList<IOnPostNetworkUpdate>();
		go.GetComponentsInChildren<IOnPostNetworkUpdate>(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnPostNetworkUpdate(entity);
		}
		Pool.FreeList(ref obj);
	}

	public static void SendOnPostNetworkUpdate(this GameObject go, BaseEntity entity)
	{
		List<IOnPostNetworkUpdate> obj = Pool.GetList<IOnPostNetworkUpdate>();
		go.GetComponents<IOnPostNetworkUpdate>(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnPostNetworkUpdate(entity);
		}
		Pool.FreeList(ref obj);
	}
}
