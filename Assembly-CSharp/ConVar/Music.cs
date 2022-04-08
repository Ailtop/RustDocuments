using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("music")]
public class Music : ConsoleSystem
{
	[ClientVar]
	public static bool enabled = true;

	[ClientVar]
	public static int songGapMin = 240;

	[ClientVar]
	public static int songGapMax = 480;

	[ClientVar]
	public static void info(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (SingletonComponent<MusicManager>.Instance == null)
		{
			stringBuilder.Append("No music manager was found");
		}
		else
		{
			stringBuilder.Append("Current music info: ");
			stringBuilder.AppendLine();
			stringBuilder.Append("  theme: " + SingletonComponent<MusicManager>.Instance.currentTheme);
			stringBuilder.AppendLine();
			stringBuilder.Append("  intensity: " + SingletonComponent<MusicManager>.Instance.intensity);
			stringBuilder.AppendLine();
			stringBuilder.Append("  next music: " + SingletonComponent<MusicManager>.Instance.nextMusic);
			stringBuilder.AppendLine();
			stringBuilder.Append("  current time: " + UnityEngine.Time.time);
			stringBuilder.AppendLine();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}
}
