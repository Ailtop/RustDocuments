using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRailLayout : ProceduralComponent
{
	private class PathNode
	{
		public MonumentInfo monument;

		public TerrainPathConnect target;

		public PathFinder.Node node;
	}

	private class PathSegment
	{
		public PathFinder.Node start;

		public PathFinder.Node end;

		public TerrainPathConnect origin;

		public TerrainPathConnect target;
	}

	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 32f;

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
			OuterFade = 32f,
			RandomScale = 1f,
			MeshOffset = 0f,
			TerrainOffset = -0.125f,
			Topology = 524288,
			Splat = 128,
			Hierarchy = 1
		};
	}

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Rails.Clear();
			TerrainMeta.Path.Rails.AddRange(World.GetPaths("Rail"));
			{
				foreach (PathList rail2 in TerrainMeta.Path.Rails)
				{
					Func<int, float> filter = delegate(int i)
					{
						float a4 = Mathf.InverseLerp(0f, 8f, i);
						float b3 = Mathf.InverseLerp(rail2.Path.DefaultMaxIndex, rail2.Path.DefaultMaxIndex - 8, i);
						return Mathf.SmoothStep(0f, 1f, Mathf.Min(a4, b3));
					};
					Vector3[] points = rail2.Path.Points;
					for (int j = 1; j < points.Length - 1; j++)
					{
						Vector3 vector = points[j];
						vector.y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(vector), 1f);
						points[j] = vector;
					}
					rail2.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter);
					rail2.Path.RecalculateTangents();
				}
				return;
			}
		}
		if (!World.Config.AboveGroundRails)
		{
			return;
		}
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		PathFinder pathFinder2 = new PathFinder(array);
		int length = array.GetLength(0);
		new List<PathSegment>();
		List<PathFinder.Point> list2 = new List<PathFinder.Point>();
		List<PathFinder.Point> list3 = new List<PathFinder.Point>();
		List<PathFinder.Point> list4 = new List<PathFinder.Point>();
		List<Vector3> list5 = new List<Vector3>();
		foreach (PathList rail3 in TerrainMeta.Path.Rails)
		{
			for (PathFinder.Node node = rail3.ProcgenStartNode; node != null; node = node.next)
			{
				list2.Add(node.point);
			}
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			pathFinder.PushPoint = monument.GetPathFinderPoint(length);
			pathFinder.PushRadius = (pathFinder.PushDistance = monument.GetPathFinderRadius(length));
			pathFinder.PushMultiplier = 50000;
			int a = int.MaxValue;
			TerrainPathConnect[] array2 = (from target in monument.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true)
				where target.Type == InfrastructureType.Rail
				orderby DistanceToRail(target.transform.position)
				select target).ToArray();
			TerrainPathConnect[] array3 = array2;
			foreach (TerrainPathConnect terrainPathConnect in array3)
			{
				pathFinder.PushPointsAdditional.Clear();
				pathFinder.BlockedPointsAdditional.Clear();
				Vector3 position = terrainPathConnect.transform.position;
				TerrainPathConnect[] array4 = array2;
				foreach (TerrainPathConnect terrainPathConnect2 in array4)
				{
					if (!(terrainPathConnect == terrainPathConnect2))
					{
						Vector3 position2 = terrainPathConnect2.transform.position;
						PathFinder.Point point = PathFinder.GetPoint(terrainPathConnect2.transform.position, length);
						pathFinder.PushPointsAdditional.Add(point);
						position += position2;
					}
				}
				position /= (float)array2.Length;
				Vector3 vector2 = ((array2.Length > 1) ? (terrainPathConnect.transform.position - position).normalized : terrainPathConnect.transform.forward);
				foreach (PathList item in list)
				{
					pathFinder.PushPointsAdditional.Add(PathFinder.GetPoint(item.Path.GetEndPoint(), length));
					PathFinder.Point point2 = PathFinder.GetPoint(item.Path.GetStartPoint(), length);
					Vector3[] points2 = item.Path.Points;
					for (int l = 0; l < points2.Length; l++)
					{
						PathFinder.Point point3 = PathFinder.GetPoint(points2[l], length);
						pathFinder.BlockedPointsAdditional.Add(point3);
						pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point3.x, point2.y));
						pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point2.x, point3.y));
						point2 = point3;
					}
					if (item.ProcgenStartNode != null)
					{
						PathFinder.Point point4 = item.ProcgenStartNode.point;
						for (PathFinder.Node node2 = item.ProcgenStartNode; node2 != null; node2 = node2.next)
						{
							PathFinder.Point point5 = node2.point;
							pathFinder.BlockedPointsAdditional.Add(point5);
							pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point5.x, point4.y));
							pathFinder.BlockedPointsAdditional.Add(new PathFinder.Point(point4.x, point5.y));
							point4 = point5;
						}
					}
				}
				list5.Clear();
				Vector3 position3 = terrainPathConnect.transform.position;
				Vector3 vector3 = terrainPathConnect.transform.forward * 7.5f;
				PathFinder.Point point6 = PathFinder.GetPoint(position3, length);
				for (int m = 0; (m < 8 && pathFinder.Heuristic(point6, list2) > 1) || (m < 16 && !pathFinder.IsWalkable(point6)); m++)
				{
					list5.Add(position3);
					position3 += vector3;
					point6 = PathFinder.GetPoint(position3, length);
				}
				if (!pathFinder.IsWalkable(point6))
				{
					continue;
				}
				list3.Clear();
				list3.Add(point6);
				list4.Clear();
				list4.AddRange(list2);
				PathFinder.Node node3 = pathFinder.FindPathDirected(list3, list4, 250000);
				bool flag = false;
				if (node3 == null && list.Count > 0 && a != int.MaxValue)
				{
					PathList pathList = list[list.Count - 1];
					list4.Clear();
					for (int n = 0; n < pathList.Path.Points.Length; n++)
					{
						list4.Add(PathFinder.GetPoint(pathList.Path.Points[n], length));
					}
					node3 = pathFinder2.FindPathDirected(list3, list4, 250000);
					flag = true;
				}
				if (node3 == null)
				{
					continue;
				}
				PathFinder.Node node4 = null;
				PathFinder.Node node5 = null;
				PathFinder.Node node6 = node3;
				while (node6 != null && node6.next != null)
				{
					if (node6 == node3.next)
					{
						node4 = node6;
					}
					if (node6.next.next == null)
					{
						node5 = node6;
						node5.next = null;
					}
					node6 = node6.next;
				}
				for (PathFinder.Node node7 = node4; node7 != null; node7 = node7.next)
				{
					float normX = ((float)node7.point.x + 0.5f) / (float)length;
					float normZ = ((float)node7.point.y + 0.5f) / (float)length;
					float x = TerrainMeta.DenormalizeX(normX);
					float z = TerrainMeta.DenormalizeZ(normZ);
					float y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
					list5.Add(new Vector3(x, y, z));
				}
				Vector3 a2 = list5[list5.Count - 1];
				Vector3 to = vector2;
				PathList pathList2 = null;
				float num = float.MaxValue;
				int num2 = -1;
				if (!flag)
				{
					foreach (PathList rail4 in TerrainMeta.Path.Rails)
					{
						Vector3[] points3 = rail4.Path.Points;
						for (int num3 = 0; num3 < points3.Length; num3++)
						{
							float num4 = Vector3.Distance(a2, points3[num3]);
							if (num4 < num)
							{
								num = num4;
								pathList2 = rail4;
								num2 = num3;
							}
						}
					}
				}
				else
				{
					foreach (PathList item2 in list)
					{
						Vector3[] points4 = item2.Path.Points;
						for (int num5 = 0; num5 < points4.Length; num5++)
						{
							float num6 = Vector3.Distance(a2, points4[num5]);
							if (num6 < num)
							{
								num = num6;
								pathList2 = item2;
								num2 = num5;
							}
						}
					}
				}
				int b = 1;
				if (!flag)
				{
					Vector3 tangentByIndex = pathList2.Path.GetTangentByIndex(num2);
					b = ((Vector3.Angle(tangentByIndex, to) < Vector3.Angle(-tangentByIndex, to)) ? 1 : (-1));
					if (a != int.MaxValue)
					{
						GenericsUtil.Swap(ref a, ref b);
						b = -b;
					}
					else
					{
						a = b;
					}
				}
				Vector3 vector4 = Vector3.Normalize(pathList2.Path.GetPointByIndex(num2 + b * 8 * 2) - pathList2.Path.GetPointByIndex(num2));
				Vector3 vector5 = rot90 * vector4;
				if (!flag)
				{
					Vector3 to2 = Vector3.Normalize(list5[list5.Count - 1] - list5[Mathf.Max(0, list5.Count - 1 - 16)]);
					if (0f - Vector3.SignedAngle(vector4, to2, Vector3.up) < 0f)
					{
						vector5 = -vector5;
					}
				}
				for (int num7 = 0; num7 < 8; num7++)
				{
					float t = Mathf.InverseLerp(7f, 0f, num7);
					float num8 = Mathf.SmoothStep(0f, 2f, t) * 4f;
					list5.Add(pathList2.Path.GetPointByIndex(num2 + b * num7) + vector5 * num8);
				}
				if (list5.Count >= 2)
				{
					int number = TerrainMeta.Path.Rails.Count + list.Count;
					PathList rail = CreateSegment(number, list5.ToArray());
					rail.Start = true;
					rail.End = false;
					rail.ProcgenStartNode = node4;
					rail.ProcgenEndNode = node5;
					Func<int, float> filter2 = delegate(int i)
					{
						float a3 = Mathf.InverseLerp(0f, 8f, i);
						float b2 = Mathf.InverseLerp(rail.Path.DefaultMaxIndex, rail.Path.DefaultMaxIndex - 8, i);
						return Mathf.SmoothStep(0f, 1f, Mathf.Min(a3, b2));
					};
					rail.Path.Smoothen(32, new Vector3(1f, 0f, 1f), filter2);
					rail.Path.Smoothen(64, new Vector3(0f, 1f, 0f), filter2);
					rail.Path.Resample(7.5f);
					rail.Path.RecalculateTangents();
					list.Add(rail);
				}
			}
		}
		foreach (PathList item3 in list)
		{
			item3.AdjustPlacementMap(20f);
		}
		TerrainMeta.Path.Rails.AddRange(list);
		static float DistanceToRail(Vector3 vec)
		{
			float num9 = float.MaxValue;
			foreach (PathList rail5 in TerrainMeta.Path.Rails)
			{
				Vector3[] points5 = rail5.Path.Points;
				foreach (Vector3 vector6 in points5)
				{
					num9 = Mathf.Min(num9, (vec - vector6).Magnitude2D());
				}
			}
			return num9;
		}
	}
}
