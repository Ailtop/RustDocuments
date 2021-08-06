#define UNITY_ASSERTIONS
using UnityEngine;

namespace VLB
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(VolumetricLightBeam))]
	[HelpURL("http://saladgamer.com/vlb-doc/comp-dustparticles/")]
	public class VolumetricDustParticles : MonoBehaviour
	{
		public enum Direction
		{
			Beam,
			Random
		}

		[Range(0f, 1f)]
		public float alpha = 0.5f;

		[Range(0.0001f, 0.1f)]
		public float size = 0.01f;

		public Direction direction = Direction.Random;

		public float speed = 0.03f;

		public float density = 5f;

		[Range(0f, 1f)]
		public float spawnMaxDistance = 0.7f;

		public bool cullingEnabled = true;

		public float cullingMaxDistance = 10f;

		public static bool isFeatureSupported = true;

		private ParticleSystem m_Particles;

		private ParticleSystemRenderer m_Renderer;

		private static bool ms_NoMainCameraLogged = false;

		private static Camera ms_MainCamera = null;

		private VolumetricLightBeam m_Master;

		public bool isCulled { get; private set; }

		public bool particlesAreInstantiated => m_Particles;

		public int particlesCurrentCount
		{
			get
			{
				if (!m_Particles)
				{
					return 0;
				}
				return m_Particles.particleCount;
			}
		}

		public int particlesMaxCount
		{
			get
			{
				if (!m_Particles)
				{
					return 0;
				}
				return m_Particles.main.maxParticles;
			}
		}

		public Camera mainCamera
		{
			get
			{
				if (!ms_MainCamera)
				{
					ms_MainCamera = Camera.main;
					if (!ms_MainCamera && !ms_NoMainCameraLogged)
					{
						Debug.LogErrorFormat(base.gameObject, "In order to use 'VolumetricDustParticles' culling, you must have a MainCamera defined in your scene.");
						ms_NoMainCameraLogged = true;
					}
				}
				return ms_MainCamera;
			}
		}

		private void Start()
		{
			isCulled = false;
			m_Master = GetComponent<VolumetricLightBeam>();
			Debug.Assert(m_Master);
			InstantiateParticleSystem();
			SetActiveAndPlay();
		}

		private void InstantiateParticleSystem()
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(true);
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				Object.DestroyImmediate(componentsInChildren[num].gameObject);
			}
			m_Particles = Config.Instance.NewVolumetricDustParticles();
			if ((bool)m_Particles)
			{
				m_Particles.transform.SetParent(base.transform, false);
				m_Renderer = m_Particles.GetComponent<ParticleSystemRenderer>();
			}
		}

		private void OnEnable()
		{
			SetActiveAndPlay();
		}

		private void SetActiveAndPlay()
		{
			if ((bool)m_Particles)
			{
				m_Particles.gameObject.SetActive(true);
				SetParticleProperties();
				m_Particles.Play(true);
			}
		}

		private void OnDisable()
		{
			if ((bool)m_Particles)
			{
				m_Particles.gameObject.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			if ((bool)m_Particles)
			{
				Object.DestroyImmediate(m_Particles.gameObject);
			}
			m_Particles = null;
		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				UpdateCulling();
			}
			SetParticleProperties();
		}

		private void SetParticleProperties()
		{
			if (!m_Particles || !m_Particles.gameObject.activeSelf)
			{
				return;
			}
			float t = Mathf.Clamp01(1f - m_Master.fresnelPow / 10f);
			float num = m_Master.fadeEnd * spawnMaxDistance;
			float num2 = num * density;
			int maxParticles = (int)(num2 * 4f);
			ParticleSystem.MainModule main = m_Particles.main;
			ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
			startLifetime.mode = ParticleSystemCurveMode.TwoConstants;
			startLifetime.constantMin = 4f;
			startLifetime.constantMax = 6f;
			main.startLifetime = startLifetime;
			ParticleSystem.MinMaxCurve startSize = main.startSize;
			startSize.mode = ParticleSystemCurveMode.TwoConstants;
			startSize.constantMin = size * 0.9f;
			startSize.constantMax = size * 1.1f;
			main.startSize = startSize;
			ParticleSystem.MinMaxGradient startColor = main.startColor;
			if (m_Master.colorMode == ColorMode.Flat)
			{
				startColor.mode = ParticleSystemGradientMode.Color;
				Color color = m_Master.color;
				color.a *= alpha;
				startColor.color = color;
			}
			else
			{
				startColor.mode = ParticleSystemGradientMode.Gradient;
				Gradient colorGradient = m_Master.colorGradient;
				GradientColorKey[] colorKeys = colorGradient.colorKeys;
				GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
				for (int i = 0; i < alphaKeys.Length; i++)
				{
					alphaKeys[i].alpha *= alpha;
				}
				Gradient gradient = new Gradient();
				gradient.SetKeys(colorKeys, alphaKeys);
				startColor.gradient = gradient;
			}
			main.startColor = startColor;
			ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
			startSpeed.constant = speed;
			main.startSpeed = startSpeed;
			main.maxParticles = maxParticles;
			ParticleSystem.ShapeModule shape = m_Particles.shape;
			shape.shapeType = ParticleSystemShapeType.ConeVolume;
			shape.radius = m_Master.coneRadiusStart * Mathf.Lerp(0.3f, 1f, t);
			shape.angle = m_Master.coneAngle * 0.5f * Mathf.Lerp(0.7f, 1f, t);
			shape.length = num;
			shape.arc = 360f;
			shape.randomDirectionAmount = ((direction == Direction.Random) ? 1f : 0f);
			ParticleSystem.EmissionModule emission = m_Particles.emission;
			ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
			rateOverTime.constant = num2;
			emission.rateOverTime = rateOverTime;
			if ((bool)m_Renderer)
			{
				m_Renderer.sortingLayerID = m_Master.sortingLayerID;
				m_Renderer.sortingOrder = m_Master.sortingOrder;
			}
		}

		private void UpdateCulling()
		{
			if (!m_Particles)
			{
				return;
			}
			bool flag = true;
			if (cullingEnabled && m_Master.hasGeometry)
			{
				if ((bool)mainCamera)
				{
					float num = cullingMaxDistance * cullingMaxDistance;
					flag = m_Master.bounds.SqrDistance(mainCamera.transform.position) <= num;
				}
				else
				{
					cullingEnabled = false;
				}
			}
			if (m_Particles.gameObject.activeSelf != flag)
			{
				m_Particles.gameObject.SetActive(flag);
				isCulled = !flag;
			}
			if (flag && !m_Particles.isPlaying)
			{
				m_Particles.Play();
			}
		}
	}
}
