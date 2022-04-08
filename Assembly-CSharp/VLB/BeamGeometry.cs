#define UNITY_ASSERTIONS
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB
{
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam/")]
	public class BeamGeometry : MonoBehaviour
	{
		private VolumetricLightBeam m_Master;

		private Matrix4x4 m_ColorGradientMatrix;

		private MeshType m_CurrentMeshType;

		public MeshRenderer meshRenderer { get; private set; }

		public MeshFilter meshFilter { get; private set; }

		public Material material { get; private set; }

		public Mesh coneMesh { get; private set; }

		public bool visible
		{
			get
			{
				return meshRenderer.enabled;
			}
			set
			{
				meshRenderer.enabled = value;
			}
		}

		public int sortingLayerID
		{
			get
			{
				return meshRenderer.sortingLayerID;
			}
			set
			{
				meshRenderer.sortingLayerID = value;
			}
		}

		public int sortingOrder
		{
			get
			{
				return meshRenderer.sortingOrder;
			}
			set
			{
				meshRenderer.sortingOrder = value;
			}
		}

		private void Start()
		{
		}

		private void OnDestroy()
		{
			if ((bool)material)
			{
				UnityEngine.Object.DestroyImmediate(material);
				material = null;
			}
		}

		private static bool IsUsingCustomRenderPipeline()
		{
			if (RenderPipelineManager.currentPipeline == null)
			{
				return GraphicsSettings.renderPipelineAsset != null;
			}
			return true;
		}

		private void OnEnable()
		{
			if (IsUsingCustomRenderPipeline())
			{
				RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
			}
		}

		private void OnDisable()
		{
			if (IsUsingCustomRenderPipeline())
			{
				RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
			}
		}

		public void Initialize(VolumetricLightBeam master, Shader shader)
		{
			HideFlags proceduralObjectsHideFlags = Consts.ProceduralObjectsHideFlags;
			m_Master = master;
			base.transform.SetParent(master.transform, false);
			material = new Material(shader);
			material.hideFlags = proceduralObjectsHideFlags;
			meshRenderer = Utils.GetOrAddComponent<MeshRenderer>(base.gameObject);
			meshRenderer.hideFlags = proceduralObjectsHideFlags;
			meshRenderer.material = material;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			if (SortingLayer.IsValid(m_Master.sortingLayerID))
			{
				sortingLayerID = m_Master.sortingLayerID;
			}
			else
			{
				Debug.LogError($"Beam '{Utils.GetPath(m_Master.transform)}' has an invalid sortingLayerID ({m_Master.sortingLayerID}). Please fix it by setting a valid layer.");
			}
			sortingOrder = m_Master.sortingOrder;
			meshFilter = Utils.GetOrAddComponent<MeshFilter>(base.gameObject);
			meshFilter.hideFlags = proceduralObjectsHideFlags;
			base.gameObject.hideFlags = proceduralObjectsHideFlags;
		}

		public void RegenerateMesh()
		{
			Debug.Assert(m_Master);
			base.gameObject.layer = Config.Instance.geometryLayerID;
			base.gameObject.tag = Config.Instance.geometryTag;
			if ((bool)coneMesh && m_CurrentMeshType == MeshType.Custom)
			{
				UnityEngine.Object.DestroyImmediate(coneMesh);
			}
			m_CurrentMeshType = m_Master.geomMeshType;
			switch (m_Master.geomMeshType)
			{
			case MeshType.Custom:
				coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap);
				coneMesh.hideFlags = Consts.ProceduralObjectsHideFlags;
				meshFilter.mesh = coneMesh;
				break;
			case MeshType.Shared:
				coneMesh = GlobalMesh.mesh;
				meshFilter.sharedMesh = coneMesh;
				break;
			default:
				Debug.LogError("Unsupported MeshType");
				break;
			}
			UpdateMaterialAndBounds();
		}

		private void ComputeLocalMatrix()
		{
			float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
			base.transform.localScale = new Vector3(num, num, m_Master.fadeEnd);
		}

		public void UpdateMaterialAndBounds()
		{
			Debug.Assert(m_Master);
			material.renderQueue = Config.Instance.geometryRenderQueue;
			float f = m_Master.coneAngle * ((float)Math.PI / 180f) / 2f;
			material.SetVector("_ConeSlopeCosSin", new Vector2(Mathf.Cos(f), Mathf.Sin(f)));
			Vector2 vector = new Vector2(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f));
			material.SetVector("_ConeRadius", vector);
			float value = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), 0.0001f);
			material.SetFloat("_ConeApexOffsetZ", value);
			if (m_Master.colorMode == ColorMode.Gradient)
			{
				Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
				material.EnableKeyword((floatPackingPrecision == Utils.FloatPackingPrecision.High) ? "VLB_COLOR_GRADIENT_MATRIX_HIGH" : "VLB_COLOR_GRADIENT_MATRIX_LOW");
				m_ColorGradientMatrix = Utils.SampleInMatrix(m_Master.colorGradient, (int)floatPackingPrecision);
			}
			else
			{
				material.DisableKeyword("VLB_COLOR_GRADIENT_MATRIX_HIGH");
				material.DisableKeyword("VLB_COLOR_GRADIENT_MATRIX_LOW");
				material.SetColor("_ColorFlat", m_Master.color);
			}
			if (Consts.BlendingMode_AlphaAsBlack[m_Master.blendingModeAsInt])
			{
				material.EnableKeyword("ALPHA_AS_BLACK");
			}
			else
			{
				material.DisableKeyword("ALPHA_AS_BLACK");
			}
			material.SetInt("_BlendSrcFactor", (int)Consts.BlendingMode_SrcFactor[m_Master.blendingModeAsInt]);
			material.SetInt("_BlendDstFactor", (int)Consts.BlendingMode_DstFactor[m_Master.blendingModeAsInt]);
			material.SetFloat("_AlphaInside", m_Master.alphaInside);
			material.SetFloat("_AlphaOutside", m_Master.alphaOutside);
			material.SetFloat("_AttenuationLerpLinearQuad", m_Master.attenuationLerpLinearQuad);
			material.SetFloat("_DistanceFadeStart", m_Master.fadeStart);
			material.SetFloat("_DistanceFadeEnd", m_Master.fadeEnd);
			material.SetFloat("_DistanceCamClipping", m_Master.cameraClippingDistance);
			material.SetFloat("_FresnelPow", Mathf.Max(0.001f, m_Master.fresnelPow));
			material.SetFloat("_GlareBehind", m_Master.glareBehind);
			material.SetFloat("_GlareFrontal", m_Master.glareFrontal);
			material.SetFloat("_DrawCap", m_Master.geomCap ? 1 : 0);
			if (m_Master.depthBlendDistance > 0f)
			{
				material.EnableKeyword("VLB_DEPTH_BLEND");
				material.SetFloat("_DepthBlendDistance", m_Master.depthBlendDistance);
			}
			else
			{
				material.DisableKeyword("VLB_DEPTH_BLEND");
			}
			if (m_Master.noiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported)
			{
				Noise3D.LoadIfNeeded();
				material.EnableKeyword("VLB_NOISE_3D");
				material.SetVector("_NoiseLocal", new Vector4(m_Master.noiseVelocityLocal.x, m_Master.noiseVelocityLocal.y, m_Master.noiseVelocityLocal.z, m_Master.noiseScaleLocal));
				material.SetVector("_NoiseParam", new Vector3(m_Master.noiseIntensity, m_Master.noiseVelocityUseGlobal ? 1f : 0f, m_Master.noiseScaleUseGlobal ? 1f : 0f));
			}
			else
			{
				material.DisableKeyword("VLB_NOISE_3D");
			}
			ComputeLocalMatrix();
		}

		public void SetClippingPlane(Plane planeWS)
		{
			Vector3 normal = planeWS.normal;
			material.EnableKeyword("VLB_CLIPPING_PLANE");
			material.SetVector("_ClippingPlaneWS", new Vector4(normal.x, normal.y, normal.z, planeWS.distance));
		}

		public void SetClippingPlaneOff()
		{
			material.DisableKeyword("VLB_CLIPPING_PLANE");
		}

		private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
		{
			UpdateCameraRelatedProperties(cam);
		}

		private void OnWillRenderObject()
		{
			if (!IsUsingCustomRenderPipeline())
			{
				Camera current = Camera.current;
				if (current != null)
				{
					UpdateCameraRelatedProperties(current);
				}
			}
		}

		private void UpdateCameraRelatedProperties(Camera cam)
		{
			if (!cam || !m_Master)
			{
				return;
			}
			if ((bool)material)
			{
				Vector3 vector = m_Master.transform.InverseTransformPoint(cam.transform.position);
				material.SetVector("_CameraPosObjectSpace", vector);
				Vector3 normalized = base.transform.InverseTransformDirection(cam.transform.forward).normalized;
				float w = (cam.orthographic ? (-1f) : m_Master.GetInsideBeamFactorFromObjectSpacePos(vector));
				material.SetVector("_CameraParams", new Vector4(normalized.x, normalized.y, normalized.z, w));
				if (m_Master.colorMode == ColorMode.Gradient)
				{
					material.SetMatrix("_ColorGradientMatrix", m_ColorGradientMatrix);
				}
			}
			if (m_Master.depthBlendDistance > 0f)
			{
				cam.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
	}
}
