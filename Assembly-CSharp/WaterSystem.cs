using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Rust;
using Rust.Water5;
using UnityEngine;

[ExecuteInEditMode]
public class WaterSystem : MonoBehaviour
{
	[Serializable]
	public class RenderingSettings
	{
		[Serializable]
		public class SkyProbe
		{
			public float ProbeUpdateInterval = 1f;

			public bool TimeSlicing = true;
		}

		[Serializable]
		public class SSR
		{
			public float FresnelCutoff = 0.02f;

			public float ThicknessMin = 1f;

			public float ThicknessMax = 20f;

			public float ThicknessStartDist = 40f;

			public float ThicknessEndDist = 100f;
		}

		public Vector4[] TessellationQuality;

		public SkyProbe SkyReflections;

		public SSR ScreenSpaceReflections;
	}

	private static float oceanLevel = 0f;

	[Header("Ocean Settings")]
	public OceanSettings oceanSettings;

	public OceanSimulation oceanSimulation;

	public WaterQuality Quality = WaterQuality.High;

	public Material oceanMaterial;

	public RenderingSettings Rendering = new RenderingSettings();

	public int patchSize = 100;

	public int patchCount = 4;

	public float patchScale = 1f;

	public static WaterSystem Instance { get; private set; }

	public static WaterCollision Collision { get; private set; }

	public static WaterBody Ocean { get; private set; }

	public static Material OceanMaterial => Instance?.oceanMaterial;

	public static ListHashSet<WaterCamera> WaterCameras { get; } = new ListHashSet<WaterCamera>();


	public static HashSet<WaterBody> WaterBodies { get; } = new HashSet<WaterBody>();


	public static HashSet<WaterDepthMask> DepthMasks { get; } = new HashSet<WaterDepthMask>();


	public static float WaveTime { get; private set; }

	public static float OceanLevel
	{
		get
		{
			return oceanLevel;
		}
		set
		{
			value = Mathf.Max(value, 0f);
			if (!Mathf.Approximately(oceanLevel, value))
			{
				oceanLevel = value;
				UpdateOceanLevel();
			}
		}
	}

	public bool IsInitialized { get; private set; }

	public int Layer => base.gameObject.layer;

	public int Reflections => Water.reflections;

	public float WindowDirection => oceanSettings.windDirection;

	public float[] OctaveScales => oceanSettings.octaveScales;

	private void CheckInstance()
	{
		Instance = ((Instance != null) ? Instance : this);
		Collision = ((Collision != null) ? Collision : GetComponent<WaterCollision>());
	}

	private void Awake()
	{
		CheckInstance();
	}

	private void OnEnable()
	{
		CheckInstance();
		oceanSimulation = new OceanSimulation(oceanSettings);
		IsInitialized = true;
	}

	private void OnDisable()
	{
		if (!UnityEngine.Application.isPlaying || !Rust.Application.isQuitting)
		{
			oceanSimulation.Dispose();
			oceanSimulation = null;
			IsInitialized = false;
			Instance = null;
		}
	}

	private void Update()
	{
		using (TimeWarning.New("UpdateWaves"))
		{
			UpdateOceanSimulation();
		}
	}

	public static bool Trace(Ray ray, out Vector3 position, float maxDist = 100f)
	{
		if (Instance == null)
		{
			position = Vector3.zero;
			return false;
		}
		return Instance.oceanSimulation.Trace(ray, maxDist, out position);
	}

	public static bool Trace(Ray ray, out Vector3 position, out Vector3 normal, float maxDist = 100f)
	{
		if (Instance == null)
		{
			position = Vector3.zero;
			normal = Vector3.zero;
			return false;
		}
		normal = Vector3.up;
		return Instance.oceanSimulation.Trace(ray, maxDist, out position);
	}

