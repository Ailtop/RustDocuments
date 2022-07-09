#define UNITY_ASSERTIONS
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/config/")]
public class Config : ScriptableObject
{
	public int geometryLayerID = 1;

	public string geometryTag = "Untagged";

	public int geometryRenderQueue = 3000;

	public bool forceSinglePass;

	[SerializeField]
	[HighlightNull]
	private Shader beamShader1Pass;

	[FormerlySerializedAs("BeamShader")]
	[FormerlySerializedAs("beamShader")]
	[SerializeField]
	[HighlightNull]
	private Shader beamShader2Pass;

	public int sharedMeshSides = 24;

	public int sharedMeshSegments = 5;

	[Range(0.01f, 2f)]
	public float globalNoiseScale = 0.5f;

	public Vector3 globalNoiseVelocity = Consts.NoiseVelocityDefault;

	[HighlightNull]
	public TextAsset noise3DData;

	public int noise3DSize = 64;

	[HighlightNull]
	public ParticleSystem dustParticlesPrefab;

	private static Config m_Instance;

	public Shader beamShader
	{
		get
		{
			if (!forceSinglePass)
			{
				return beamShader2Pass;
			}
			return beamShader1Pass;
		}
	}

	public Vector4 globalNoiseParam => new Vector4(globalNoiseVelocity.x, globalNoiseVelocity.y, globalNoiseVelocity.z, globalNoiseScale);

	public static Config Instance
	{
		get
		{
			if (m_Instance == null)
			{
				Config[] array = Resources.LoadAll<Config>("Config");
				Debug.Assert(array.Length != 0, $"Can't find any resource of type '{typeof(Config)}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.");
				m_Instance = array[0];
			}
			return m_Instance;
		}
	}

	public void Reset()
	{
		geometryLayerID = 1;
		geometryTag = "Untagged";
		geometryRenderQueue = 3000;
		beamShader1Pass = Shader.Find("Hidden/VolumetricLightBeam1Pass");
		beamShader2Pass = Shader.Find("Hidden/VolumetricLightBeam2Pass");
		sharedMeshSides = 24;
		sharedMeshSegments = 5;
		globalNoiseScale = 0.5f;
		globalNoiseVelocity = Consts.NoiseVelocityDefault;
		noise3DData = Resources.Load("Noise3D_64x64x64") as TextAsset;
		noise3DSize = 64;
		dustParticlesPrefab = Resources.Load("DustParticles", typeof(ParticleSystem)) as ParticleSystem;
	}

	public ParticleSystem NewVolumetricDustParticles()
	{
		if (!dustParticlesPrefab)
		{
			if (Application.isPlaying)
			{
				Debug.LogError("Failed to instantiate VolumetricDustParticles prefab.");
			}
			return null;
		}
		ParticleSystem particleSystem = Object.Instantiate(dustParticlesPrefab);
		particleSystem.useAutoRandomSeed = false;
		particleSystem.name = "Dust Particles";
		particleSystem.gameObject.hideFlags = Consts.ProceduralObjectsHideFlags;
		particleSystem.gameObject.SetActive(value: true);
		return particleSystem;
	}
}
