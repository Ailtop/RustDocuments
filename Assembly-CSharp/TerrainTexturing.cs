using Rust;
using Unity.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainTexturing : TerrainExtension
{
	private const int ShoreVectorDownscale = 3;

	private const int ShoreVectorBlurPasses = 0;

	private float terrainSize;

	private int shoreMapSize;

	private float[] shoreDistances;

	private Vector3[] shoreVectors;

	public bool debugFoliageDisplacement;

	private bool initialized;

	private static TerrainTexturing instance;

	public int ShoreMapSize => shoreMapSize;

	public Vector3[] ShoreMap => shoreVectors;

	public static TerrainTexturing Instance => instance;

	private void InitializeBasePyramid()
	{
	}

	private void ReleaseBasePyramid()
	{
	}

	private void UpdateBasePyramid()
	{
	}

	private void InitializeCoarseHeightSlope()
	{
	}

	private void ReleaseCoarseHeightSlope()
	{
	}

	private void UpdateCoarseHeightSlope()
	{
	}

	private void InitializeShoreVector()
	{
		int num = Mathf.ClosestPowerOfTwo(terrain.terrainData.heightmapResolution) >> 3;
		int num2 = num * num;
		terrainSize = Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z);
		shoreMapSize = num;
		shoreDistances = new float[num * num];
		shoreVectors = new Vector3[num * num];
		for (int i = 0; i < num2; i++)
		{
			shoreDistances[i] = 10000f;
			shoreVectors[i] = Vector3.one;
		}
	}

	private void GenerateShoreVector()
	{
		using (TimeWarning.New("GenerateShoreVector", 500))
		{
			GenerateShoreVector(out shoreDistances, out shoreVectors);
		}
	}

	private void ReleaseShoreVector()
	{
		shoreDistances = null;
		shoreVectors = null;
	}

	private void GenerateShoreVector(out float[] distances, out Vector3[] vectors)
	{
		float num = terrainSize / (float)shoreMapSize;
		Vector3 position = terrain.GetPosition();
		int num2 = LayerMask.NameToLayer("Terrain");
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(shoreMapSize * shoreMapSize, Allocator.TempJob);
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(shoreMapSize * shoreMapSize, Allocator.TempJob);
		for (int i = 0; i < shoreMapSize; i++)
		{
			for (int j = 0; j < shoreMapSize; j++)
			{
				float x = ((float)j + 0.5f) * num;
				float z = ((float)i + 0.5f) * num;
				Vector3 from = new Vector3(position.x, 0f, position.z) + new Vector3(x, 1000f, z);
				Vector3 down = Vector3.down;
				commands[i * shoreMapSize + j] = new RaycastCommand(from, down);
			}
		}
		RaycastCommand.ScheduleBatch(commands, results, 1).Complete();
		byte[] image = new byte[shoreMapSize * shoreMapSize];
		distances = new float[shoreMapSize * shoreMapSize];
		vectors = new Vector3[shoreMapSize * shoreMapSize];
		int k = 0;
		int num3 = 0;
		for (; k < shoreMapSize; k++)
		{
			int num4 = 0;
			while (num4 < shoreMapSize)
			{
				bool flag = results[k * shoreMapSize + num4].collider.gameObject.layer == num2;
				image[num3] = (byte)(flag ? 255u : 0u);
				distances[num3] = (flag ? 256 : 0);
				num4++;
				num3++;
			}
		}
		ref int size = ref shoreMapSize;
		byte threshold = 127;
		DistanceField.Generate(in size, in threshold, in image, ref distances);
		DistanceField.ApplyGaussianBlur(shoreMapSize, distances, 0);
		DistanceField.GenerateVectors(in shoreMapSize, in distances, ref vectors);
		results.Dispose();
		commands.Dispose();
	}

	public float GetCoarseDistanceToShore(Vector3 pos)
	{
		Vector2 uv = default(Vector2);
		uv.x = (pos.x - TerrainMeta.Position.x) * TerrainMeta.OneOverSize.x;
		uv.y = (pos.z - TerrainMeta.Position.z) * TerrainMeta.OneOverSize.z;
		return GetCoarseDistanceToShore(uv);
	}

	public float GetCoarseDistanceToShore(Vector2 uv)
	{
		int num = shoreMapSize;
		int num2 = num - 1;
		float num3 = uv.x * (float)num2;
		float num4 = uv.y * (float)num2;
		int num5 = (int)num3;
		int num6 = (int)num4;
		float num7 = num3 - (float)num5;
		float num8 = num4 - (float)num6;
		num5 = ((num5 >= 0) ? num5 : 0);
		num6 = ((num6 >= 0) ? num6 : 0);
		num5 = ((num5 <= num2) ? num5 : num2);
		num6 = ((num6 <= num2) ? num6 : num2);
		int num9 = ((num3 < (float)num2) ? 1 : 0);
		int num10 = ((num4 < (float)num2) ? num : 0);
		int num11 = num6 * num + num5;
		int num12 = num11 + num9;
		int num13 = num11 + num10;
		int num14 = num13 + num9;
		float num15 = shoreDistances[num11];
		float num16 = shoreDistances[num12];
		float num17 = shoreDistances[num13];
		float num18 = shoreDistances[num14];
		float num19 = (num16 - num15) * num7 + num15;
		return ((num18 - num17) * num7 + num17 - num19) * num8 + num19;
	}

	public Vector3 GetCoarseVectorToShore(Vector3 pos)
	{
		Vector2 uv = default(Vector2);
		uv.x = (pos.x - TerrainMeta.Position.x) * TerrainMeta.OneOverSize.x;
		uv.y = (pos.z - TerrainMeta.Position.z) * TerrainMeta.OneOverSize.z;
		return GetCoarseVectorToShore(uv);
	}

	public Vector3 GetCoarseVectorToShore(Vector2 uv)
	{
		int num = shoreMapSize;
		int num2 = num - 1;
		float num3 = uv.x * (float)num2;
		float num4 = uv.y * (float)num2;
		int num5 = (int)num3;
		int num6 = (int)num4;
		float num7 = num3 - (float)num5;
		float num8 = num4 - (float)num6;
		num5 = ((num5 >= 0) ? num5 : 0);
		num6 = ((num6 >= 0) ? num6 : 0);
		num5 = ((num5 <= num2) ? num5 : num2);
		num6 = ((num6 <= num2) ? num6 : num2);
		int num9 = ((num3 < (float)num2) ? 1 : 0);
		int num10 = ((num4 < (float)num2) ? num : 0);
		int num11 = num6 * num + num5;
		int num12 = num11 + num9;
		int num13 = num11 + num10;
		int num14 = num13 + num9;
		Vector3 vector = shoreVectors[num11];
		Vector3 vector2 = shoreVectors[num12];
		Vector3 vector3 = shoreVectors[num13];
		Vector3 vector4 = shoreVectors[num14];
		Vector3 vector5 = default(Vector3);
		vector5.x = (vector2.x - vector.x) * num7 + vector.x;
		vector5.y = (vector2.y - vector.y) * num7 + vector.y;
		vector5.z = (vector2.z - vector.z) * num7 + vector.z;
		Vector3 vector6 = default(Vector3);
		vector6.x = (vector4.x - vector3.x) * num7 + vector3.x;
		vector6.y = (vector4.y - vector3.y) * num7 + vector3.y;
		vector6.z = (vector4.z - vector3.z) * num7 + vector3.z;
		float x = (vector6.x - vector5.x) * num8 + vector5.x;
		float y = (vector6.y - vector5.y) * num8 + vector5.y;
		float z = (vector6.z - vector5.z) * num8 + vector5.z;
		return new Vector3(x, y, z);
	}

	private void CheckInstance()
	{
		instance = ((instance != null) ? instance : this);
	}

	private void Awake()
	{
		CheckInstance();
	}

	public override void Setup()
	{
		InitializeShoreVector();
	}

	public override void PostSetup()
	{
		TerrainMeta component = GetComponent<TerrainMeta>();
		if (component == null || component.config == null)
		{
			Debug.LogError("[TerrainTexturing] Missing TerrainMeta or TerrainConfig not assigned.");
			return;
		}
		Shutdown();
		InitializeCoarseHeightSlope();
		GenerateShoreVector();
		initialized = true;
	}

	private void Shutdown()
	{
		ReleaseBasePyramid();
		ReleaseCoarseHeightSlope();
		ReleaseShoreVector();
		initialized = false;
	}

	private void OnEnable()
	{
		CheckInstance();
	}

	private void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			Shutdown();
		}
	}

	private void Update()
	{
		if (initialized)
		{
			UpdateBasePyramid();
			UpdateCoarseHeightSlope();
		}
	}
}