	public static void GetHeightArray_Managed(Vector2[] pos, Vector2[] posUV, Vector3[] shore, float[] terrainHeight, float[] waterHeight)
	{
		if (TerrainTexturing.Instance != null)
		{
			for (int i = 0; i < posUV.Length; i++)
			{
				shore[i] = ((TerrainTexturing.Instance != null) ? TerrainTexturing.Instance.GetCoarseVectorToShore(posUV[i]) : Vector3.zero);
			}
		}
		for (int j = 0; j < pos.Length; j++)
		{
			terrainHeight[j] = ((TerrainMeta.HeightMap != null) ? TerrainMeta.HeightMap.GetHeightFast(posUV[j]) : 0f);
			if (Instance != null)
			{
				waterHeight[j] = Instance.oceanSimulation.GetHeight(pos[j].XZ3D());
			}
		}
		float num = OceanLevel;
		for (int k = 0; k < posUV.Length; k++)
		{
			Vector2 uv = posUV[k];
			float num2 = ((TerrainMeta.WaterMap != null) ? TerrainMeta.WaterMap.GetHeightFast(uv) : 0f);
			if (Instance != null && (double)num2 <= (double)num + 0.01)
			{
				waterHeight[k] = num + waterHeight[k];
			}
			else
			{
				waterHeight[k] = num2;
			}
		}
	}

	public static void GetHeightArray(Vector2[] pos, Vector2[] posUV, Vector3[] shore, float[] terrainHeight, float[] waterHeight)
	{
		GetHeightArray_Managed(pos, posUV, shore, terrainHeight, waterHeight);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetHeight(Vector3 pos)
	{
		if (Instance == null)
		{
			return OceanLevel;
		}
		return Instance.oceanSimulation.GetHeight(pos) + OceanLevel;
	}

	public static Vector3 GetNormal(Vector3 pos)
	{
		return Vector3.up;
	}

	public static float MinLevel()
	{
		if (Instance == null)
		{
			return OceanLevel;
		}
		return Instance.oceanSimulation.MinLevel() + OceanLevel;
	}

	public static float MaxLevel()
	{
		if (Instance == null)
		{
			return OceanLevel;
		}
		return Instance.oceanSimulation.MaxLevel() + OceanLevel;
	}

	public static void RegisterBody(WaterBody body)
	{
		if (body.Type == WaterBodyType.Ocean)
		{
			if (Ocean == null)
			{
				Ocean = body;
				body.Transform.position = body.Transform.position.WithY(OceanLevel);
			}
			else if (Ocean != body)
			{
				Debug.LogWarning("[Water] Ocean body is already registered. Ignoring call because only one is allowed.");
				return;
			}
		}
		WaterBodies.Add(body);
	}

	public static void UnregisterBody(WaterBody body)
	{
		if (body == Ocean)
		{
			Ocean = null;
		}
		WaterBodies.Remove(body);
	}

	private static void UpdateOceanLevel()
	{
		if (Ocean != null)
		{
			Ocean.Transform.position = Ocean.Transform.position.WithY(OceanLevel);
		}
		foreach (WaterBody waterBody in WaterBodies)
		{
			waterBody.OnOceanLevelChanged(OceanLevel);
		}
	}

	private void UpdateOceanSimulation()
	{
		if (Water.scaled_time)
		{
			WaveTime += UnityEngine.Time.deltaTime;
		}
		else
		{
			WaveTime = UnityEngine.Time.realtimeSinceStartup;
		}
		if (Weather.ocean_time >= 0f)
		{
			WaveTime = Weather.ocean_time;
		}
		float beaufort = (SingletonComponent<Climate>.Instance ? SingletonComponent<Climate>.Instance.WeatherState.OceanScale : 4f);
		oceanSimulation?.Update(WaveTime, UnityEngine.Time.deltaTime, beaufort);
	}

	public void Refresh()
	{
		oceanSimulation.Dispose();
		oceanSimulation = new OceanSimulation(oceanSettings);
	}

	private void EditorInitialize()
	{
	}

	private void EditorShutdown()
	{
	}
}
