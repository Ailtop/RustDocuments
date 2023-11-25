using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PathList
{
	public enum Side
	{
		Both = 0,
		Left = 1,
		Right = 2,
		Any = 3
	}

	public enum Placement
	{
		Center = 0,
		Side = 1
	}

	public enum Alignment
	{
		None = 0,
		Neighbor = 1,
		Forward = 2,
		Inward = 3
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
				meshData[i].Apply(Meshes[i] = new Mesh());
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

	public int Hierarchy;

	public PathFinder.Node ProcgenStartNode;

	public PathFinder.Node ProcgenEndNode;

	public const float StepSize = 1f;

	private static float[] placements = new float[3] { 0f, -1f, 1f };

	public PathList(string name, Vector3[] points)
	{
		Name = name;
		Path = new PathInterpolator(points);
	}

	private void SpawnObjectsNeighborAligned(ref uint seed, Prefab[] prefabs, List<Vector3> positions, SpawnFilter filter = null)
	{
		if (positions.Count < 2)
		{
			return;
		}
		List<Prefab> obj = Pool.GetList<Prefab>();
		for (int i = 0; i < positions.Count; i++)
		{
			int index = Mathf.Max(i - 1, 0);
			int index2 = Mathf.Min(i + 1, positions.Count - 1);
			Vector3 position = positions[i];
			Quaternion rotation = Quaternion.LookRotation((positions[index2] - positions[index]).XZ3D());
			SpawnObject(ref seed, prefabs, position, rotation, obj, out var spawned, positions.Count, i, filter);
			if (spawned != null)
			{
				obj.Add(spawned);
			}
		}
		Pool.FreeList(ref obj);
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

	private bool SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 position, Quaternion rotation, List<Prefab> previousSpawns, out Prefab spawned, int pathLength, int index, SpawnFilter filter = null)
	{
		spawned = null;
		Prefab replacement = ArrayEx.GetRandom(prefabs, ref seed);
		replacement.ApplySequenceReplacement(previousSpawns, ref replacement, prefabs, pathLength, index);
		Vector3 pos = position;
		Quaternion rot = rotation;
		Vector3 scale = replacement.Object.transform.localScale;
		replacement.ApplyDecorComponents(ref pos, ref rot, ref scale);
		if (!replacement.ApplyTerrainAnchors(ref pos, rot, scale, filter))
		{
			return false;
		}
		World.AddPrefab(Name, replacement, pos, rot, scale);
		spawned = replacement;
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
		Vector3 vector = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector2 = pos + placements[i] * vector;
			if (obj.HeightToTerrain)
			{
				vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
			}
			if (filter.Test(vector2))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (SpawnObject(ref seed, prefabs, vector2, rotation, filter))
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
		Vector3 vector = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector2 = pos + placements[i] * vector;
			if (obj.HeightToTerrain)
			{
				vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
			}
			if (filter.Test(vector2))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (CheckObjects(prefabs, vector2, rotation, filter))
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
		Vector3 vector = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num3 = distance * 0.25f;
		float num4 = distance * 0.5f;
		float num5 = Path.StartOffset + num4;
		float num6 = Path.Length - Path.EndOffset - num4;
		for (float num7 = num5; num7 <= num6; num7 += num3)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num7) : Path.GetPoint(num7));
			if ((vector2 - vector).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num7);
			Vector3 vector3 = rot90 * tangent;
			for (int i = 0; i < array2.Length; i++)
			{
				int num8 = (num2 + i) % array2.Length;
				if ((side == Side.Left && num8 != 0) || (side == Side.Right && num8 != 1))
				{
					continue;
				}
				float num9 = array2[num8];
				Vector3 vector4 = vector2;
				vector4.x += vector3.x * num9;
				vector4.z += vector3.z * num9;
				float normX = TerrainMeta.NormalizeX(vector4.x);
				float normZ = TerrainMeta.NormalizeZ(vector4.z);
				if (filter.GetFactor(normX, normZ) < SeedRandom.Value(ref seed))
				{
					continue;
				}
				if (density >= SeedRandom.Value(ref seed))
				{
					vector4.y = heightMap.GetHeight(normX, normZ);
					if (obj.Alignment == Alignment.None)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(Vector3.zero), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Forward)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(tangent * num9), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Inward)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(tangent * num9) * rot270, filter))
						{
							continue;
						}
					}
					else
					{
						list.Add(vector4);
					}
				}
				num2 = num8;
				vector = vector2;
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
		Vector3 vector = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num = distance * 0.25f;
		float num2 = distance * 0.5f;
		float num3 = Path.StartOffset + num2;
		float num4 = Path.Length - Path.EndOffset - num2;
		for (float num5 = num3; num5 <= num4; num5 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num5) : Path.GetPoint(num5));
			if ((vector2 - vector).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num5);
			Vector3 forward = rot90 * tangent;
			Vector3 vector3 = vector2;
			vector3.x += SeedRandom.Range(ref seed, 0f - dithering, dithering);
			vector3.z += SeedRandom.Range(ref seed, 0f - dithering, dithering);
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
					if (!SpawnObject(ref seed, array, vector3, Quaternion.identity, filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Forward)
				{
					if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(tangent), filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Inward)
				{
					if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(forward), filter))
					{
						continue;
					}
				}
				else
				{
					list.Add(vector3);
				}
			}
			vector = vector2;
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
		Vector3 vector = Path.GetEndPoint() - startPoint;
		float magnitude = vector.magnitude;
		Vector3 vector2 = vector / magnitude;
		float num = magnitude / obj.Distance;
		int num2 = Mathf.RoundToInt(num);
		float num3 = 0.5f * (num - (float)num2);
		Vector3 vector3 = obj.Distance * vector2;
		Vector3 vector4 = startPoint + (0.5f + num3) * vector3;
		Quaternion rotation = Quaternion.LookRotation(vector2);
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainWaterMap waterMap = TerrainMeta.WaterMap;
		for (int i = 0; i < num2; i++)
		{
			float num4 = Mathf.Max(heightMap.GetHeight(vector4), waterMap.GetHeight(vector4)) - 1f;
			if (vector4.y > num4)
			{
				SpawnObject(ref seed, array, vector4, rotation);
			}
			vector4 += vector3;
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

	public void AdjustTerrainHeight(float intensity = 1f, float fade = 1f)
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		_ = TerrainMeta.TopologyMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float outerFade = OuterFade * fade;
		float innerFade = InnerFade;
		float offset = TerrainOffset * TerrainMeta.OneOverSize.y;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 vector2 = startPoint;
		Line prev_line = new Line(startPoint, startPoint + startTangent * num);
		Vector3 vector3 = startPoint - vector * (num2 + outerPadding + outerFade);
		Vector3 vector4 = startPoint + vector * (num2 + outerPadding + outerFade);
		Vector3 vector5 = vector2;
		Vector3 v = startTangent;
		Line cur_line = prev_line;
		Vector3 vector6 = vector3;
		Vector3 vector7 = vector4;
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector8 = (Spline ? Path.GetPointCubicHermite(num4 + num) : Path.GetPoint(num4 + num));
			Vector3 tangent = Path.GetTangent(num4 + num);
			Line next_line = new Line(vector8, vector8 + tangent * num);
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector5.x, vector5.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a = (startPoint - vector5).Magnitude2D();
				float b = (endPoint - vector5).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			normalized = v.XZ3D().normalized;
			vector = rot90 * normalized;
			vector6 = vector5 - vector * (radius + outerPadding + outerFade);
			vector7 = vector5 + vector * (radius + outerPadding + outerFade);
			float yn = TerrainMeta.NormalizeY((vector5.y + vector2.y) * 0.5f);
			heightmap.ForEach(vector3, vector4, vector6, vector7, delegate(int x, int z)
			{
				Vector3 vector9 = TerrainMeta.Denormalize(new Vector3(heightmap.Coordinate(x), z: heightmap.Coordinate(z), y: yn));
				Vector3 vector10 = prev_line.ClosestPoint2D(vector9);
				Vector3 vector11 = cur_line.ClosestPoint2D(vector9);
				Vector3 vector12 = next_line.ClosestPoint2D(vector9);
				float num5 = (vector9 - vector10).Magnitude2D();
				float num6 = (vector9 - vector11).Magnitude2D();
				float num7 = (vector9 - vector12).Magnitude2D();
				float value = num6;
				Vector3 vector13 = vector11;
				if (!(num6 <= num5) || !(num6 <= num7))
				{
					if (num5 <= num7)
					{
						value = num5;
						vector13 = vector10;
					}
					else
					{
						value = num7;
						vector13 = vector12;
					}
				}
				float num8 = Mathf.InverseLerp(radius + outerPadding + outerFade, radius + outerPadding, value);
				float t = Mathf.InverseLerp(radius - innerPadding, radius - innerPadding - innerFade, value);
				float num9 = TerrainMeta.NormalizeY(vector13.y);
				heightmap.SetHeight(x, z, num9 + Mathf.SmoothStep(0f, offset, t), intensity * opacity * num8);
			});
			vector2 = vector5;
			vector3 = vector6;
			vector4 = vector7;
			prev_line = cur_line;
			vector5 = vector8;
			v = tangent;
			cur_line = next_line;
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
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * (num2 + outerPadding);
		Vector3 v2 = startPoint + vector * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector2.x, vector2.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a = (startPoint - vector2).Magnitude2D();
				float b = (endPoint - vector2).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * (radius + outerPadding);
			Vector3 vector4 = vector2 + vector * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector2.y);
			splatmap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(splatmap.Coordinate(x), z: splatmap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				float value = (vector5 - vector6).Magnitude2D();
				float num5 = Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value);
				splatmap.SetSplat(x, z, Splat, num5 * opacity);
			});
			v = vector3;
			v2 = vector4;
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
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * (num2 + outerPadding);
		Vector3 v2 = startPoint + vector * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = Mathf.Lerp(num2, num2 * randomScale, Noise.Billow(vector2.x, vector2.z, 2, 0.005f));
			if (!Path.Circular)
			{
				float a = (startPoint - vector2).Magnitude2D();
				float b = (endPoint - vector2).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * (radius + outerPadding);
			Vector3 vector4 = vector2 + vector * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector2.y);
			topomap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(topomap.Coordinate(x), z: topomap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				float value = (vector5 - vector6).Magnitude2D();
				if (Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value) * opacity > 0.3f)
				{
					topomap.AddTopology(x, z, Topology);
				}
			});
			v = vector3;
			v2 = vector4;
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
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * radius;
		Vector3 v2 = startPoint + vector * radius;
		float num2 = Path.Length + num;
		for (float num3 = 0f; num3 < num2; num3 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num3) : Path.GetPoint(num3));
			startTangent = Path.GetTangent(num3);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * radius;
			Vector3 vector4 = vector2 + vector * radius;
			float yn = TerrainMeta.NormalizeY(vector2.y);
			placementmap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(placementmap.Coordinate(x), z: placementmap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				if ((vector5 - vector6).Magnitude2D() <= radius)
				{
					placementmap.SetBlocked(x, z);
				}
			});
			v = vector3;
			v2 = vector4;
		}
	}

	public List<MeshObject> CreateMesh(Mesh[] meshes, float normalSmoothing, bool snapToTerrain, bool snapStartToTerrain, bool snapEndToTerrain)
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
		Bounds bounds = meshes[^1].bounds;
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
		_ = array[0].vertices.Length;
		_ = array[0].triangles.Length;
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
						Vector3 vector3 = data.normals[n];
						Vector4 vector4 = data.tangents[n];
						float t = (vector2.x - min.x) / size.x;
						float num7 = vector2.y - min.y;
						float num8 = (vector2.z - min.z) / size.z;
						float num9 = num6 + num8 * num4;
						Vector3 vector5 = (Spline ? Path.GetPointCubicHermite(num9) : Path.GetPoint(num9));
						Vector3 tangent = Path.GetTangent(num9);
						Vector3 normalized = tangent.XZ3D().normalized;
						Vector3 vector6 = rot90 * normalized;
						Vector3 vector7 = Vector3.Cross(tangent, vector6);
						Quaternion quaternion = Quaternion.LookRotation(normalized, vector7);
						float num10 = Mathf.Lerp(num5, num5 * randomScale, Noise.Billow(vector5.x, vector5.z, 2, 0.005f));
						Vector3 vector8 = vector5 - vector6 * num10;
						Vector3 vector9 = vector5 + vector6 * num10;
						if (snapToTerrain)
						{
							vector8.y = heightMap.GetHeight(vector8);
							vector9.y = heightMap.GetHeight(vector9);
						}
						vector8 += vector7 * meshOffset;
						vector9 += vector7 * meshOffset;
						vector2 = Vector3.Lerp(vector8, vector9, t);
						if ((snapStartToTerrain && num9 < 0.1f) || (snapEndToTerrain && num9 > Path.Length - 0.1f))
						{
							vector2.y = heightMap.GetHeight(vector2);
						}
						else
						{
							vector2.y += num7;
						}
						vector2 -= vector;
						vector3 = quaternion * vector3;
						vector4 = quaternion * vector4;
						if (normalSmoothing > 0f)
						{
							vector3 = Vector3.Slerp(vector3, Vector3.up, normalSmoothing);
						}
						meshData.vertices.Add(vector2);
						meshData.normals.Add(vector3);
						meshData.tangents.Add(vector4);
						meshData.uv.Add(item);
					}
					for (int num11 = 0; num11 < data.triangles.Length; num11++)
					{
						int num12 = data.triangles[num11];
						meshData.triangles.Add(count + num12);
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
