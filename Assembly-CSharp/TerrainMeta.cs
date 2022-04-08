using Oxide.Core;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainMeta : MonoBehaviour
{
	public enum PaintMode
	{
		None = 0,
		Splats = 1,
		Biomes = 2,
		Alpha = 3,
		Blend = 4,
		Field = 5,
		Cliff = 6,
		Summit = 7,
		Beachside = 8,
		Beach = 9,
		Forest = 10,
		Forestside = 11,
		Ocean = 12,
		Oceanside = 13,
		Decor = 14,
		Monument = 15,
		Road = 16,
		Roadside = 17,
		Bridge = 18,
		River = 19,
		Riverside = 20,
		Lake = 21,
		Lakeside = 22,
		Offshore = 23,
		Rail = 24,
		Railside = 25,
		Building = 26,
		Cliffside = 27,
		Mountain = 28,
		Clutter = 29,
		Alt = 30,
		Tier0 = 31,
		Tier1 = 32,
		Tier2 = 33,
		Mainland = 34,
		Hilltop = 35
	}

	public Terrain terrain;

	public TerrainConfig config;

	public PaintMode paint;

	[HideInInspector]
	public PaintMode currentPaintMode;

	public static TerrainConfig Config { get; private set; }

	public static Terrain Terrain { get; private set; }

	public static Transform Transform { get; private set; }

	public static Vector3 Position { get; private set; }

	public static Vector3 Size { get; private set; }

	public static Vector3 Center => Position + Size * 0.5f;

	public static Vector3 OneOverSize { get; private set; }

	public static Vector3 HighestPoint { get; set; }

	public static Vector3 LowestPoint { get; set; }

	public static float LootAxisAngle { get; private set; }

	public static float BiomeAxisAngle { get; private set; }

	public static TerrainData Data { get; private set; }

	public static TerrainCollider Collider { get; private set; }

	public static TerrainCollision Collision { get; private set; }

	public static TerrainPhysics Physics { get; private set; }

	public static TerrainColors Colors { get; private set; }

	public static TerrainQuality Quality { get; private set; }

	public static TerrainPath Path { get; private set; }

	public static TerrainBiomeMap BiomeMap { get; private set; }

	public static TerrainAlphaMap AlphaMap { get; private set; }

	public static TerrainBlendMap BlendMap { get; private set; }

	public static TerrainHeightMap HeightMap { get; private set; }

	public static TerrainSplatMap SplatMap { get; private set; }

	public static TerrainTopologyMap TopologyMap { get; private set; }

	public static TerrainWaterMap WaterMap { get; private set; }

	public static TerrainDistanceMap DistanceMap { get; private set; }

	public static TerrainPlacementMap PlacementMap { get; private set; }

	public static TerrainTexturing Texturing { get; private set; }

	public static bool OutOfBounds(Vector3 worldPos)
	{
		if (worldPos.x < Position.x)
		{
			return true;
		}
		if (worldPos.z < Position.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfMargin(Vector3 worldPos)
	{
		if (worldPos.x < Position.x - Size.x)
		{
			return true;
		}
		if (worldPos.z < Position.z - Size.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static Vector3 RandomPointOffshore()
	{
		float num = UnityEngine.Random.Range(-1f, 1f);
		float num2 = UnityEngine.Random.Range(0f, 100f);
		Vector3 vector = new Vector3(Mathf.Min(Size.x, 4000f) - 100f, 0f, Mathf.Min(Size.z, 4000f) - 100f);
		if (num2 < 25f)
		{
			return Center + new Vector3(0f - vector.x, 0f, num * vector.z);
		}
		if (num2 < 50f)
		{
			return Center + new Vector3(vector.x, 0f, num * vector.z);
		}
		if (num2 < 75f)
		{
			return Center + new Vector3(num * vector.x, 0f, 0f - vector.z);
		}
		return Center + new Vector3(num * vector.x, 0f, vector.z);
	}

	public static Vector3 Normalize(Vector3 worldPos)
	{
		float x = (worldPos.x - Position.x) * OneOverSize.x;
		float y = (worldPos.y - Position.y) * OneOverSize.y;
		float z = (worldPos.z - Position.z) * OneOverSize.z;
		return new Vector3(x, y, z);
	}

	public static float NormalizeX(float x)
	{
		return (x - Position.x) * OneOverSize.x;
	}

	public static float NormalizeY(float y)
	{
		return (y - Position.y) * OneOverSize.y;
	}

	public static float NormalizeZ(float z)
	{
		return (z - Position.z) * OneOverSize.z;
	}

	public static Vector3 Denormalize(Vector3 normPos)
	{
		float x = Position.x + normPos.x * Size.x;
		float y = Position.y + normPos.y * Size.y;
		float z = Position.z + normPos.z * Size.z;
		return new Vector3(x, y, z);
	}

	public static float DenormalizeX(float normX)
	{
		return Position.x + normX * Size.x;
	}

	public static float DenormalizeY(float normY)
	{
		return Position.y + normY * Size.y;
	}

	public static float DenormalizeZ(float normZ)
	{
		return Position.z + normZ * Size.z;
	}

	protected void Awake()
	{
		if (Application.isPlaying)
		{
			Shader.DisableKeyword("TERRAIN_PAINTING");
		}
	}

	public void Init(Terrain terrainOverride = null, TerrainConfig configOverride = null)
	{
		if (terrainOverride != null)
		{
			terrain = terrainOverride;
		}
		if (configOverride != null)
		{
			config = configOverride;
		}
		Terrain = terrain;
		Config = config;
		Transform = terrain.transform;
		Data = terrain.terrainData;
		Size = terrain.terrainData.size;
		OneOverSize = Size.Inverse();
		Position = terrain.GetPosition();
		Collider = terrain.GetComponent<TerrainCollider>();
		Collision = terrain.GetComponent<TerrainCollision>();
		Physics = terrain.GetComponent<TerrainPhysics>();
		Colors = terrain.GetComponent<TerrainColors>();
		Quality = terrain.GetComponent<TerrainQuality>();
		Path = terrain.GetComponent<TerrainPath>();
		BiomeMap = terrain.GetComponent<TerrainBiomeMap>();
		AlphaMap = terrain.GetComponent<TerrainAlphaMap>();
		BlendMap = terrain.GetComponent<TerrainBlendMap>();
		HeightMap = terrain.GetComponent<TerrainHeightMap>();
		SplatMap = terrain.GetComponent<TerrainSplatMap>();
		TopologyMap = terrain.GetComponent<TerrainTopologyMap>();
		WaterMap = terrain.GetComponent<TerrainWaterMap>();
		DistanceMap = terrain.GetComponent<TerrainDistanceMap>();
		PlacementMap = terrain.GetComponent<TerrainPlacementMap>();
		Texturing = terrain.GetComponent<TerrainTexturing>();
		terrain.drawInstanced = false;
		HighestPoint = new Vector3(Position.x, Position.y + Size.y, Position.z);
		LowestPoint = new Vector3(Position.x, Position.y, Position.z);
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Init(terrain, config);
		}
		uint seed = World.Seed;
		int num = SeedRandom.Range(ref seed, 0, 4) * 90;
		int num2 = SeedRandom.Range(ref seed, -45, 46);
		int num3 = SeedRandom.Sign(ref seed);
		LootAxisAngle = num;
		BiomeAxisAngle = num + num2 + num3 * 90;
	}

	public static void InitNoTerrain(bool createPath = false)
	{
		Size = new Vector3(4096f, 4096f, 4096f);
		OneOverSize = Size.Inverse();
		Position = -0.5f * Size;
	}

	public void SetupComponents()
	{
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		foreach (TerrainExtension obj in components)
		{
			obj.Setup();
			obj.isInitialized = true;
		}
	}

	public void PostSetupComponents()
	{
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].PostSetup();
		}
		Interface.CallHook("OnTerrainInitialized");
	}

	public void BindShaderProperties()
	{
		if ((bool)config)
		{
			Shader.SetGlobalTexture("Terrain_AlbedoArray", config.AlbedoArray);
			Shader.SetGlobalTexture("Terrain_NormalArray", config.NormalArray);
			Shader.SetGlobalVector("Terrain_TexelSize", new Vector2(1f / config.GetMinSplatTiling(), 1f / config.GetMinSplatTiling()));
			Shader.SetGlobalVector("Terrain_TexelSize0", new Vector4(1f / config.Splats[0].SplatTiling, 1f / config.Splats[1].SplatTiling, 1f / config.Splats[2].SplatTiling, 1f / config.Splats[3].SplatTiling));
			Shader.SetGlobalVector("Terrain_TexelSize1", new Vector4(1f / config.Splats[4].SplatTiling, 1f / config.Splats[5].SplatTiling, 1f / config.Splats[6].SplatTiling, 1f / config.Splats[7].SplatTiling));
			Shader.SetGlobalVector("Splat0_UVMIX", new Vector3(config.Splats[0].UVMIXMult, config.Splats[0].UVMIXStart, 1f / config.Splats[0].UVMIXDist));
			Shader.SetGlobalVector("Splat1_UVMIX", new Vector3(config.Splats[1].UVMIXMult, config.Splats[1].UVMIXStart, 1f / config.Splats[1].UVMIXDist));
			Shader.SetGlobalVector("Splat2_UVMIX", new Vector3(config.Splats[2].UVMIXMult, config.Splats[2].UVMIXStart, 1f / config.Splats[2].UVMIXDist));
			Shader.SetGlobalVector("Splat3_UVMIX", new Vector3(config.Splats[3].UVMIXMult, config.Splats[3].UVMIXStart, 1f / config.Splats[3].UVMIXDist));
			Shader.SetGlobalVector("Splat4_UVMIX", new Vector3(config.Splats[4].UVMIXMult, config.Splats[4].UVMIXStart, 1f / config.Splats[4].UVMIXDist));
			Shader.SetGlobalVector("Splat5_UVMIX", new Vector3(config.Splats[5].UVMIXMult, config.Splats[5].UVMIXStart, 1f / config.Splats[5].UVMIXDist));
			Shader.SetGlobalVector("Splat6_UVMIX", new Vector3(config.Splats[6].UVMIXMult, config.Splats[6].UVMIXStart, 1f / config.Splats[6].UVMIXDist));
			Shader.SetGlobalVector("Splat7_UVMIX", new Vector3(config.Splats[7].UVMIXMult, config.Splats[7].UVMIXStart, 1f / config.Splats[7].UVMIXDist));
		}
		if ((bool)HeightMap)
		{
			Shader.SetGlobalTexture("Terrain_Normal", HeightMap.NormalTexture);
		}
		if ((bool)AlphaMap)
		{
			Shader.SetGlobalTexture("Terrain_Alpha", AlphaMap.AlphaTexture);
		}
		if ((bool)BiomeMap)
		{
			Shader.SetGlobalTexture("Terrain_Biome", BiomeMap.BiomeTexture);
		}
		if ((bool)SplatMap)
		{
			Shader.SetGlobalTexture("Terrain_Control0", SplatMap.SplatTexture0);
			Shader.SetGlobalTexture("Terrain_Control1", SplatMap.SplatTexture1);
		}
		_ = (bool)WaterMap;
		if ((bool)DistanceMap)
		{
			Shader.SetGlobalTexture("Terrain_Distance", DistanceMap.DistanceTexture);
		}
		if (!terrain)
		{
			return;
		}
		Shader.SetGlobalVector("Terrain_Position", Position);
		Shader.SetGlobalVector("Terrain_Size", Size);
		Shader.SetGlobalVector("Terrain_RcpSize", OneOverSize);
		if ((bool)terrain.materialTemplate)
		{
			if (terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_BLEND_LINEAR"))
			{
				terrain.materialTemplate.DisableKeyword("_TERRAIN_BLEND_LINEAR");
			}
			if (terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_VERTEX_NORMALS"))
			{
				terrain.materialTemplate.DisableKeyword("_TERRAIN_VERTEX_NORMALS");
			}
		}
	}
}
