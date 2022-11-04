using System;
using UnityEngine;

namespace Rust;

public class GC : MonoBehaviour, IClientComponent
{
	public static bool Enabled => true;

	public static void Collect()
	{
		System.GC.Collect();
	}

	public static long GetTotalMemory()
	{
		return System.GC.GetTotalMemory(forceFullCollection: false) / 1048576;
	}

	public static int CollectionCount()
	{
		return System.GC.CollectionCount(0);
	}
}
