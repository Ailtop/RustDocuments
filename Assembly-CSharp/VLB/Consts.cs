using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

public static class Consts
{
	private const string HelpUrlBase = "http://saladgamer.com/vlb-doc/";

	public const string HelpUrlBeam = "http://saladgamer.com/vlb-doc/comp-lightbeam/";

	public const string HelpUrlDustParticles = "http://saladgamer.com/vlb-doc/comp-dustparticles/";

	public const string HelpUrlDynamicOcclusion = "http://saladgamer.com/vlb-doc/comp-dynocclusion/";

	public const string HelpUrlTriggerZone = "http://saladgamer.com/vlb-doc/comp-triggerzone/";

	public const string HelpUrlConfig = "http://saladgamer.com/vlb-doc/config/";

	public static readonly bool ProceduralObjectsVisibleInEditor = true;

	public static readonly Color FlatColor = Color.white;

	public const ColorMode ColorModeDefault = ColorMode.Flat;

	public const float Alpha = 1f;

	public const float SpotAngleDefault = 35f;

	public const float SpotAngleMin = 0.1f;

	public const float SpotAngleMax = 179.9f;

	public const float ConeRadiusStart = 0.1f;

	public const MeshType GeomMeshType = MeshType.Shared;

	public const int GeomSidesDefault = 18;

	public const int GeomSidesMin = 3;

	public const int GeomSidesMax = 256;

	public const int GeomSegmentsDefault = 5;

	public const int GeomSegmentsMin = 0;

	public const int GeomSegmentsMax = 64;

	public const bool GeomCap = false;

	public const AttenuationEquation AttenuationEquationDefault = AttenuationEquation.Quadratic;

	public const float AttenuationCustomBlending = 0.5f;

	public const float FadeStart = 0f;

	public const float FadeEnd = 3f;

	public const float FadeMinThreshold = 0.01f;

	public const float DepthBlendDistance = 2f;

	public const float CameraClippingDistance = 0.5f;

	public const float FresnelPowMaxValue = 10f;

	public const float FresnelPow = 8f;

	public const float GlareFrontal = 0.5f;

	public const float GlareBehind = 0.5f;

	public const float NoiseIntensityMin = 0f;

	public const float NoiseIntensityMax = 1f;

	public const float NoiseIntensityDefault = 0.5f;

	public const float NoiseScaleMin = 0.01f;

	public const float NoiseScaleMax = 2f;

	public const float NoiseScaleDefault = 0.5f;

	public static readonly Vector3 NoiseVelocityDefault = new Vector3(0.07f, 0.18f, 0.05f);

	public const BlendingMode BlendingModeDefault = BlendingMode.Additive;

	public static readonly BlendMode[] BlendingMode_SrcFactor = new BlendMode[3]
	{
		BlendMode.One,
		BlendMode.OneMinusDstColor,
		BlendMode.SrcAlpha
	};

	public static readonly BlendMode[] BlendingMode_DstFactor = new BlendMode[3]
	{
		BlendMode.One,
		BlendMode.One,
		BlendMode.OneMinusSrcAlpha
	};

	public static readonly bool[] BlendingMode_AlphaAsBlack = new bool[3] { true, true, false };

	public const float DynOcclusionMinSurfaceRatioDefault = 0.5f;

	public const float DynOcclusionMinSurfaceRatioMin = 50f;

	public const float DynOcclusionMinSurfaceRatioMax = 100f;

	public const float DynOcclusionMaxSurfaceDotDefault = 0.25f;

	public const float DynOcclusionMaxSurfaceAngleMin = 45f;

	public const float DynOcclusionMaxSurfaceAngleMax = 90f;

	public const int ConfigGeometryLayerIDDefault = 1;

	public const string ConfigGeometryTagDefault = "Untagged";

	public const RenderQueue ConfigGeometryRenderQueueDefault = RenderQueue.Transparent;

	public const bool ConfigGeometryForceSinglePassDefault = false;

	public const int ConfigNoise3DSizeDefault = 64;

	public const int ConfigSharedMeshSides = 24;

	public const int ConfigSharedMeshSegments = 5;

	public static HideFlags ProceduralObjectsHideFlags
	{
		get
		{
			if (!ProceduralObjectsVisibleInEditor)
			{
				return HideFlags.HideAndDontSave;
			}
			return HideFlags.DontSave | HideFlags.NotEditable;
		}
	}
}
