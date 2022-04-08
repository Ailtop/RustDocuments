#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class WaterSystem : MonoBehaviour
{
	[Serializable]
	public class SimulationSettings
	{
		public Vector3 Wind = new Vector3(3f, 0f, 3f);

		public int SolverResolution = 64;

		public float SolverSizeInWorld = 18f;

		public float Gravity = 9.81f;

		public float Amplitude = 0.0001f;

		public Texture2D PerlinNoise;

		public WaterGerstner.WaveParams[] OpenSeaWaves = new WaterGerstner.WaveParams[6];

		public WaterGerstner.ShoreWaveParams ShoreWaves = new WaterGerstner.ShoreWaveParams();

		[Range(0.1f, 250f)]
		public float ShoreWavesFadeDistance = 25f;

		[Range(0.1f, 250f)]
		public float TerrainFadeDistance = 10f;

		[Range(0.001f, 1f)]
		public float OpenSeaCrestFoamThreshold = 0.08f;

		[Range(0.001f, 1f)]
		public float ShoreCrestFoamThreshold = 0.08f;

		[Range(0.001f, 1f)]
		public float ShoreCrestFoamFarThreshold = 0.08f;

		[Range(0.1f, 250f)]
		public float ShoreCrestFoamFadeDistance = 10f;
	}

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

		[Serializable]
		public class Caustics
		{
			public float FrameRate = 15f;

			public Texture2D[] FramesShallow = new Texture2D[0];

			public Texture2D[] FramesDeep = new Texture2D[0];
		}

		public float MaxDisplacementDistance = 50f;

		public SkyProbe SkyReflections;

		public SSR ScreenSpaceReflections;

		public Caustics CausticsAnimation;
	}

	private enum NativePathState
	{
		Initializing = 0,
		Failed = 1,
		Ready = 2
	}

	public WaterQuality Quality = WaterQuality.High;

	public bool ShowDebug;

	public bool ShowGizmos;

	public bool ProgressTime = true;

	public SimulationSettings Simulation = new SimulationSettings();

	public RenderingSettings Rendering = new RenderingSettings();

	private WaterGerstner.PrecomputedWave[] precomputedWaves = new WaterGerstner.PrecomputedWave[6]
	{
		WaterGerstner.PrecomputedWave.Default,
		WaterGerstner.PrecomputedWave.Default,
		WaterGerstner.PrecomputedWave.Default,
		WaterGerstner.PrecomputedWave.Default,
		WaterGerstner.PrecomputedWave.Default,
		WaterGerstner.PrecomputedWave.Default
	};

	private WaterGerstner.PrecomputedShoreWaves precomputedShoreWaves = WaterGerstner.PrecomputedShoreWaves.Default;

	private Vector4[] waveArray = new Vector4[0];

	private Vector4[] shoreWaveArray = new Vector4[0];

	private Vector4 global0;

	private Vector4 global1;

	private static float oceanLevel = 0f;

	private static WaterSystem instance;

	private static Vector3[] emptyShoreMap = new Vector3[1] { Vector3.one };

	private static short[] emptyWaterMap = new short[1];

	private static short[] emptyHeightMap = new short[1];

	private static NativePathState nativePathState = NativePathState.Initializing;

	private static Vector3[] currentShoreMap;

	private static GCHandle currentShoreMapHandle;

	private static short[] currentWaterMap;

	private static GCHandle currentWaterMapHandle;

	private static short[] currentHeightMap;

	private static GCHandle currentHeightMapHandle;

	private static Vector4[] currentOpenWaves;

	private static GCHandle currentOpenWavesHandle;

	private static Vector4[] currentShoreWaves;

	private static GCHandle currentShoreWavesHandle;

	public WaterGerstner.PrecomputedWave[] PrecomputedWaves => precomputedWaves;

	public WaterGerstner.PrecomputedShoreWaves PrecomputedShoreWaves => precomputedShoreWaves;

	public Vector4 Global0 => global0;

	public Vector4 Global1 => global1;

	public float ShoreWavesRcpFadeDistance { get; private set; } = 0.04f;


	public float TerrainRcpFadeDistance { get; private set; } = 0.1f;


	public bool IsInitialized { get; private set; }

	public static WaterCollision Collision { get; private set; }

	public static WaterDynamics Dynamics { get; private set; }

	public static WaterBody Ocean { get; private set; } = null;


	public static HashSet<WaterBody> WaterBodies { get; private set; } = new HashSet<WaterBody>();


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

	public static float WaveTime { get; private set; } = 0f;


	public static WaterSystem Instance => instance;

	private void CheckInstance()
	{
		instance = ((instance != null) ? instance : this);
		Collision = ((Collision != null) ? Collision : GetComponent<WaterCollision>());
		Dynamics = ((Dynamics != null) ? Dynamics : GetComponent<WaterDynamics>());
	}

	public void Awake()
	{
		CheckInstance();
	}

	[DllImport("RustNative", EntryPoint = "Water_SetBaseConstants")]
	private static extern void SetBaseConstants_Native(int shoreMapSize, ref Vector3 shoreMap, int waterHeightMapSize, ref short waterHeightMap, Vector4 packedParams);

	[DllImport("RustNative", EntryPoint = "Water_SetTerrainConstants")]
	private static extern void SetTerrainConstants_Native(int terrainHeightMapSize, ref short terrainHeightMap, Vector3 terrainPosition, Vector3 terrainSize);

	[DllImport("RustNative", EntryPoint = "Water_SetGerstnerConstants")]
	private static extern void SetGerstnerConstants_Native(Vector4 globalParams0, Vector4 globalParams1, ref Vector4 openWaves, ref Vector4 shoreWaves);

	[DllImport("RustNative", EntryPoint = "Water_UpdateOceanLevel")]
	private static extern void UpdateOceanLevel_Native(float oceanWaterLevel);

	[DllImport("RustNative", EntryPoint = "Water_GetHeightArray")]
	private static extern float GetHeightArray_Native(int sampleCount, ref Vector2 pos, ref Vector2 posUV, ref Vector3 shore, ref float terrainHeight, ref float waterHeight);

	[DllImport("RustNative", EntryPoint = "Water_GetHeight")]
	private static extern float GetHeight_Native(Vector3 pos);

	[DllImport("RustNative")]
	private static extern bool CPU_SupportsSSE41();

	private static void SetNativeConstants(TerrainTexturing terrainTexturing, TerrainWaterMap terrainWaterMap, TerrainHeightMap terrainHeightMap, Vector4 globalParams0, Vector4 globalParams1, Vector4[] openWaves, Vector4[] shoreWaves)
	{
		if (nativePathState == NativePathState.Initializing)
		{
			try
			{
				nativePathState = ((!CPU_SupportsSSE41()) ? NativePathState.Failed : nativePathState);
			}
			catch (EntryPointNotFoundException)
			{
				nativePathState = NativePathState.Failed;
			}
		}
		if (nativePathState == NativePathState.Failed)
		{
			return;
		}
		try
		{
			int shoreMapSize = 1;
			Vector3[] shoreMap = emptyShoreMap;
			if (terrainTexturing != null && terrainTexturing.ShoreMap != null)
			{
				shoreMapSize = terrainTexturing.ShoreMapSize;
				shoreMap = terrainTexturing.ShoreMap;
			}
			int waterHeightMapSize = 1;
			short[] src = emptyWaterMap;
			if (terrainWaterMap != null && terrainWaterMap.src != null && terrainWaterMap.src.Length != 0)
			{
				waterHeightMapSize = terrainWaterMap.res;
				src = terrainWaterMap.src;
			}
			int terrainHeightMapSize = 1;
			short[] src2 = emptyHeightMap;
			if (terrainHeightMap != null && terrainHeightMap.src != null && terrainHeightMap.src.Length != 0)
			{
				terrainHeightMapSize = terrainHeightMap.res;
				src2 = terrainHeightMap.src;
			}
			Vector4 packedParams = default(Vector4);
			packedParams.x = OceanLevel;
			packedParams.y = ((instance != null) ? 1f : 0f);
			packedParams.z = ((TerrainTexturing.Instance != null) ? 1f : 0f);
			packedParams.w = 0f;
			PinObject(shoreMap, ref currentShoreMap, ref currentShoreMapHandle);
			PinObject(src, ref currentWaterMap, ref currentWaterMapHandle);
			PinObject(src2, ref currentHeightMap, ref currentHeightMapHandle);
			PinObject(openWaves, ref currentOpenWaves, ref currentOpenWavesHandle);
			PinObject(shoreWaves, ref currentShoreWaves, ref currentShoreWavesHandle);
			SetBaseConstants_Native(shoreMapSize, ref shoreMap[0], waterHeightMapSize, ref src[0], packedParams);
			SetTerrainConstants_Native(terrainHeightMapSize, ref src2[0], TerrainMeta.Position, TerrainMeta.Size);
			SetGerstnerConstants_Native(globalParams0, globalParams1, ref openWaves[0], ref shoreWaves[0]);
			nativePathState = NativePathState.Ready;
		}
		catch (EntryPointNotFoundException)
		{
			nativePathState = NativePathState.Failed;
		}
	}

	private static void PinObject<T>(T value, ref T currentValue, ref GCHandle currentValueHandle) where T : class
	{
		if (value != null && value != currentValue)
		{
			if (currentValueHandle.IsAllocated)
			{
				currentValueHandle.Free();
			}
			currentValue = value;
			currentValueHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
		}
	}

	private static float GetHeight_Managed(Vector3 pos)
	{
		Vector2 uv = default(Vector2);
		uv.x = (pos.x - TerrainMeta.Position.x) * TerrainMeta.OneOverSize.x;
		uv.y = (pos.z - TerrainMeta.Position.z) * TerrainMeta.OneOverSize.z;
		float num = OceanLevel;
		float num2 = ((TerrainMeta.WaterMap != null) ? TerrainMeta.WaterMap.GetHeightFast(uv) : 0f);
		float num3 = ((TerrainMeta.HeightMap != null) ? TerrainMeta.HeightMap.GetHeightFast(uv) : 0f);
		if (instance != null && (double)num2 <= (double)num + 0.01)
		{
			Vector3 shore = ((TerrainTexturing.Instance != null) ? TerrainTexturing.Instance.GetCoarseVectorToShore(uv) : Vector3.zero);
			float num4 = Mathf.Clamp01(Mathf.Abs(num - num3) * 0.1f);
			num2 = WaterGerstner.SampleHeight(instance, pos, shore) * num4;
		}
		return num2;
	}

	public static void GetHeightArray_Managed(Vector2[] pos, Vector2[] posUV, Vector3[] shore, float[] terrainHeight, float[] waterHeight)
	{
		if (TerrainTexturing.Instance != null)
		{
			for (int i = 0; i < posUV.Length; i++)
			{
				shore[i] = TerrainTexturing.Instance.GetCoarseVectorToShore(posUV[i]);
			}
		}
		if (instance != null)
		{
			WaterGerstner.SampleHeightArray(instance, pos, shore, waterHeight);
		}
		float num = OceanLevel;
		for (int j = 0; j < posUV.Length; j++)
		{
			Vector2 uv = posUV[j];
			terrainHeight[j] = ((TerrainMeta.HeightMap != null) ? TerrainMeta.HeightMap.GetHeightFast(uv) : 0f);
			float num2 = ((TerrainMeta.WaterMap != null) ? TerrainMeta.WaterMap.GetHeightFast(uv) : 0f);
			if (instance != null && (double)num2 <= (double)num + 0.01)
			{
				float num3 = Mathf.Clamp01(Mathf.Abs(num - terrainHeight[j]) * 0.1f);
				waterHeight[j] = num + waterHeight[j] * num3;
			}
			else
			{
				waterHeight[j] = num2;
			}
		}
	}

	public static float GetHeight(Vector3 pos)
	{
		float val = ((nativePathState != NativePathState.Ready) ? GetHeight_Managed(pos) : GetHeight_Native(pos));
		return Math.Max(val, OceanLevel);
	}

	public static void GetHeightArray(Vector2[] pos, Vector2[] posUV, Vector3[] shore, float[] terrainHeight, float[] waterHeight)
	{
		Debug.Assert(pos.Length == posUV.Length);
		Debug.Assert(pos.Length == shore.Length);
		Debug.Assert(pos.Length == terrainHeight.Length);
		Debug.Assert(pos.Length == waterHeight.Length);
		if (nativePathState == NativePathState.Ready)
		{
			GetHeightArray_Native(pos.Length, ref pos[0], ref posUV[0], ref shore[0], ref terrainHeight[0], ref waterHeight[0]);
		}
		else
		{
			GetHeightArray_Managed(pos, posUV, shore, terrainHeight, waterHeight);
		}
	}

	public static Vector3 GetNormal(Vector3 pos)
	{
		return ((TerrainMeta.WaterMap != null) ? TerrainMeta.WaterMap.GetNormal(pos) : Vector3.up).normalized;
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
		WaterBodies.Remove(body);
	}

	private void UpdateWaves()
	{
		WaveTime = (ProgressTime ? Time.realtimeSinceStartup : WaveTime);
		using (TimeWarning.New("WaterGerstner.UpdatePrecomputedWaves"))
		{
			WaterGerstner.UpdatePrecomputedWaves(Simulation.OpenSeaWaves, ref precomputedWaves);
		}
		using (TimeWarning.New("WaterGerstner.UpdatePrecomputedShoreWaves"))
		{
			WaterGerstner.UpdatePrecomputedShoreWaves(Simulation.ShoreWaves, ref precomputedShoreWaves);
		}
	}

	private static void UpdateOceanLevel()
	{
		if (Ocean != null)
		{
			Ocean.Transform.position = Ocean.Transform.position.WithY(OceanLevel);
		}
		if (nativePathState == NativePathState.Ready)
		{
			UpdateOceanLevel_Native(OceanLevel);
		}
		foreach (WaterBody waterBody in WaterBodies)
		{
			waterBody.OnOceanLevelChanged(OceanLevel);
		}
	}

	public void UpdateWaveData()
	{
		ShoreWavesRcpFadeDistance = 1f / Simulation.ShoreWavesFadeDistance;
		TerrainRcpFadeDistance = 1f / Simulation.TerrainFadeDistance;
		global0.x = ShoreWavesRcpFadeDistance;
		global0.y = TerrainRcpFadeDistance;
		global0.z = precomputedShoreWaves.DirectionVarFreq;
		global0.w = precomputedShoreWaves.DirectionVarAmp;
		global1.x = precomputedShoreWaves.Steepness;
		global1.y = precomputedShoreWaves.Amplitude;
		global1.z = precomputedShoreWaves.K;
		global1.w = precomputedShoreWaves.C;
		using (TimeWarning.New("WaterGerstner.UpdateWaveArray"))
		{
			WaterGerstner.UpdateWaveArray(precomputedWaves, ref waveArray);
		}
		using (TimeWarning.New("WaterGerstner.UpdateShoreWaveArray"))
		{
			WaterGerstner.UpdateShoreWaveArray(precomputedShoreWaves, ref shoreWaveArray);
		}
		using (TimeWarning.New("WaterSystem.SetNativeConstants"))
		{
			SetNativeConstants(TerrainTexturing.Instance, TerrainMeta.WaterMap, TerrainMeta.HeightMap, global0, global1, waveArray, shoreWaveArray);
		}
	}

	private void Update()
	{
		using (TimeWarning.New("UpdateWaves"))
		{
			UpdateWaves();
		}
		using (TimeWarning.New("UpdateWaveData"))
		{
			UpdateWaveData();
		}
	}
}
