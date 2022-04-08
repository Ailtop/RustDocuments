using System;

namespace ConVar;

[Factory("env")]
public class Env : ConsoleSystem
{
	[ServerVar]
	public static bool progresstime
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return false;
			}
			return TOD_Sky.Instance.Components.Time.ProgressTime;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Components.Time.ProgressTime = value;
			}
		}
	}

	[ServerVar(ShowInAdminUI = true)]
	public static float time
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0f;
			}
			return TOD_Sky.Instance.Cycle.Hour;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Hour = value;
			}
		}
	}

	[ServerVar]
	public static int day
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Day;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Day = value;
			}
		}
	}

	[ServerVar]
	public static int month
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Month;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Month = value;
			}
		}
	}

	[ServerVar]
	public static int year
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Year;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Year = value;
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float oceanlevel
	{
		get
		{
			return WaterSystem.OceanLevel;
		}
		set
		{
			WaterSystem.OceanLevel = value;
		}
	}

	[ServerVar]
	public static void addtime(Arg arg)
	{
		if (!(TOD_Sky.Instance == null))
		{
			DateTime dateTime = TOD_Sky.Instance.Cycle.DateTime.AddTicks(arg.GetTicks(0, 0L));
			TOD_Sky.Instance.Cycle.DateTime = dateTime;
		}
	}
}
