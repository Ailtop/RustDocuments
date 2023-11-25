using UnityEngine;

namespace ConVar;

[Factory("time")]
public class Time : ConsoleSystem
{
	[ServerVar]
	[Help("Pause time while loading")]
	public static bool pausewhileloading = true;

	[ServerVar]
	[Help("Fixed delta time in seconds")]
	public static float fixeddelta
	{
		get
		{
			return UnityEngine.Time.fixedDeltaTime;
		}
		set
		{
			UnityEngine.Time.fixedDeltaTime = value;
		}
	}

	[ServerVar]
	[Help("The minimum amount of times to tick per frame")]
	public static float maxdelta
	{
		get
		{
			return UnityEngine.Time.maximumDeltaTime;
		}
		set
		{
			UnityEngine.Time.maximumDeltaTime = value;
		}
	}

	[ServerVar]
	[Help("The time scale")]
	public static float timescale
	{
		get
		{
			return UnityEngine.Time.timeScale;
		}
		set
		{
			UnityEngine.Time.timeScale = value;
		}
	}
}
