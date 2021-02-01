using System;
using System.Collections.Generic;
using UnityEngine;

public class PathList
{
	public enum Side
	{
		Both,
		Left,
		Right,
		Any
	}

	public enum Placement
	{
		Center,
		Side
	}

	public enum Alignment
	{
		None,
		Neighbor,
		Forward,
		Inward
	}

	[Serializable]
	public class BasicObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Placement Placement;

		public bool AlignToNormal = true;

		public bool HeightToTerrain = true;

		public float Offset;
	}

	[Serializable]
	public class SideObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Side Side;

		public Alignment Alignment;

		public float Density = 1f;

		public float Distance = 25f;

		public float Offset = 2f;
	}

	[Serializable]
	public class PathObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Alignment Alignment;

		public float Density = 1f;

		public float Distance = 5f;

		public float Dithering = 5f;
	}

	[Serializable]
	public class BridgeObject
	{
		public string Folder;

		public float Distance = 10f;
	}

	public class MeshObject
	{
		public Vector3 Position;

		public Mesh[] Meshes;

		public MeshObject(Vector3 meshPivot, MeshData[] meshData)
		{
			Position = meshPivot;
			Meshes = new Mesh[meshData.Length];
			for (int i = 0; i < Meshes.Length; i++)
			{
				MeshData obj = meshData[i];
				Mesh mesh = (Meshes[i] = new Mesh());
				obj.Apply(mesh);
				mesh.RecalculateTangents();
			}
		}
	}

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	private static Quaternion rot180 = Quaternion.Euler(0f, 180f, 0f);

	private static Quaternion rot270 = Quaternion.Euler(0f, 270f, 0f);

	public string Name;

	public PathInterpolator Path;

	public bool Spline;

	public bool Start;

	public bool End;

	public float Width;

	public float InnerPadding;

	public float OuterPadding;

	public float InnerFade;

	public float OuterFade;

	public float RandomScale;

	public float MeshOffset;

	public float TerrainOffset;

	public int Topology;

	public int Splat;

	public PathFinder.Node ProcgenStartNode;

	public PathFinder.Node ProcgenEndNode;

	public const float StepSize = 1f;

	private static float[] placements = new float[3]
	{
		0f,
		-1f,
		1f
	};

	public bool IsExtraWide => Width > 10f;

	public bool IsExtraNarrow => Width < 5f;

	public PathList(string name, Vector3[] points)
	{
		Name = name;
		Path = new PathInterpolator(points);
	}

	private void SpawnObjectsNeighborAligned(ref uint seed, Prefab[] prefabs, List<Vector3> positions, SpawnFilter filter = null)
	{
		if (positions.Count >= 2)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				int index = Mathf.Max(i - 1, 0);
				int index2 = Mathf.Min(i + 1, positions.Count - 1);
				Vector3 position = positions[i];
				Quaternion rotation = Quaternion.LookRotation((positions[index2] - positions[index]).XZ3D());
				SpawnObject(ref seed, prefabs, position, rotation, filter);
			}
		}
	}

	private bool SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 position, Quaternion rotation, SpawnFilter filter = null)
	{
		Prefab random = ArrayEx.GetRandom(prefabs, ref seed);
		Vector3 pos = position;
		Quaternion rot = rotation;
		Vector3 scale = random.Object.transform.localScale;
		random.ApplyDecorComponents(ref pos, ref rot, ref scale);
		if (!random.ApplyTerrainAnchors(ref pos, rot, scale, filter))
		{
			return false;
		}
		World.AddPrefab(Name, random, pos, rot, scale);
		return true;
	}

	private bool CheckObjects(Prefab[] prefabs, Vector3 position, Quaternion rotation, SpawnFilter filter = null)
	{
		foreach (Prefab obj in prefabs)
		{
			Vector3 pos = position;
			Vector3 localScale = obj.Object.transform.localScale;
			if (!obj.ApplyTerrainAnchors(ref pos, rotation, localScale, filter))
			{
				return false;
			}
		}
		return true;
	}

	private void SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 pos, Vector3 dir, BasicObject obj)
	{
		if (!obj.AlignToNormal)
		{
			dir = dir.XZ3D().normalized;
		}
		SpawnFilter filter = obj.Filter;
		Vector3 a = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector = pos + placements[i] * a;
			if (obj.HeightToTerrain)
			{
				vector.y = TerrainMeta.HeightMap.GetHeight(vector);
			}
			if (filter.Test(vector))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (SpawnObject(ref seed, prefabs, vector, rotation, filter))
				{
					break;
				}
			}
		}
	}

	private bool CheckObjects(Prefab[] prefabs, Vector3 pos, Vector3 dir, BasicObject obj)
	{
		if (!obj.AlignToNormal)
		{
			dir = dir.XZ3D().normalized;
		}
		SpawnFilter filter = obj.Filter;
		Vector3 a = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector = pos + placements[i] * a;
			if (obj.HeightToTerrain)
			{
				vector.y = TerrainMeta.HeightMap.GetHeight(vector);
			}
			if (filter.Test(vector))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (CheckObjects(prefabs, vector, rotation, filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SpawnSide(ref uint seed, SideObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Side side = obj.Side;
		SpawnFilter filter = obj.Filter;
		float density = obj.Density;
		float distance = obj.Distance;
		float num = Width * 0.5f + obj.Offset;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		float[] array2 = new float[2]
		{
			0f - num,
			num
		};
		int num2 = 0;
		Vector3 b = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num3 = distance * 0.25f;
		float num4 = distance * 0.5f;
		float num5 = Path.StartOffset + num4;
		float num6 = Path.Length - Path.EndOffset - num4;
		for (float num7 = num5; num7 <= num6; num7 += num3)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num7) : Path.GetPoint(num7));
			if ((vector - b).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num7);
			Vector3 vector2 = rot90 * tangent;
			for (int i = 0; i < array2.Length; i++)
			{
				int num8 = (num2 + i) % array2.Length;
				if ((side == Side.Left && num8 != 0) || (side == Side.Right && num8 != 1))
				{
					continue;
				}
				float num9 = array2[num8];
				Vector3 vector3 = vector;
				vector3.x += vector2.x * num9;
				vector3.z += vector2.z * num9;
				float normX = TerrainMeta.NormalizeX(vector3.x);
				float normZ = TerrainMeta.NormalizeZ(vector3.z);
				if (filter.GetFactor(normX, normZ) < SeedRandom.Value(ref seed))
				{
					continue;
				}
				if (density >= SeedRandom.Value(ref seed))
				{
					vector3.y = heightMap.GetHeight(normX, normZ);
					if (obj.Alignment == Alignment.None)
					{
						if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(Vector3.zero), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Forward)
					{
						if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(tangent * num9), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Inward)
					{
						if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(tangent * num9) * rot270, filter))
						{
							continue;
						}
					}
					else
					{
						list.Add(vector3);
					}
				}
				num2 = num8;
				b = vector;
				if (side == Side.Any)
				{
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			SpawnObjectsNeighborAligned(ref seed, array, list, filter);
		}
	}

	public void SpawnAlong(ref uint seed, PathObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		SpawnFilter filter = obj.Filter;
		float density = obj.Density;
		float distance = obj.Distance;
		float dithering = obj.Dithering;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 b = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num = distance * 0.25f;
		float num2 = distance * 0.5f;
		float num3 = Path.StartOffset + num2;
		float num4 = Path.Length - Path.EndOffset - num2;
		for (float num5 = num3; num5 <= num4; num5 += num)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num5) : Path.GetPoint(num5));
			if ((vector - b).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num5);
			Vector3 forward = rot90 * tangent;
			Vector3 vector2 = vector;
			vector2.x += SeedRandom.Range(ref seed, 0f - dithering, dithering);
			vector2.z += SeedRandom.Range(ref seed, 0f - dithering, dithering);
			float normX = TerrainMeta.NormalizeX(vector2.x);
			float normZ = TerrainMeta.NormalizeZ(vector2.z);
			if (filter.GetFactor(normX, normZ) < SeedRandom.Value(ref seed))
			{
				continue;
			}
			if (density >= SeedRandom.Value(ref seed))
			{
				vector2.y = heightMap.GetHeight(normX, normZ);
				if (obj.Alignment == Alignment.None)
				{
					if (!SpawnObject(ref seed, array, vector2, Quaternion.identity, filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Forward)
				{
					if (!SpawnObject(ref seed, array, vector2, Quaternion.LookRotation(tangent), filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Inward)
				{
					if (!SpawnObject(ref seed, array, vector2, Quaternion.LookRotation(forward), filter))
					{
						continue;
					}
				}
				else
				{
					list.Add(vector2);
				}
			}
			b = vector;
		}
		if (list.Count > 0)
		{
			SpawnObjectsNeighborAligned(ref seed, array, list, filter);
		}
	}

	public void SpawnBridge(ref uint seed, BridgeObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 a = Path.GetEndPoint() - startPoint;
		float magnitude = a.magnitude;
		Vector3 vector = a / magnitude;
		float num = magnitude / obj.Distance;
		int num2 = Mathf.RoundToInt(num);
		float num3 = 0.5f * (num - (float)num2);
		Vector3 vector2 = obj.Distance * vector;
		Vector3 vector3 = startPoint + (0.5f + num3) * vector2;
		Quaternion rotation = Quaternion.LookRotation(vector);
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainWaterMap waterMap = TerrainMeta.WaterMap;
		for (int i = 0; i < num2; i++)
		{
			float num4 = Mathf.Max(heightMap.GetHeight(vector3), waterMap.GetHeight(vector3)) - 1f;
			if (vector3.y > num4)
			{
				SpawnObject(ref seed, array, vector3, rotation);
			}
			vector3 += vector2;
		}
	}

	public void SpawnStart(ref uint seed, BasicObject obj)
	{
		if (Start && !string.IsNullOrEmpty(obj.Folder))
		{
			Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
			if (array == null || array.Length == 0)
			{
				Debug.LogError("Empty decor folder: " + obj.Folder);
				return;
			}
			Vector3 startPoint = Path.GetStartPoint();
			Vector3 startTangent = Path.GetStartTangent();
			SpawnObject(ref seed, array, startPoint, startTangent, obj);
		}
	}

	public void SpawnEnd(ref uint seed, BasicObject obj)
	{
		if (End && !string.IsNullOrEmpty(obj.Folder))
		{
			Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
			if (array == null || array.Length == 0)
			{
				Debug.LogError("Empty decor folder: " + obj.Folder);
				return;
			}
			Vector3 endPoint = Path.GetEndPoint();
			Vector3 dir = -Path.GetEndTangent();
			SpawnObject(ref seed, array, endPoint, dir, obj);
		}
	}

	public void TrimStart(BasicObject obj)
	{
		if (!Start || string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3[] points = Path.Points;
		Vector3[] tangents = Path.Tangents;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = points[Path.MinIndex + i];
			Vector3 dir = tangents[Path.MinIndex + i];
			if (CheckObjects(array, pos, dir, obj))
			{
				Path.MinIndex += i;
				break;
			}
		}
	}

	public void TrimEnd(BasicObject obj)
	{
		if (!End || string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3[] points = Path.Points;
		Vector3[] tangents = Path.Tangents;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = points[Path.MaxIndex - i];
			Vector3 dir = -tangents[Path.MaxIndex - i];
			if (CheckObjects(array, pos, dir, obj))
			{
				Path.MaxIndex -= i;
				break;
			}
		}
	}

	public void TrimTopology(int topology)
	{
		Vector3[] points = Path.Points;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 worldPos = points[Path.MinIndex + i];
			if (!TerrainMeta.TopologyMap.GetTopology(worldPos, topology))
			{
				Path.MinIndex += i;
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			Vector3 worldPos2 = points[Path.MaxIndex - j];
			if (!TerrainMeta.TopologyMap.GetTopology(worldPos2, topology))
			{
				Path.MaxIndex -= j;
				break;
			}
		}
	}

	public void ResetTrims()
	{
		Path.MinIndex = Path.DefaultMinIndex;
		Path.MaxIndex = Path.DefaultMaxIndex;
	}

	public void AdjustTerrainHeight()
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float outerFade = OuterFade;
		float innerFade = InnerFade;
		float offset = TerrainOffset * TerrainMeta.OneOverSize.y;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 a = rot90 * normalized;
		Vector3 v = startPoint - a * (num2 + outerPadding + outerFade);
		Vector3 v2 = startPoint + a * (num2 + outerPadding + outerFade);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector.x, vector.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a2 = (startPoint - vector).Magnitude2D();
				float b = (endPoint - vector).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a2, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			a = rot90 * normalized;
			Ray ray = new Ray(vector, startTangent);
			Vector3 vector2 = vector - a * (radius + outerPadding + outerFade);
			Vector3 vector3 = vector + a * (radius + outerPadding + outerFade);
			float yn = TerrainMeta.NormalizeY(vector.y);
			heightmap.ForEach(v, v2, vector2, vector3, delegate(int x, int z)
			{
				float num5 = heightmap.Coordinate(x);
				float num6 = heightmap.Coordinate(z);
				if ((topomap.GetTopology(num5, num6) & Topology) == 0)
				{
					Vector3 vector4 = TerrainMeta.Denormalize(new Vector3(num5, yn, num6));
					Vector3 b2 = RayEx.ClosestPoint(ray, vector4);
					float value = (vector4 - b2).Magnitude2D();
					float t = Mathf.InverseLerp(radius + outerPadding + outerFade, radius + outerPadding, value);
					float t2 = Mathf.InverseLerp(radius - innerPadding, radius - innerPadding - innerFade, value);
					float num7 = TerrainMeta.NormalizeY(b2.y);
					t = Mathf.SmoothStep(0f, 1f, t);
					t2 = Mathf.SmoothStep(0f, 1f, t2);
					heightmap.SetHeight(x, z, num7 + offset * t2, opacity * t);
				}
			});
			v = vector2;
			v2 = vector3;
		}
	}

	public void AdjustTerrainTexture()
	{
		if (Splat == 0)
		{
			return;
		}
		TerrainSplatMap splatmap = TerrainMeta.SplatMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 a = rot90 * normalized;
		Vector3 v = startPoint - a * (num2 + outerPadding);
		Vector3 v2 = startPoint + a * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector.x, vector.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a2 = (startPoint - vector).Magnitude2D();
				float b = (endPoint - vector).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a2, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			a = rot90 * normalized;
			Ray ray = new Ray(vector, startTangent);
			Vector3 vector2 = vector - a * (radius + outerPadding);
			Vector3 vector3 = vector + a * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector.y);
			splatmap.ForEach(v, v2, vector2, vector3, delegate(int x, int z)
			{
				Vector3 vector4 = TerrainMeta.Denormalize(new Vector3(splatmap.Coordinate(x), z: splatmap.Coordinate(z), y: yn));
				Vector3 b2 = RayEx.ClosestPoint(ray, vector4);
				float value = (vector4 - b2).Magnitude2D();
				float num5 = Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value);
				splatmap.SetSplat(x, z, Splat, num5 * opacity);
			});
			v = vector2;
			v2 = vector3;
		}
	}

	public void AdjustTerrainTopology()
	{
		if (Topology == 0)
		{
			return;
		}
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 a = rot90 * normalized;
		Vector3 v = startPoint - a * (num2 + outerPadding);
		Vector3 v2 = startPoint + a * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector.x, vector.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a2 = (startPoint - vector).Magnitude2D();
				float b = (endPoint - vector).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a2, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			a = rot90 * normalized;
			Ray ray = new Ray(vector, startTangent);
			Vector3 vector2 = vector - a * (radius + outerPadding);
			Vector3 vector3 = vector + a * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector.y);
			topomap.ForEach(v, v2, vector2, vector3, delegate(int x, int z)
			{
				Vector3 vector4 = TerrainMeta.Denormalize(new Vector3(topomap.Coordinate(x), z: topomap.Coordinate(z), y: yn));
				Vector3 b2 = RayEx.ClosestPoint(ray, vector4);
				float value = (vector4 - b2).Magnitude2D();
				if (Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value) * opacity > 0.3f)
				{
					topomap.AddTopology(x, z, Topology);
				}
			});
			v = vector2;
			v2 = vector3;
		}
	}

	public void AdjustPlacementMap(float width)
	{
		TerrainPlacementMap placementmap = TerrainMeta.PlacementMap;
		float num = 1f;
		float radius = width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 a = rot90 * normalized;
		Vector3 v = startPoint - a * radius;
		Vector3 v2 = startPoint + a * radius;
		float num2 = Path.Length + num;
		for (float num3 = 0f; num3 < num2; num3 += num)
		{
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(num3) : Path.GetPoint(num3));
			startTangent = Path.GetTangent(num3);
			normalized = startTangent.XZ3D().normalized;
			a = rot90 * normalized;
			Ray ray = new Ray(vector, startTangent);
			Vector3 vector2 = vector - a * radius;
			Vector3 vector3 = vector + a * radius;
			float yn = TerrainMeta.NormalizeY(vector.y);
			placementmap.ForEach(v, v2, vector2, vector3, delegate(int x, int z)
			{
				Vector3 vector4 = TerrainMeta.Denormalize(new Vector3(placementmap.Coordinate(x), z: placementmap.Coordinate(z), y: yn));
				Vector3 b = RayEx.ClosestPoint(ray, vector4);
				if ((vector4 - b).Magnitude2D() <= radius)
				{
					placementmap.SetBlocked(x, z);
				}
			});
			v = vector2;
			v2 = vector3;
		}
	}

	public List<MeshObject> CreateMesh(Mesh[] meshes, float normalSmoothing)
	{
		MeshCache.Data[] array = new MeshCache.Data[meshes.Length];
		MeshData[] array2 = new MeshData[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			array[i] = MeshCache.Get(meshes[i]);
			array2[i] = new MeshData();
		}
		MeshData[] array3 = array2;
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j].AllocMinimal();
		}
		Bounds bounds = meshes[0].bounds;
		Vector3 min = bounds.min;
		Vector3 size = bounds.size;
		float num = Width / bounds.size.x;
		List<MeshObject> list = new List<MeshObject>();
		int num2 = (int)(Path.Length / (num * bounds.size.z));
		int num3 = 5;
		float num4 = Path.Length / (float)num2;
		float randomScale = RandomScale;
		float meshOffset = MeshOffset;
		float num5 = Width * 0.5f;
		int num12 = array[0].vertices.Length;
		int num13 = array[0].triangles.Length;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		for (int k = 0; k < num2; k += num3)
		{
			float distance = (float)k * num4 + 0.5f * (float)num3 * num4;
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(distance) : Path.GetPoint(distance));
			for (int l = 0; l < num3 && k + l < num2; l++)
			{
				float num6 = (float)(k + l) * num4;
				for (int m = 0; m < meshes.Length; m++)
				{
					MeshCache.Data data = array[m];
					MeshData meshData = array2[m];
					int count = meshData.vertices.Count;
					for (int n = 0; n < data.vertices.Length; n++)
					{
						Vector2 item = data.uv[n];
						Vector3 vector2 = data.vertices[n];
						Vector3 point = data.normals[n];
						float t = (vector2.x - min.x) / size.x;
						float num7 = vector2.y - min.y;
						float num8 = (vector2.z - min.z) / size.z;
						float num9 = num6 + num8 * num4;
						Vector3 a = (Spline ? Path.GetPointCubicHermite(num9) : Path.GetPoint(num9));
						Vector3 tangent = Path.GetTangent(num9);
						Vector3 normalized = tangent.XZ3D().normalized;
						Vector3 vector3 = rot90 * normalized;
						Vector3 vector4 = Vector3.Cross(tangent, vector3);
						Quaternion rotation = Quaternion.FromToRotation(Vector3.up, vector4);
						float d = Mathf.Lerp(num5, num5 * randomScale, Noise.Billow(a.x, a.z, 2, 0.005f));
						Vector3 vector5 = a - vector3 * d;
						Vector3 vector6 = a + vector3 * d;
						vector5.y = heightMap.GetHeight(vector5);
						vector6.y = heightMap.GetHeight(vector6);
						vector5 += vector4 * meshOffset;
						vector6 += vector4 * meshOffset;
						vector2 = Vector3.Lerp(vector5, vector6, t);
						if (!Path.Circular && (num9 < 0.1f || num9 > Path.Length - 0.1f))
						{
							vector2.y = heightMap.GetHeight(vector2);
						}
						else
						{
							vector2.y += num7;
						}
						vector2 -= vector;
						point = rotation * point;
						if (normalSmoothing > 0f)
						{
							point = Vector3.Slerp(point, Vector3.up, normalSmoothing);
						}
						meshData.vertices.Add(vector2);
						meshData.normals.Add(point);
						meshData.uv.Add(item);
					}
					for (int num10 = 0; num10 < data.triangles.Length; num10++)
					{
						int num11 = data.triangles[num10];
						meshData.triangles.Add(count + num11);
					}
				}
			}
			list.Add(new MeshObject(vector, array2));
			array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j].Clear();
			}
		}
		array3 = array2;
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j].Free();
		}
		return list;
	}
}
