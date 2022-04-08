using System.Collections.Generic;
using Facepunch;

namespace UnityEngine;

public static class CoroutineEx
{
	public static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	private static Dictionary<float, WaitForSeconds> waitForSecondsBuffer = new Dictionary<float, WaitForSeconds>();

	public static WaitForSeconds waitForSeconds(float seconds)
	{
		if (!waitForSecondsBuffer.TryGetValue(seconds, out var value))
		{
			value = new WaitForSeconds(seconds);
			waitForSecondsBuffer.Add(seconds, value);
		}
		return value;
	}

	public static WaitForSecondsRealtimeEx waitForSecondsRealtime(float seconds)
	{
		WaitForSecondsRealtimeEx waitForSecondsRealtimeEx = Pool.Get<WaitForSecondsRealtimeEx>();
		waitForSecondsRealtimeEx.WaitTime = seconds;
		return waitForSecondsRealtimeEx;
	}
}
