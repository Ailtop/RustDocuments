using System.Collections.Generic;
using UnityEngine;

public class GenerateRailBranching : ProceduralComponent
{
	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 8f;

	public const float RandomScale = 1f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private const int Smoothen = 32;

	private const int MaxDepth = 250000;

	private PathList CreateSegment(int number, Vector3[] points)
	{
		return new PathList("Rail " + number, points)
		{
			Width = 4f,
			InnerPadding = 1f,
			OuterPadding = 1f,
			InnerFade = 1f,
			OuterFade = 8f,
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
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		int length = array.GetLength(0);
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector3> list4 = new List<Vector3>();
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			PathInterpolator path = rail.Path;
			float num = path.StartOffset + 5f;
			float num2 = path.Length - path.EndOffset - 5f - 900f;
			for (float num3 = num; num3 <= num2; num3 += 5f)
			{
				list2.Clear();
				list3.Clear();
				list4.Clear();
				float num4 = SeedRandom.Range(ref seed, 300f, 900f);
				float num5 = num3;
				Vector3 worldPos = (rail.Spline ? path.GetPointCubicHermite(num5) : path.GetPoint(num5));
				PathFinder.Point pathFinderPoint = GetPathFinderPoint(worldPos, length);
				float num6 = num3 + num4;
				Vector3 worldPos2 = (rail.Spline ? path.GetPointCubicHermite(num6) : path.GetPoint(num6));
				PathFinder.Point pathFinderPoint2 = GetPathFinderPoint(worldPos2, length);
				PathFinder.Node node = pathFinder.FindPath(pathFinderPoint, pathFinderPoint2, 250000);
				if (node == null)
				{
					continue;
				}
				PathFinder.Node node2 = null;
				PathFinder.Node procgenEndNode = null;
				for (PathFinder.Node node3 = node; node3 != null; node3 = node3.next)
				{
					if (node3 == node)
					{
						node2 = node3;
					}
					if (node3.next == null)
					{
						procgenEndNode = node3;
					}
				}
				for (int i = 0; i < 12; i++)
				{
					float distance = num5 + (float)(i + 1 - 12) * 5f;
					float distance2 = num6 + (float)i * 5f;
					list2.Add(rail.Spline ? path.GetPointCubicHermite(distance) : path.GetPoint(distance));
					list3.Add(rail.Spline ? path.GetPointCubicHermite(distance2) : path.GetPoint(distance2));
				}
				list4.AddRange(list2);
				for (PathFinder.Node node4 = node2; node4 != null; node4 = node4.next)
				{
					float normX = ((float)node4.point.x + 0.5f) / (float)length;
					float normZ = ((float)node4.point.y + 0.5f) / (float)length;
					float x = TerrainMeta.DenormalizeX(normX);
					float z = TerrainMeta.DenormalizeZ(normZ);
					float y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
					list4.Add(new Vector3(x, y, z));
				}
				list4.AddRange(list3);
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
				if (list4.Count != 0 && list4.Count >= 2)
				{
					int number = TerrainMeta.Path.Rails.Count + list.Count;
					PathList pathList = CreateSegment(number, list4.ToArray());
					pathList.ProcgenStartNode = node2;
					pathList.ProcgenEndNode = procgenEndNode;
					list.Add(pathList);
				}
				num3 += num4 + 300f;
			}
		}
		foreach (PathList item2 in list)
		{
			item2.Path.Smoothen(32);
			item2.Path.RecalculateTangents();
			item2.AdjustPlacementMap(20f);
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
