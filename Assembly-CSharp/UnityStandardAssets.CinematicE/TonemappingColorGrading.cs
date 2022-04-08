using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Cinematic/Tonemapping and Color Grading")]
	[ImageEffectAllowedInSceneView]
	public class TonemappingColorGrading : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		public class IndentedGroup : PropertyAttribute
		{
		}

		public class ChannelMixer : PropertyAttribute
		{
		}

		public class ColorWheelGroup : PropertyAttribute
		{
			public int minSizePerWheel = 60;

			public int maxSizePerWheel = 150;

			public ColorWheelGroup()
			{
			}

			public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
			{
				this.minSizePerWheel = minSizePerWheel;
				this.maxSizePerWheel = maxSizePerWheel;
			}
		}

		public class Curve : PropertyAttribute
		{
			public Color color = Color.white;

			public Curve()
			{
			}

			public Curve(float r, float g, float b, float a)
			{
				color = new Color(r, g, b, a);
			}
		}

		[Serializable]
		public struct EyeAdaptationSettings
		{
			public bool enabled;

			[Min(0f)]
			[Tooltip("Midpoint Adjustment.")]
			public float middleGrey;

			[Tooltip("The lowest possible exposure value; adjust this value to modify the brightest areas of your level.")]
			public float min;

			[Tooltip("The highest possible exposure value; adjust this value to modify the darkest areas of your level.")]
			public float max;

			[Min(0f)]
			[Tooltip("Speed of linear adaptation. Higher is faster.")]
			public float speed;

			[Tooltip("Displays a luminosity helper in the GameView.")]
			public bool showDebug;

			public static EyeAdaptationSettings defaultSettings
			{
				get
				{
					EyeAdaptationSettings result = default(EyeAdaptationSettings);
					result.enabled = false;
					result.showDebug = false;
					result.middleGrey = 0.5f;
					result.min = -3f;
					result.max = 3f;
					result.speed = 1.5f;
					return result;
				}
			}
		}

		public enum Tonemapper
		{
			ACES = 0,
			Curve = 1,
			Hable = 2,
			HejlDawson = 3,
			Photographic = 4,
			Reinhard = 5,
			Neutral = 6
		}

		[Serializable]
		public struct TonemappingSettings
		{
			public bool enabled;

			[Tooltip("Tonemapping technique to use. ACES is the recommended one.")]
			public Tonemapper tonemapper;

			[Min(0f)]
			[Tooltip("Adjusts the overall exposure of the scene.")]
			public float exposure;

			[Tooltip("Custom tonemapping curve.")]
			public AnimationCurve curve;

			[Range(-0.1f, 0.1f)]
			public float neutralBlackIn;

			[Range(1f, 20f)]
			public float neutralWhiteIn;

			[Range(-0.09f, 0.1f)]
			public float neutralBlackOut;

			[Range(1f, 19f)]
			public float neutralWhiteOut;

			[Range(0.1f, 20f)]
			public float neutralWhiteLevel;

			[Range(1f, 10f)]
			public float neutralWhiteClip;

			public static TonemappingSettings defaultSettings
			{
				get
				{
					TonemappingSettings result = default(TonemappingSettings);
					result.enabled = false;
					result.tonemapper = Tonemapper.Neutral;
					result.exposure = 1f;
					result.curve = CurvesSettings.defaultCurve;
					result.neutralBlackIn = 0.02f;
					result.neutralWhiteIn = 10f;
					result.neutralBlackOut = 0f;
					result.neutralWhiteOut = 10f;
					result.neutralWhiteLevel = 5.3f;
					result.neutralWhiteClip = 10f;
					return result;
				}
			}
		}

		[Serializable]
		public struct LUTSettings
		{
			public bool enabled;

			[Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
			public Texture texture;

			[Range(0f, 1f)]
			[Tooltip("Blending factor.")]
			public float contribution;

			public static LUTSettings defaultSettings
			{
				get
				{
					LUTSettings result = default(LUTSettings);
					result.enabled = false;
					result.texture = null;
					result.contribution = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ColorWheelsSettings
		{
			[ColorUsage(false)]
			public Color shadows;

			[ColorUsage(false)]
			public Color midtones;

			[ColorUsage(false)]
			public Color highlights;

			public static ColorWheelsSettings defaultSettings
			{
				get
				{
					ColorWheelsSettings result = default(ColorWheelsSettings);
					result.shadows = Color.white;
					result.midtones = Color.white;
					result.highlights = Color.white;
					return result;
				}
			}
		}

		[Serializable]
		public struct BasicsSettings
		{
			[Range(-2f, 2f)]
			[Tooltip("Sets the white balance to a custom color temperature.")]
			public float temperatureShift;

			[Range(-2f, 2f)]
			[Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
			public float tint;

			[Tooltip("Shift the hue of all colors.")]
			[Space]
			[Range(-0.5f, 0.5f)]
			public float hue;

			[Range(0f, 2f)]
			[Tooltip("Pushes the intensity of all colors.")]
			public float saturation;

			[Range(-1f, 1f)]
			[Tooltip("Adjusts the saturation so that clipping is minimized as colors approach full saturation.")]
			public float vibrance;

			[Range(0f, 10f)]
			[Tooltip("Brightens or darkens all colors.")]
			public float value;

			[Space]
			[Range(0f, 2f)]
			[Tooltip("Expands or shrinks the overall range of tonal values.")]
			public float contrast;

			[Range(0.01f, 5f)]
			[Tooltip("Contrast gain curve. Controls the steepness of the curve.")]
			public float gain;

			[Range(0.01f, 5f)]
			[Tooltip("Applies a pow function to the source.")]
			public float gamma;

			public static BasicsSettings defaultSettings
			{
				get
				{
					BasicsSettings result = default(BasicsSettings);
					result.temperatureShift = 0f;
					result.tint = 0f;
					result.contrast = 1f;
					result.hue = 0f;
					result.saturation = 1f;
					result.value = 1f;
					result.vibrance = 0f;
					result.gain = 1f;
					result.gamma = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ChannelMixerSettings
		{
			public int currentChannel;

			public Vector3[] channels;

			public static ChannelMixerSettings defaultSettings
			{
				get
				{
					ChannelMixerSettings result = default(ChannelMixerSettings);
					result.currentChannel = 0;
					result.channels = new Vector3[3]
					{
						new Vector3(1f, 0f, 0f),
						new Vector3(0f, 1f, 0f),
						new Vector3(0f, 0f, 1f)
					};
					return result;
				}
			}
		}

		[Serializable]
		public struct CurvesSettings
		{
			[Curve]
			public AnimationCurve master;

			[Curve(1f, 0f, 0f, 1f)]
			public AnimationCurve red;

			[Curve(0f, 1f, 0f, 1f)]
			public AnimationCurve green;

			[Curve(0f, 1f, 1f, 1f)]
			public AnimationCurve blue;

			public static CurvesSettings defaultSettings
			{
				get
				{
					CurvesSettings result = default(CurvesSettings);
					result.master = defaultCurve;
					result.red = defaultCurve;
					result.green = defaultCurve;
					result.blue = defaultCurve;
					return result;
				}
			}

			public static AnimationCurve defaultCurve => new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
		}

		public enum ColorGradingPrecision
		{
			Normal = 0x10,
			High = 0x20
		}

		[Serializable]
		public struct ColorGradingSettings
		{
			public bool enabled;

			[Tooltip("Internal LUT precision. \"Normal\" is 256x16, \"High\" is 1024x32. Prefer \"Normal\" on mobile devices.")]
			public ColorGradingPrecision precision;

			[Space]
			[ColorWheelGroup]
			public ColorWheelsSettings colorWheels;

			[Space]
			[IndentedGroup]
			public BasicsSettings basics;

			[Space]
			[ChannelMixer]
			public ChannelMixerSettings channelMixer;

			[Space]
			[IndentedGroup]
			public CurvesSettings curves;

			[Space]
			[Tooltip("Use dithering to try and minimize color banding in dark areas.")]
			public bool useDithering;

			[Tooltip("Displays the generated LUT in the top left corner of the GameView.")]
			public bool showDebug;

			public static ColorGradingSettings defaultSettings
			{
				get
				{
					ColorGradingSettings result = default(ColorGradingSettings);
					result.enabled = false;
					result.useDithering = false;
					result.showDebug = false;
					result.precision = ColorGradingPrecision.Normal;
					result.colorWheels = ColorWheelsSettings.defaultSettings;
					result.basics = BasicsSettings.defaultSettings;
					result.channelMixer = ChannelMixerSettings.defaultSettings;
					result.curves = CurvesSettings.defaultSettings;
					return result;
				}
			}

			internal void Reset()
			{
				curves = CurvesSettings.defaultSettings;
			}
		}

		[SerializeField]
		[SettingsGroup]
		private EyeAdaptationSettings m_EyeAdaptation = EyeAdaptationSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private TonemappingSettings m_Tonemapping = TonemappingSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private ColorGradingSettings m_ColorGrading = ColorGradingSettings.defaultSettings;

		[SerializeField]
		[SettingsGroup]
		private LUTSettings m_Lut = LUTSettings.defaultSettings;

		[SerializeField]
		private Shader m_Shader;
	}
}
