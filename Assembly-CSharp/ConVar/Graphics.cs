using Rust.Workshop;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConVar;

[Factory("graphics")]
public class Graphics : ConsoleSystem
{
	private const float MinShadowDistance = 100f;

	private const float MaxShadowDistance2Split = 600f;

	private const float MaxShadowDistance4Split = 1000f;

	private static float _shadowdistance = 1000f;

	[ClientVar(Saved = true)]
	public static int shadowmode = 2;

	[ClientVar(Saved = true)]
	public static int shadowlights = 1;

	private static int _shadowquality = 1;

	[ClientVar(Saved = true)]
	public static bool grassshadows = false;

	[ClientVar(Saved = true)]
	public static bool contactshadows = false;

	[ClientVar(Saved = true)]
	public static float drawdistance = 2500f;

	private static float _fov = 75f;

	[ClientVar]
	public static bool hud = true;

	[ClientVar(Saved = true)]
	public static bool chat = true;

	[ClientVar(Saved = true)]
	public static bool branding = true;

	[ClientVar(Saved = true)]
	public static int compass = 1;

	[ClientVar(Saved = true)]
	public static bool dof = false;

	[ClientVar(Saved = true)]
	public static float dof_aper = 12f;

	[ClientVar(Saved = true)]
	public static float dof_blur = 1f;

	[ClientVar(Saved = true, Help = "0 = auto 1 = manual 2 = dynamic based on target")]
	public static int dof_mode = 0;

	[ClientVar(Saved = true, Help = "distance from camera to focus on")]
	public static float dof_focus_dist = 10f;

	[ClientVar(Saved = true)]
	public static float dof_focus_time = 0.2f;

	[ClientVar(Saved = true, ClientAdmin = true)]
	public static bool dof_debug = false;

	public static BaseEntity dof_focus_target_entity = null;

	[ClientVar(Saved = true, Help = "Whether to scale vm models with fov")]
	public static bool vm_fov_scale = true;

	[ClientVar(Saved = true, Help = "FLips viewmodels horizontally (for left handed players)")]
	public static bool vm_horizontal_flip = false;

	private static float _uiscale = 1f;

	private static int _anisotropic = 1;

	private static int _parallax = 0;

	[ClientVar(Help = "The currently selected quality level")]
	public static int quality
	{
		get
		{
			return QualitySettings.GetQualityLevel();
		}
		set
		{
			int num = shadowcascades;
			QualitySettings.SetQualityLevel(value, applyExpensiveChanges: true);
			shadowcascades = num;
		}
	}

	[ClientVar(Saved = true)]
	public static float shadowdistance
	{
		get
		{
			return _shadowdistance;
		}
		set
		{
			_shadowdistance = value;
			QualitySettings.shadowDistance = EnforceShadowDistanceBounds(_shadowdistance);
		}
	}

	[ClientVar(Saved = true)]
	public static int shadowcascades
	{
		get
		{
			return QualitySettings.shadowCascades;
		}
		set
		{
			QualitySettings.shadowCascades = value;
			QualitySettings.shadowDistance = EnforceShadowDistanceBounds(shadowdistance);
		}
	}

	[ClientVar(Saved = true)]
	public static int shadowquality
	{
		get
		{
			return _shadowquality;
		}
		set
		{
			_shadowquality = Mathf.Clamp(value, 0, 3);
			shadowmode = _shadowquality + 1;
			bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
			KeywordUtil.EnsureKeywordState("SHADOW_QUALITY_HIGH", !flag && _shadowquality == 2);
			KeywordUtil.EnsureKeywordState("SHADOW_QUALITY_VERYHIGH", !flag && _shadowquality == 3);
		}
	}

	[ClientVar(Saved = true)]
	public static float fov
	{
		get
		{
			return _fov;
		}
		set
		{
			_fov = Mathf.Clamp(value, 70f, 90f);
		}
	}

	[ClientVar]
	public static float lodbias
	{
		get
		{
			return QualitySettings.lodBias;
		}
		set
		{
			QualitySettings.lodBias = Mathf.Clamp(value, 0.25f, 5f);
		}
	}

	[ClientVar(Saved = true)]
	public static int shaderlod
	{
		get
		{
			return Shader.globalMaximumLOD;
		}
		set
		{
			Shader.globalMaximumLOD = Mathf.Clamp(value, 100, 600);
		}
	}

	[ClientVar(Saved = true)]
	public static float uiscale
	{
		get
		{
			return _uiscale;
		}
		set
		{
			_uiscale = Mathf.Clamp(value, 0.5f, 1f);
		}
	}

	[ClientVar(Saved = true)]
	public static int af
	{
		get
		{
			return _anisotropic;
		}
		set
		{
			value = Mathf.Clamp(value, 1, 16);
			UnityEngine.Texture.SetGlobalAnisotropicFilteringLimits(1, value);
			if (value <= 1)
			{
				UnityEngine.Texture.anisotropicFiltering = AnisotropicFiltering.Disable;
			}
			if (value > 1)
			{
				UnityEngine.Texture.anisotropicFiltering = AnisotropicFiltering.Enable;
			}
			_anisotropic = value;
		}
	}

	[ClientVar(Saved = true)]
	public static int parallax
	{
		get
		{
			return _parallax;
		}
		set
		{
			switch (value)
			{
			default:
				Shader.DisableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.DisableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			case 1:
				Shader.EnableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.DisableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			case 2:
				Shader.DisableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.EnableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			}
			_parallax = value;
		}
	}

	[ClientVar]
	public static bool itemskins
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.AllowApply;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.AllowApply = value;
		}
	}

	[ClientVar]
	public static bool itemskinunload
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.AllowUnload;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.AllowUnload = value;
		}
	}

	[ClientVar]
	public static float itemskintimeout
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.DownloadTimeout;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.DownloadTimeout = value;
		}
	}

	public static float EnforceShadowDistanceBounds(float distance)
	{
		distance = ((QualitySettings.shadowCascades == 1) ? Mathf.Clamp(distance, 100f, 100f) : ((QualitySettings.shadowCascades != 2) ? Mathf.Clamp(distance, 100f, 1000f) : Mathf.Clamp(distance, 100f, 600f)));
		return distance;
	}

	[ClientVar(ClientAdmin = true)]
	public static void dof_focus_target(Arg arg)
	{
	}

	[ClientVar]
	public static void dof_nudge(Arg arg)
	{
		float @float = arg.GetFloat(0);
		dof_focus_dist += @float;
		if (dof_focus_dist < 0f)
		{
			dof_focus_dist = 0f;
		}
	}
}
