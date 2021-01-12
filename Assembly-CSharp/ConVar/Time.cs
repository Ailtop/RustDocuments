using UnityEngine;

namespace ConVar
{
	[Factory("time")]
	public class Time : ConsoleSystem
	{
		[Help("Pause time while loading")]
		[ServerVar]
		public static bool pausewhileloading = true;

		[Help("Fixed delta time in seconds")]
		[ServerVar]
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

		[Help("The minimum amount of times to tick per frame")]
		[ServerVar]
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

		[Help("The time scale")]
		[ServerVar]
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
}
