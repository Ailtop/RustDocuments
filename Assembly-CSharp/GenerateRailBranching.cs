using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRailBranching : ProceduralComponent
{
	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 16f;

	public const float RandomScale = 1f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	private const int MaxDepth = 250000;

	private PathList CreateSegment(int number, Vector3[] points)
	{
		return new PathList("Rail " + number, points)
		{
			Spline = true,
			Width = 4f,
			InnerPadding = 1f,
			OuterPadding = 1f,
			InnerFade = 1f,
			OuterFade = 16f,
			RandomScale = 1f,
			MeshOffset = 0f,
			TerrainOffset = -0.125f,
			Topology = 524288,
			Splat = 128
		};
	}

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Rails.Clear();
			TerrainMeta.Path.Rails.AddRange(World.GetPaths("Rail"));
			return;
		}
		int min = Mathf.RoundToInt(40f);
		int max = Mathf.RoundToInt(53.3333321f);
		int min2 = Mathf.RoundToInt(40f);
		int max2 = Mathf.RoundToInt(120f);
		int transitionSteps = 8;
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		int length = array.GetLength(0);
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector3> list4 = new List<Vector3>();
		foreach (PathList rail2 in TerrainMeta.Path.Rails)
		{
			PathInterpolator path = rail2.Path;
			Vector3[] points = path.Points;
			Vector3[] tangents = path.Tangents;
			int num = path.MinIndex + 1 + transitionSteps;
			int num2 = path.MaxIndex - 1 - transitionSteps;
			for (int j = num; j <= num2; j++)
			{
				list2.Clear();
				list3.Clear();
				list4.Clear();
				int num3 = SeedRandom.Range(ref seed, min2, max2);
				int num4 = SeedRandom.Range(ref seed, min, max);
				int num5 = j;
				int num6 = j + num3;
				if (num6 >= num2)
				{
					continue;
				}
				Vector3 from = tangents[num5];
				Vector3 to = tangents[num6];
				if (Vector3.Angle(from, to) > 30f)
				{
					continue;
				}
				Vector3 worldPos = points[num5];
				PathFinder.Point pathFinderPoint = GetPathFinderPoint(worldPos, length);
				Vector3 worldPos2 = points[num6];
				PathFinder.Point pathFinderPoint2 = GetPathFinderPoint(worldPos2, length);
				j += num4;
				PathFinder.Node node = pathFinder.FindPath(pathFinderPoint, pathFinderPoint2, 250000);
				if (node == null)
				{
					continue;
				}
				PathFinder.Node node2 = null;
				PathFinder.Node node3 = null;
				PathFinder.Node node4 = node;
				while (node4 != null && node4.next != null)
				{
					if (node4 == node.next)
					{
						node2 = node4;
					}
					if (node4.next.next == null)
					{
						node3 = node4;
					}
					node4 = node4.next;
				}
				if (node2 == null || node3 == null)
				{
					continue;
				}
				node = node2;
				node3.next = null;
				for (int k = 0; k < transitionSteps; k++)
				{
					int num7 = num5 + (k + 1 - transitionSteps);
					int num8 = num6 + k;
					list2.Add(points[num7]);
					list3.Add(points[num8]);
				}
				list4.AddRange(list2);
				for (PathFinder.Node node5 = node2; node5 != null; node5 = node5.next)
				{
					float normX = ((float)node5.point.x + 0.5f) / (float)length;
					float normZ = ((float)node5.point.y + 0.5f) / (float)length;
					float x = TerrainMeta.DenormalizeX(normX);
					float z = TerrainMeta.DenormalizeZ(normZ);
					float y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
					list4.Add(new Vector3(x, y, z));
				}
				list4.AddRange(list3);
				int num9 = transitionSteps;
				int num10 = list4.Count - 1 - transitionSteps;
				Vector3 vector = Vector3.Normalize(list4[num9 + transitionSteps] - list4[num9]);
				Vector3 vector2 = Vector3.Normalize(list4[num10] - list4[num10 - transitionSteps]);
				Vector3 from2 = Vector3.Normalize(points[num5 + transitionSteps] - points[num5]);
				Vector3 from3 = Vector3.Normalize(points[num6] - points[num6 - transitionSteps]);
				float num11 = Vector3.SignedAngle(from2, vector, Vector3.up);
				float num12 = 0f - Vector3.SignedAngle(from3, vector2, Vector3.up);
				if (Mathf.Sign(num11) != Mathf.Sign(num12) || Mathf.Abs(num11) > 60f || Mathf.Abs(num12) > 60f)
				{
					continue;
				}
				Vector3 vector3 = rot90 * vector;
				Vector3 vector4 = rot90 * vector2;
				if (num11 < 0f)
				{
					vector3 = -vector3;
				}
				if (num12 < 0f)
				{
					vector4 = -vector4;
				}
				for (int l = 0; l < transitionSteps * 2; l++)
				{
					int index = l;
					int index2 = list4.Count - l - 1;
					float t = Mathf.InverseLerp(0f, transitionSteps, l);
					float num13 = Mathf.SmoothStep(0f, 2f, t) * 4f;
					list4[index] += vector3 * num13;
					list4[index2] += vector4 * num13;
				}
				bool flag = false;
				bool flag2 = false;
				foreach (Vector3 item in list4)
				{
					bool blocked = TerrainMeta.PlacementMap.GetBlocked(item);
					if (!flag2 && !flag && !blocked)
					{
						flag = true;
					}
					if (flag && !flag2 && blocked)
					{
						flag2 = true;
					}
					if (flag && flag2 && !blocked)
					{
						list4.Clear();
						break;
					}
				}
				if (list4.Count != 0)
				{
					if (list4.Count >= 2)
					{
						int number = TerrainMeta.Path.Rails.Count + list.Count;
						PathList pathList = CreateSegment(number, list4.ToArray());
						pathList.Start = false;
						pathList.End = false;
						pathList.ProcgenStartNode = node2;
						pathList.ProcgenEndNode = node3;
						list.Add(pathList);
					}
					j += num3;
				}
			}
		}
		foreach (PathList rail in list)
		{
			Func<int, float> filter = delegate(int i)
			{
				float a = Mathf.InverseLerp(0f, transitionSteps, i);
				float b = Mathf.InverseLerp(rail.Path.DefaultMaxIndex, rail.Path.DefaultMaxIndex - transitionSteps, i);
				return Mathf.SmoothStep(0f, 1f, Mathf.Min(a, b));
			};
			rail.Path.Smoothen(32, new Vector3(1f, 0f, 1f), filter);
			rail.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter);
			rail.Path.RecalculateLength();
			rail.Path.Resample(7.5f);
			rail.Path.RecalculateTangents();
			rail.AdjustPlacementMap(20f);
		}
		TerrainMeta.Path.Rails.AddRange(list);
	}

	public PathFinder.Point GetPathFinderPoint(Vector3 worldPos, int res)
	{
		float num = TerrainMeta.NormalizeX(worldPos.x);
		float num2 = TerrainMeta.NormalizeZ(worldPos.z);
		PathFinder.Point result = default(PathFinder.Point);
		result.x = Mathf.Clamp((int)(num * (float)res), 0, res - 1);
		result.y = Mathf.Clamp((int)(num2 * (float)res), 0, res - 1);
		return result;
	}
}
