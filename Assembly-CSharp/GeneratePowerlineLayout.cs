using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratePowerlineLayout : ProceduralComponent
{
	private class PathNode
	{
		public MonumentInfo monument;

		public PathFinder.Node node;
	}

	private class PathSegment
	{
		public PathFinder.Node start;

		public PathFinder.Node end;
	}

	private const int MaxDepth = 100000;

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Powerlines.Clear();
			TerrainMeta.Path.Powerlines.AddRange(World.GetPaths("Powerline"));
			return;
		}
		List<PathList> list = new List<PathList>();
		List<MonumentInfo> monuments = TerrainMeta.Path.Monuments;
		int[,] array = TerrainPath.CreatePowerlineCostmap(ref seed);
		PathFinder pathFinder = new PathFinder(array);
		int length = array.GetLength(0);
		List<PathSegment> list2 = new List<PathSegment>();
		List<PathNode> list3 = new List<PathNode>();
		List<PathNode> list4 = new List<PathNode>();
		List<PathFinder.Point> list5 = new List<PathFinder.Point>();
		List<PathFinder.Point> list6 = new List<PathFinder.Point>();
		List<PathFinder.Point> list7 = new List<PathFinder.Point>();
		foreach (MonumentInfo item in monuments)
		{
			bool flag = list3.Count == 0;
			TerrainPathConnect[] componentsInChildren = item.GetComponentsInChildren<TerrainPathConnect>(true);
			foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
			{
				if (terrainPathConnect.Type == InfrastructureType.Power)
				{
					PathFinder.Point point = terrainPathConnect.GetPoint(length);
					PathFinder.Node node = pathFinder.FindClosestWalkable(point, 100000);
					if (node != null)
					{
						PathNode pathNode = new PathNode();
						pathNode.monument = item;
						pathNode.node = node;
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
		while (list4.Count != 0)
		{
			list6.Clear();
			list7.Clear();
			list6.AddRange(list3.Select((PathNode x) => x.node.point));
			list6.AddRange(list5);
			list7.AddRange(list4.Select((PathNode x) => x.node.point));
			PathFinder.Node node2 = pathFinder.FindPathUndirected(list6, list7, 100000);
			if (node2 == null)
			{
				PathNode copy2 = list4[0];
				list3.AddRange(list4.Where((PathNode x) => x.monument == copy2.monument));
				list4.RemoveAll((PathNode x) => x.monument == copy2.monument);
				continue;
			}
			PathSegment segment = new PathSegment();
			for (PathFinder.Node node3 = node2; node3 != null; node3 = node3.next)
			{
				if (node3 == node2)
				{
					segment.start = node3;
				}
				if (node3.next == null)
				{
					segment.end = node3;
				}
			}
			list2.Add(segment);
			PathNode copy = list4.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
			list3.AddRange(list4.Where((PathNode x) => x.monument == copy.monument));
			list4.RemoveAll((PathNode x) => x.monument == copy.monument);
			int num = 1;
			for (PathFinder.Node node4 = node2; node4 != null; node4 = node4.next)
			{
				if (num % 8 == 0)
				{
					list5.Add(node4.point);
				}
				num++;
			}
		}
		List<Vector3> list8 = new List<Vector3>();
		foreach (PathSegment item2 in list2)
		{
			for (PathFinder.Node node5 = item2.start; node5 != null; node5 = node5.next)
			{
				float num2 = ((float)node5.point.x + 0.5f) / (float)length;
				float num3 = ((float)node5.point.y + 0.5f) / (float)length;
				float height = TerrainMeta.HeightMap.GetHeight01(num2, num3);
				list8.Add(TerrainMeta.Denormalize(new Vector3(num2, height, num3)));
			}
			if (list8.Count != 0)
			{
				if (list8.Count >= 8)
				{
					int num4 = TerrainMeta.Path.Powerlines.Count + list.Count;
					PathList pathList = new PathList("Powerline " + num4, list8.ToArray());
					pathList.Start = true;
					pathList.End = true;
					pathList.ProcgenStartNode = item2.start;
					pathList.ProcgenEndNode = item2.end;
					list.Add(pathList);
				}
				list8.Clear();
			}
		}
		foreach (PathList item3 in list)
		{
			item3.Path.RecalculateTangents();
		}
		TerrainMeta.Path.Powerlines.AddRange(list);
	}
}
