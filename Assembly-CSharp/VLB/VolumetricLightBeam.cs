#define UNITY_ASSERTIONS
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam/")]
	[SelectionBase]
	public class VolumetricLightBeam : MonoBehaviour
	{
		public bool colorFromLight = true;

		public ColorMode colorMode;

		[ColorUsage(true, true)]
		[FormerlySerializedAs("colorValue")]
		public Color color = Consts.FlatColor;

		public Gradient colorGradient;

		[Range(0f, 1f)]
		public float alphaInside = 1f;

		[FormerlySerializedAs("alpha")]
		[Range(0f, 1f)]
		public float alphaOutside = 1f;

		public BlendingMode blendingMode;

		[FormerlySerializedAs("angleFromLight")]
		public bool spotAngleFromLight = true;

		[Range(0.1f, 179.9f)]
		public float spotAngle = 35f;

		[FormerlySerializedAs("radiusStart")]
		public float coneRadiusStart = 0.1f;

		public MeshType geomMeshType;

		[FormerlySerializedAs("geomSides")]
		public int geomCustomSides = 18;

		public int geomCustomSegments = 5;

		public bool geomCap;

		public bool fadeEndFromLight = true;

		public AttenuationEquation attenuationEquation = AttenuationEquation.Quadratic;

		[Range(0f, 1f)]
		public float attenuationCustomBlending = 0.5f;

		public float fadeStart;

		public float fadeEnd = 3f;

		public float depthBlendDistance = 2f;

		public float cameraClippingDistance = 0.5f;

		[Range(0f, 1f)]
		public float glareFrontal = 0.5f;

		[Range(0f, 1f)]
		public float glareBehind = 0.5f;

		[Obsolete("Use 'glareFrontal' instead")]
		public float boostDistanceInside = 0.5f;

		[Obsolete("This property has been merged with 'fresnelPow'")]
		public float fresnelPowInside = 6f;

		[FormerlySerializedAs("fresnelPowOutside")]
		public float fresnelPow = 8f;

		public bool noiseEnabled;

		[Range(0f, 1f)]
		public float noiseIntensity = 0.5f;

		public bool noiseScaleUseGlobal = true;

		[Range(0.01f, 2f)]
		public float noiseScaleLocal = 0.5f;

		public bool noiseVelocityUseGlobal = true;

		public Vector3 noiseVelocityLocal = Consts.NoiseVelocityDefault;

		private Plane m_PlaneWS;

		[SerializeField]
		private int pluginVersion = -1;

		[FormerlySerializedAs("trackChangesDuringPlaytime")]
		[SerializeField]
		private bool _TrackChangesDuringPlaytime;

		[SerializeField]
		private int _SortingLayerID;

		[SerializeField]
		private int _SortingOrder;

		private BeamGeometry m_BeamGeom;

		private Coroutine m_CoPlaytimeUpdate;

		private Light _CachedLight;

		public float coneAngle => Mathf.Atan2(coneRadiusEnd - coneRadiusStart, fadeEnd) * 57.29578f * 2f;

		public float coneRadiusEnd => fadeEnd * Mathf.Tan(spotAngle * ((float)Math.PI / 180f) * 0.5f);

		public float coneVolume
		{
			get
			{
				float num = coneRadiusStart;
				float num2 = coneRadiusEnd;
				return (float)Math.PI / 3f * (num * num + num * num2 + num2 * num2) * fadeEnd;
			}
		}

		public float coneApexOffsetZ
		{
			get
			{
				float num = coneRadiusStart / coneRadiusEnd;
				if (num != 1f)
				{
					return fadeEnd * num / (1f - num);
				}
				return float.MaxValue;
			}
		}

		public int geomSides
		{
			get
			{
				if (geomMeshType != MeshType.Custom)
				{
					return Config.Instance.sharedMeshSides;
				}
				return geomCustomSides;
			}
			set
			{
				geomCustomSides = value;
				Debug.LogWarning("The setter VLB.VolumetricLightBeam.geomSides is OBSOLETE and has been renamed to geomCustomSides.");
			}
		}

		public int geomSegments
		{
			get
			{
				if (geomMeshType != MeshType.Custom)
				{
					return Config.Instance.sharedMeshSegments;
				}
				return geomCustomSegments;
			}
			set
			{
				geomCustomSegments = value;
				Debug.LogWarning("The setter VLB.VolumetricLightBeam.geomSegments is OBSOLETE and has been renamed to geomCustomSegments.");
			}
		}

		public float attenuationLerpLinearQuad
		{
			get
			{
				if (attenuationEquation == AttenuationEquation.Linear)
				{
					return 0f;
				}
				if (attenuationEquation == AttenuationEquation.Quadratic)
				{
					return 1f;
				}
				return attenuationCustomBlending;
			}
		}

		public int sortingLayerID
		{
			get
			{
				return _SortingLayerID;
			}
			set
			{
				_SortingLayerID = value;
				if ((bool)m_BeamGeom)
				{
					m_BeamGeom.sortingLayerID = value;
				}
			}
		}

		public string sortingLayerName
		{
			get
			{
				return SortingLayer.IDToName(sortingLayerID);
			}
			set
			{
				sortingLayerID = SortingLayer.NameToID(value);
			}
		}

		public int sortingOrder
		{
			get
			{
				return _SortingOrder;
			}
			set
			{
				_SortingOrder = value;
				if ((bool)m_BeamGeom)
				{
					m_BeamGeom.sortingOrder = value;
				}
			}
		}

		public bool trackChangesDuringPlaytime
		{
			get
			{
				return _TrackChangesDuringPlaytime;
			}
			set
			{
				_TrackChangesDuringPlaytime = value;
				StartPlaytimeUpdateIfNeeded();
			}
		}

		public bool isCurrentlyTrackingChanges => m_CoPlaytimeUpdate != null;

		public bool hasGeometry => m_BeamGeom != null;

		public Bounds bounds
		{
			get
			{
				if (!(m_BeamGeom != null))
				{
					return new Bounds(Vector3.zero, Vector3.zero);
				}
				return m_BeamGeom.meshRenderer.bounds;
			}
		}

		public int blendingModeAsInt => Mathf.Clamp((int)blendingMode, 0, Enum.GetValues(typeof(BlendingMode)).Length);

		public MeshRenderer Renderer
		{
			get
			{
				if (!(m_BeamGeom != null))
				{
					return null;
				}
				return m_BeamGeom.meshRenderer;
			}
		}

		public string meshStats
		{
			get
			{
				Mesh mesh = (m_BeamGeom ? m_BeamGeom.coneMesh : null);
				if ((bool)mesh)
				{
					return $"Cone angle: {coneAngle:0.0} degrees\nMesh: {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles";
				}
				return "no mesh available";
			}
		}

		public int meshVerticesCount
		{
			get
			{
				if (!m_BeamGeom || !m_BeamGeom.coneMesh)
				{
					return 0;
				}
				return m_BeamGeom.coneMesh.vertexCount;
			}
		}

		public int meshTrianglesCount
		{
			get
			{
				if (!m_BeamGeom || !m_BeamGeom.coneMesh)
				{
					return 0;
				}
				return m_BeamGeom.coneMesh.triangles.Length / 3;
			}
		}

		private Light lightSpotAttached
		{
			get
			{
				if (_CachedLight == null)
				{
					_CachedLight = GetComponent<Light>();
				}
				if ((bool)_CachedLight && _CachedLight.type == LightType.Spot)
				{
					return _CachedLight;
				}
				return null;
			}
		}

		public void SetClippingPlane(Plane planeWS)
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.SetClippingPlane(planeWS);
			}
			m_PlaneWS = planeWS;
		}

		public void SetClippingPlaneOff()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.SetClippingPlaneOff();
			}
			m_PlaneWS = default(Plane);
		}

		public bool IsColliderHiddenByDynamicOccluder(Collider collider)
		{
			Debug.Assert(collider, "You should pass a valid Collider to VLB.VolumetricLightBeam.IsColliderHiddenByDynamicOccluder");
			if (!Utils.IsValid(m_PlaneWS))
			{
				return false;
			}
			return !GeometryUtility.TestPlanesAABB(new Plane[1] { m_PlaneWS }, collider.bounds);
		}

		public float GetInsideBeamFactor(Vector3 posWS)
		{
			return GetInsideBeamFactorFromObjectSpacePos(base.transform.InverseTransformPoint(posWS));
		}

		public float GetInsideBeamFactorFromObjectSpacePos(Vector3 posOS)
		{
			if (posOS.z < 0f)
			{
				return -1f;
			}
			Vector2 normalized = new Vector2(Utils.xy(posOS).magnitude, posOS.z + coneApexOffsetZ).normalized;
			return Mathf.Clamp((Mathf.Abs(Mathf.Sin(coneAngle * ((float)Math.PI / 180f) / 2f)) - Mathf.Abs(normalized.x)) / 0.1f, -1f, 1f);
		}

		[Obsolete("Use 'GenerateGeometry()' instead")]
		public void Generate()
		{
			GenerateGeometry();
		}

		public virtual void GenerateGeometry()
		{
			HandleBackwardCompatibility(pluginVersion, 1510);
			pluginVersion = 1510;
			ValidateProperties();
			if (m_BeamGeom == null)
			{
				Shader beamShader = Config.Instance.beamShader;
				if (!beamShader)
				{
					Debug.LogError("Invalid BeamShader set in VLB Config");
					return;
				}
				m_BeamGeom = Utils.NewWithComponent<BeamGeometry>("Beam Geometry");
				m_BeamGeom.Initialize(this, beamShader);
			}
			m_BeamGeom.RegenerateMesh();
			m_BeamGeom.visible = base.enabled;
		}

		public virtual void UpdateAfterManualPropertyChange()
		{
			ValidateProperties();
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.UpdateMaterialAndBounds();
			}
		}

		private void Start()
		{
			GenerateGeometry();
		}

		private void OnEnable()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.visible = true;
			}
			StartPlaytimeUpdateIfNeeded();
		}

		private void OnDisable()
		{
			if ((bool)m_BeamGeom)
			{
				m_BeamGeom.visible = false;
			}
			m_CoPlaytimeUpdate = null;
		}

		private void StartPlaytimeUpdateIfNeeded()
		{
		}

		private IEnumerator CoPlaytimeUpdate()
		{
			while (trackChangesDuringPlaytime && base.enabled)
			{
				UpdateAfterManualPropertyChange();
				yield return null;
			}
			m_CoPlaytimeUpdate = null;
		}

		private void OnDestroy()
		{
			DestroyBeam();
		}

		private void DestroyBeam()
		{
			if ((bool)m_BeamGeom)
			{
				UnityEngine.Object.DestroyImmediate(m_BeamGeom.gameObject);
			}
			m_BeamGeom = null;
		}

		private void AssignPropertiesFromSpotLight(Light lightSpot)
		{
			if ((bool)lightSpot && lightSpot.type == LightType.Spot)
			{
				if (fadeEndFromLight)
				{
					fadeEnd = lightSpot.range;
				}
				if (spotAngleFromLight)
				{
					spotAngle = lightSpot.spotAngle;
				}
				if (colorFromLight)
				{
					colorMode = ColorMode.Flat;
					color = lightSpot.color;
				}
			}
		}

		private void ClampProperties()
		{
			alphaInside = Mathf.Clamp01(alphaInside);
			alphaOutside = Mathf.Clamp01(alphaOutside);
			attenuationCustomBlending = Mathf.Clamp01(attenuationCustomBlending);
			fadeEnd = Mathf.Max(0.01f, fadeEnd);
			fadeStart = Mathf.Clamp(fadeStart, 0f, fadeEnd - 0.01f);
			spotAngle = Mathf.Clamp(spotAngle, 0.1f, 179.9f);
			coneRadiusStart = Mathf.Max(coneRadiusStart, 0f);
			depthBlendDistance = Mathf.Max(depthBlendDistance, 0f);
			cameraClippingDistance = Mathf.Max(cameraClippingDistance, 0f);
			geomCustomSides = Mathf.Clamp(geomCustomSides, 3, 256);
			geomCustomSegments = Mathf.Clamp(geomCustomSegments, 0, 64);
			fresnelPow = Mathf.Max(0f, fresnelPow);
			glareBehind = Mathf.Clamp01(glareBehind);
			glareFrontal = Mathf.Clamp01(glareFrontal);
			noiseIntensity = Mathf.Clamp(noiseIntensity, 0f, 1f);
		}

		private void ValidateProperties()
		{
			AssignPropertiesFromSpotLight(lightSpotAttached);
			ClampProperties();
		}

		private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
		{
			if (serializedVersion != -1 && serializedVersion != newVersion)
			{
				if (serializedVersion < 1301)
				{
					attenuationEquation = AttenuationEquation.Linear;
				}
				if (serializedVersion < 1501)
				{
					geomMeshType = MeshType.Custom;
					geomCustomSegments = 5;
				}
				Utils.MarkCurrentSceneDirty();
			}
		}
	}
}
