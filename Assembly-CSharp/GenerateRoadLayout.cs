using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRoadLayout : ProceduralComponent
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

	public InfrastructureType RoadType;

	public const float RoadWidth = 10f;

	public const float TrailWidth = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 8f;

	public const float RandomScale = 0.75f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private const int MaxDepth = 100000;

	private PathList CreateSegment(int number, Vector3[] points)
	{
		PathList pathList = new PathList("Road " + number, points);
		if (RoadType == InfrastructureType.Road)
		{
			pathList.Spline = true;
			pathList.Width = 10f;
			pathList.InnerPadding = 1f;
			pathList.OuterPadding = 1f;
			pathList.InnerFade = 1f;
			pathList.OuterFade = 8f;
			pathList.RandomScale = 0.75f;
			pathList.MeshOffset = 0f;
			pathList.TerrainOffset = -0.125f;
			pathList.Topology = 2048;
			pathList.Splat = 128;
			pathList.Hierarchy = 1;
		}
		else
		{
			float num = 0.4f;
			pathList.Spline = true;
			pathList.Width = 4f;
			pathList.InnerPadding = 1f * num;
			pathList.OuterPadding = 1f;
			pathList.InnerFade = 1f;
			pathList.OuterFade = 8f;
			pathList.RandomScale = 0.75f;
			pathList.MeshOffset = 0f;
			pathList.TerrainOffset = -0.125f;
			pathList.Topology = 2048;
			pathList.Splat = 1;
			pathList.Hierarchy = 2;
		}
		return pathList;
	}

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Roads.Clear();
			TerrainMeta.Path.Roads.AddRange(World.GetPaths("Road"));
			return;
		}
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRoadCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		int length = array.GetLength(0);
		List<PathSegment> list2 = new List<PathSegment>();
		List<PathNode> list3 = new List<PathNode>();
		List<PathNode> list4 = new List<PathNode>();
		List<PathNode> list5 = new List<PathNode>();
		List<PathFinder.Point> list6 = new List<PathFinder.Point>();
		List<PathFinder.Point> list7 = new List<PathFinder.Point>();
		List<PathFinder.Point> list8 = new List<PathFinder.Point>();
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			if (road.ProcgenStartNode == null || road.ProcgenEndNode == null)
			{
				continue;
			}
			int num = 1;
			for (PathFinder.Node node4 = road.ProcgenStartNode; node4 != null; node4 = node4.next)
			{
				if (num % 8 == 0)
				{
					list6.Add(node4.point);
				}
				num++;
			}
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.Type == MonumentType.Roadside)
			{
				continue;
			}
			TerrainPathConnect[] componentsInChildren = monument.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true);
			foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
			{
				if (terrainPathConnect.Type == RoadType)
				{
					PathFinder.Point pathFinderPoint = terrainPathConnect.GetPathFinderPoint(length);
					PathFinder.Node node5 = pathFinder.FindClosestWalkable(pathFinderPoint, 100000);
					if (node5 != null)
					{
						PathNode pathNode = new PathNode();
						pathNode.monument = monument;
						pathNode.target = terrainPathConnect;
						pathNode.node = node5;
						list4.Add(pathNode);
					}
				}
			}
		}
		while (list4.Count != 0 || list5.Count != 0)
		{
			if (list4.Count == 0)
			{
				PathNode node3 = list5[0];
				list4.AddRange(list5.Where((PathNode x) => x.monument == node3.monument));
				list5.RemoveAll((PathNode x) => x.monument == node3.monument);
				pathFinder.PushPoint = node3.monument.GetPathFinderPoint(length);
				pathFinder.PushRadius = node3.monument.GetPathFinderRadius(length);
				pathFinder.PushDistance = 40;
				pathFinder.PushMultiplier = 1;
			}
			list8.Clear();
			list8.AddRange(list4.Select((PathNode x) => x.node.point));
			list7.Clear();
			list7.AddRange(list3.Select((PathNode x) => x.node.point));
			list7.AddRange(list5.Select((PathNode x) => x.node.point));
			list7.AddRange(list6);
			PathFinder.Node node6 = pathFinder.FindPathUndirected(list7, list8, 100000);
			if (node6 == null)
			{
				PathNode node2 = list4[0];
				list5.AddRange(list4.Where((PathNode x) => x.monument == node2.monument));
				list4.RemoveAll((PathNode x) => x.monument == node2.monument);
				list5.Remove(node2);
				list3.Add(node2);
				continue;
			}
			PathSegment segment = new PathSegment();
			for (PathFinder.Node node7 = node6; node7 != null; node7 = node7.next)
			{
				if (node7 == node6)
				{
					segment.start = node7;
				}
				if (node7.next == null)
				{
					segment.end = node7;
				}
			}
			list2.Add(segment);
			PathNode node = list4.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			list5.AddRange(list4.Where((PathNode x) => x.monument == node.monument));
			list4.RemoveAll((PathNode x) => x.monument == node.monument);
			list5.Remove(node);
			list3.Add(node);
			PathNode pathNode2 = list5.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			if (pathNode2 != null)
			{
				list5.Remove(pathNode2);
				list3.Add(pathNode2);
			}
			int num2 = 1;
			for (PathFinder.Node node8 = node6; node8 != null; node8 = node8.next)
			{
				if (num2 % 8 == 0)
				{
					list6.Add(node8.point);
				}
				num2++;
			}
		}
		foreach (PathNode target in list3)
		{
			PathSegment pathSegment = list2.Find((PathSegment x) => x.start.point == target.node.point || x.end.point == target.node.point);
			if (pathSegment != null)
			{
				if (pathSegment.start.point == target.node.point)
				{
					PathFinder.Node node9 = target.node;
					PathFinder.Node start = pathFinder.Reverse(target.node);
					node9.next = pathSegment.start;
					pathSegment.start = start;
					pathSegment.origin = target.target;
				}
				else if (pathSegment.end.point == target.node.point)
				{
					pathSegment.end.next = target.node;
					pathSegment.end = pathFinder.FindEnd(target.node);
					pathSegment.target = target.target;
				}
			}
		}
		List<Vector3> list9 = new List<Vector3>();
		foreach (PathSegment item in list2)
		{
			bool start2 = false;
			bool end = false;
			for (PathFinder.Node node10 = item.start; node10 != null; node10 = node10.next)
			{
				float normX = ((float)node10.point.x + 0.5f) / (float)length;
				float normZ = ((float)node10.point.y + 0.5f) / (float)length;
				if (item.start == node10 && item.origin != null)
				{
					start2 = true;
					normX = TerrainMeta.NormalizeX(item.origin.transform.position.x);
					normZ = TerrainMeta.NormalizeZ(item.origin.transform.position.z);
				}
				else if (item.end == node10 && item.target != null)
				{
					end = true;
					normX = TerrainMeta.NormalizeX(item.target.transform.position.x);
					normZ = TerrainMeta.NormalizeZ(item.target.transform.position.z);
				}
				float x2 = TerrainMeta.DenormalizeX(normX);
				float z = TerrainMeta.DenormalizeZ(normZ);
				float y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
				list9.Add(new Vector3(x2, y, z));
			}
			if (list9.Count != 0)
			{
				if (list9.Count >= 2)
				{
					int number = TerrainMeta.Path.Roads.Count + list.Count;
					PathList pathList = CreateSegment(number, list9.ToArray());
					pathList.Start = start2;
					pathList.End = end;
					pathList.ProcgenStartNode = item.start;
					pathList.ProcgenEndNode = item.end;
					list.Add(pathList);
				}
				list9.Clear();
			}
		}
		foreach (PathList item2 in list)
		{
			item2.Path.Smoothen(4, new Vector3(1f, 0f, 1f));
			item2.Path.Smoothen(16, new Vector3(0f, 1f, 0f));
			item2.Path.Resample(7.5f);
			item2.Path.RecalculateTangents();
			item2.AdjustPlacementMap(20f);
		}
		TerrainMeta.Path.Roads.AddRange(list);
	}
}
