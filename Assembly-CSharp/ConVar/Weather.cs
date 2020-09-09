using UnityEngine;

namespace ConVar
{
	[Factory("weather")]
	public class Weather : ConsoleSystem
	{
		[ServerVar]
		public static void clouds(Arg args)
		{
			if (!(SingletonComponent<Climate>.Instance == null))
			{
				float clouds = SingletonComponent<Climate>.Instance.Overrides.Clouds;
				float @float = args.GetFloat(0, -1f);
				string text = (clouds < 0f) ? "automatic" : (Mathf.RoundToInt(100f * clouds) + "%");
				string text2 = (@float < 0f) ? "automatic" : (Mathf.RoundToInt(100f * @float) + "%");
				args.ReplyWith("Clouds: " + text2 + " (was " + text + ")");
				SingletonComponent<Climate>.Instance.Overrides.Clouds = @float;
			}
		}

		[ServerVar]
		public static void fog(Arg args)
		{
			if (!(SingletonComponent<Climate>.Instance == null))
			{
				float fog = SingletonComponent<Climate>.Instance.Overrides.Fog;
				float @float = args.GetFloat(0, -1f);
				string text = (fog < 0f) ? "automatic" : (Mathf.RoundToInt(100f * fog) + "%");
				string text2 = (@float < 0f) ? "automatic" : (Mathf.RoundToInt(100f * @float) + "%");
				args.ReplyWith("Fog: " + text2 + " (was " + text + ")");
				SingletonComponent<Climate>.Instance.Overrides.Fog = @float;
			}
		}

		[ServerVar]
		public static void wind(Arg args)
		{
			if (!(SingletonComponent<Climate>.Instance == null))
			{
				float wind = SingletonComponent<Climate>.Instance.Overrides.Wind;
				float @float = args.GetFloat(0, -1f);
				string text = (wind < 0f) ? "automatic" : (Mathf.RoundToInt(100f * wind) + "%");
				string text2 = (@float < 0f) ? "automatic" : (Mathf.RoundToInt(100f * @float) + "%");
				args.ReplyWith("Wind: " + text2 + " (was " + text + ")");
				SingletonComponent<Climate>.Instance.Overrides.Wind = @float;
			}
		}

		[ServerVar]
		public static void rain(Arg args)
		{
			if (!(SingletonComponent<Climate>.Instance == null))
			{
				float rain = SingletonComponent<Climate>.Instance.Overrides.Rain;
				float @float = args.GetFloat(0, -1f);
				string text = (rain < 0f) ? "automatic" : (Mathf.RoundToInt(100f * rain) + "%");
				string text2 = (@float < 0f) ? "automatic" : (Mathf.RoundToInt(100f * @float) + "%");
				args.ReplyWith("Rain: " + text2 + " (was " + text + ")");
				SingletonComponent<Climate>.Instance.Overrides.Rain = @float;
			}
		}
	}
}
