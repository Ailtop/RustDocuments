using Facepunch;
using System.Collections.Generic;
using UnityEngine;

public static class OnSendNetworkUpdateEx
{
	public static void BroadcastOnSendNetworkUpdate(this GameObject go, BaseEntity entity)
	{
		List<IOnSendNetworkUpdate> obj = Pool.GetList<IOnSendNetworkUpdate>();
		go.GetComponentsInChildren(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnSendNetworkUpdate(entity);
		}
		Pool.FreeList(ref obj);
	}

	public static void SendOnSendNetworkUpdate(this GameObject go, BaseEntity entity)
	{
		List<IOnSendNetworkUpdate> obj = Pool.GetList<IOnSendNetworkUpdate>();
		go.GetComponents(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnSendNetworkUpdate(entity);
		}
		Pool.FreeList(ref obj);
	}
}
