using System;
using System.Globalization;
using UnityEngine;

namespace ConVar;

[Factory("weather")]
public class Weather : ConsoleSystem
{
	[ServerVar]
	public static float wetness_rain = 0.4f;

	[ServerVar]
	public static float wetness_snow = 0.2f;

	[ReplicatedVar(Default = "1")]
	public static float clear_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 1f;
			}
			return SingletonComponent<Climate>.Instance.Weather.ClearChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.ClearChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float dust_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.DustChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.DustChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float fog_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.FogChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.FogChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float overcast_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.OvercastChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.OvercastChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float storm_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.StormChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.StormChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float rain_chance
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.RainChance;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.Weather.RainChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float rain
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Rain;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Rain = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float wind
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Wind;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Wind = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float thunder
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Thunder;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Thunder = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float rainbow
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Rainbow;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Rainbow = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float fog
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Fogginess;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Fogginess = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_rayleigh
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.RayleighMultiplier;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.RayleighMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_mie
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.MieMultiplier;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.MieMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_brightness
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Brightness;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Brightness = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_contrast
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Contrast;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Contrast = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_directionality
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Directionality;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Directionality = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_size
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Size;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Size = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_opacity
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Opacity;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Opacity = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_coverage
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coverage;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coverage = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_sharpness
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Sharpness;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Sharpness = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_coloring
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coloring;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coloring = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_attenuation
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Attenuation;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Attenuation = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_saturation
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Saturation;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Saturation = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_scattering
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Scattering;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Scattering = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_brightness
	{
		get
		{
			if (!SingletonComponent<Climate>.Instance)
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Brightness;
		}
		set
		{
			if ((bool)SingletonComponent<Climate>.Instance)
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Brightness = value;
			}
		}
	}

	[ClientVar]
	[ServerVar]
	public static void load(Arg args)
	{
		if (!SingletonComponent<Climate>.Instance)
		{
			return;
		}
		string name = args.GetString(0);
		if (string.IsNullOrEmpty(name))
		{
			args.ReplyWith("Weather preset name invalid.");
			return;
		}
		WeatherPreset weatherPreset = Array.Find(SingletonComponent<Climate>.Instance.WeatherPresets, (WeatherPreset x) => x.name.Contains(name, CompareOptions.IgnoreCase));
		if (weatherPreset == null)
		{
			args.ReplyWith("Weather preset not found: " + name);
			return;
		}
		SingletonComponent<Climate>.Instance.WeatherOverrides.Set(weatherPreset);
		if (args.IsServerside)
		{
			ServerMgr.SendReplicatedVars("weather.");
		}
	}

	[ClientVar]
	[ServerVar]
	public static void reset(Arg args)
	{
		if ((bool)SingletonComponent<Climate>.Instance)
		{
			SingletonComponent<Climate>.Instance.WeatherOverrides.Reset();
			if (args.IsServerside)
			{
				ServerMgr.SendReplicatedVars("weather.");
			}
		}
	}

	[ClientVar]
	[ServerVar]
	public static void report(Arg args)
	{
		if ((bool)SingletonComponent<Climate>.Instance)
		{
			TextTable textTable = new TextTable();
			textTable.AddColumn(SingletonComponent<Climate>.Instance.WeatherStatePrevious.name);
			textTable.AddColumn("|");
			textTable.AddColumn(SingletonComponent<Climate>.Instance.WeatherStateTarget.name);
			textTable.AddColumn("|");
			textTable.AddColumn(SingletonComponent<Climate>.Instance.WeatherStateNext.name);
			int num = Mathf.RoundToInt(SingletonComponent<Climate>.Instance.WeatherStateBlend * 100f);
			if (num < 100)
			{
				textTable.AddRow("fading out (" + (100 - num) + "%)", "|", "fading in (" + num + "%)", "|", "up next");
			}
			else
			{
				textTable.AddRow("previous", "|", "current", "|", "up next");
			}
			args.ReplyWith(textTable.ToString() + Environment.NewLine + SingletonComponent<Climate>.Instance.WeatherState.ToString());
		}
	}
}
