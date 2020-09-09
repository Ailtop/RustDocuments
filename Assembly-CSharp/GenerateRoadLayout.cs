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

	public const float Width = 10f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 8f;

	public const float RandomScale = 0.75f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private const int Smoothen = 4;

	private const int MaxDepth = 100000;

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
		List<PathFinder.Point> list5 = new List<PathFinder.Point>();
		List<PathFinder.Point> list6 = new List<PathFinder.Point>();
		List<PathFinder.Point> list7 = new List<PathFinder.Point>();
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			if (road.ProcgenStartNode != null && road.ProcgenEndNode != null && road.IsExtraWide)
			{
				int num = 1;
				for (PathFinder.Node node = road.ProcgenStartNode; node != null; node = node.next)
				{
					if (num % 8 == 0)
					{
						list5.Add(node.point);
					}
					num++;
				}
			}
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.Type != MonumentType.Roadside)
			{
				bool flag = list3.Count == 0;
				TerrainPathConnect[] componentsInChildren = monument.GetComponentsInChildren<TerrainPathConnect>(true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type == InfrastructureType.Road)
					{
						PathFinder.Point point = terrainPathConnect.GetPoint(length);
						PathFinder.Node node2 = pathFinder.FindClosestWalkable(point, 100000);
						if (node2 != null)
						{
							PathNode pathNode = new PathNode();
							pathNode.monument = monument;
							pathNode.target = terrainPathConnect;
							pathNode.node = node2;
							if (flag)
							{
								list3.Add(pathNode);
							}
							else
							{
								list4.Add(pathNode);
							}
						}
					}
				}
			}
		}
		while (list4.Count != 0)
		{
			list6.Clear();
			list7.Clear();
			list6.AddRange(list3.Select((PathNode x) => x.node.point));
			list6.AddRange(list5);
			list7.AddRange(list4.Select((PathNode x) => x.node.point));
			PathFinder.Node node3 = pathFinder.FindPathUndirected(list6, list7, 100000);
			if (node3 == null)
			{
				PathNode copy2 = list4[0];
				list3.AddRange(list4.Where((PathNode x) => x.monument == copy2.monument));
				list4.RemoveAll((PathNode x) => x.monument == copy2.monument);
				continue;
			}
			PathSegment segment = new PathSegment();
			for (PathFinder.Node node4 = node3; node4 != null; node4 = node4.next)
			{
				if (node4 == node3)
				{
					segment.start = node4;
				}
				if (node4.next == null)
				{
					segment.end = node4;
				}
			}
			list2.Add(segment);
			PathNode copy = list4.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			list3.AddRange(list4.Where((PathNode x) => x.monument == copy.monument));
			list4.RemoveAll((PathNode x) => x.monument == copy.monument);
			int num2 = 1;
			for (PathFinder.Node node5 = node3; node5 != null; node5 = node5.next)
			{
				if (num2 % 8 == 0)
				{
					list5.Add(node5.point);
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
					PathFinder.Node node6 = target.node;
					PathFinder.Node start = pathFinder.Reverse(target.node);
					node6.next = pathSegment.start;
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
		List<Vector3> list8 = new List<Vector3>();
		foreach (PathSegment item in list2)
		{
			bool start2 = false;
			bool end = false;
			for (PathFinder.Node node7 = item.start; node7 != null; node7 = node7.next)
			{
				float normX = ((float)node7.point.x + 0.5f) / (float)length;
				float normZ = ((float)node7.point.y + 0.5f) / (float)length;
				if (item.start == node7 && item.origin != null)
				{
					start2 = true;
					normX = TerrainMeta.NormalizeX(item.origin.transform.position.x);
					normZ = TerrainMeta.NormalizeZ(item.origin.transform.position.z);
				}
				else if (item.end == node7 && item.target != null)
				{
					end = true;
					normX = TerrainMeta.NormalizeX(item.target.transform.position.x);
					normZ = TerrainMeta.NormalizeZ(item.target.transform.position.z);
				}
				float x2 = TerrainMeta.DenormalizeX(normX);
				float z = TerrainMeta.DenormalizeZ(normZ);
				float y = Mathf.Max(TerrainMeta.HeightMap.GetHeight(normX, normZ), 1f);
				list8.Add(new Vector3(x2, y, z));
			}
			if (list8.Count != 0)
			{
				if (list8.Count >= 2)
				{
					int num3 = TerrainMeta.Path.Roads.Count + list.Count;
					PathList pathList = new PathList("Road " + num3, list8.ToArray());
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
					pathList.Start = start2;
					pathList.End = end;
					pathList.ProcgenStartNode = item.start;
					pathList.ProcgenEndNode = item.end;
					list.Add(pathList);
				}
				list8.Clear();
			}
		}
		foreach (PathList item2 in list)
		{
			item2.Path.Smoothen(4);
			item2.Path.RecalculateTangents();
			item2.AdjustPlacementMap(20f);
		}
		TerrainMeta.Path.Roads.AddRange(list);
	}
}
