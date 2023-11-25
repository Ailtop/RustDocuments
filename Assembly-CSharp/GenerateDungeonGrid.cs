using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateDungeonGrid : ProceduralComponent
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

	private class PathLink
	{
		public PathLinkSide downwards;

		public PathLinkSide upwards;
	}

	private class PathLinkSide
	{
		public PathLinkSegment origin;

		public List<PathLinkSegment> segments;

		public PathLinkSegment prevSegment
		{
			get
			{
				if (segments.Count <= 0)
				{
					return origin;
				}
				return segments[segments.Count - 1];
			}
		}
	}

	private class PathLinkSegment
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public Prefab<DungeonGridLink> prefab;

		public DungeonGridLink link;

		public Transform downSocket => link.DownSocket;

		public Transform upSocket => link.UpSocket;

		public DungeonGridLinkType downType => link.DownType;

		public DungeonGridLinkType upType => link.UpType;
	}

	private struct PrefabReplacement
	{
		public Vector2i gridPosition;

		public Vector3 worldPosition;

		public int distance;

		public Prefab<DungeonGridCell> prefab;
	}

	public string TunnelFolder = string.Empty;

	public string StationFolder = string.Empty;

	public string UpwardsFolder = string.Empty;

	public string TransitionFolder = string.Empty;

	public string LinkFolder = string.Empty;

	public InfrastructureType ConnectionType = InfrastructureType.Tunnel;

	public int CellSize = 216;

	public float LinkHeight = 1.5f;

	public float LinkRadius = 3f;

	public float LinkTransition = 9f;

	private const int MaxDepth = 100000;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		if (World.Cached)
		{
			TerrainMeta.Path.DungeonGridRoot = HierarchyUtil.GetRoot("Dungeon");
		}
		else if (World.Networked)
		{
			World.Spawn("Dungeon");
			TerrainMeta.Path.DungeonGridRoot = HierarchyUtil.GetRoot("Dungeon");
		}
		else
		{
			if (ConnectionType == InfrastructureType.Tunnel && !World.Config.BelowGroundRails)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + TunnelFolder, null, null, useProbabilities: true, useWorldConfig: false);
			if (array == null || array.Length == 0)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array2 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + StationFolder, null, null, useProbabilities: true, useWorldConfig: false);
			if (array2 == null || array2.Length == 0)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array3 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + UpwardsFolder, null, null, useProbabilities: true, useWorldConfig: false);
			if (array3 == null)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array4 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + TransitionFolder, null, null, useProbabilities: true, useWorldConfig: false);
			if (array4 == null)
			{
				return;
			}
			Prefab<DungeonGridLink>[] array5 = Prefab.Load<DungeonGridLink>("assets/bundled/prefabs/autospawn/" + LinkFolder, null, null, useProbabilities: true, useWorldConfig: false);
			if (array5 == null)
			{
				return;
			}
			array5 = array5.OrderByDescending((Prefab<DungeonGridLink> x) => x.Component.Priority).ToArray();
			List<DungeonGridInfo> list = (TerrainMeta.Path ? TerrainMeta.Path.DungeonGridEntrances : null);
			WorldSpaceGrid<Prefab<DungeonGridCell>> worldSpaceGrid = new WorldSpaceGrid<Prefab<DungeonGridCell>>(TerrainMeta.Size.x * 2f, CellSize);
			int[,] array6 = new int[worldSpaceGrid.CellCount, worldSpaceGrid.CellCount];
			DungeonGridConnectionHash[,] hashmap = new DungeonGridConnectionHash[worldSpaceGrid.CellCount, worldSpaceGrid.CellCount];
			PathFinder pathFinder = new PathFinder(array6, diagonals: false);
			int cellCount = worldSpaceGrid.CellCount;
			int num = 0;
			int num2 = worldSpaceGrid.CellCount - 1;
			for (int i = 0; i < cellCount; i++)
			{
				for (int j = 0; j < cellCount; j++)
				{
					array6[j, i] = 1;
				}
			}
			List<PathSegment> list2 = new List<PathSegment>();
			List<PathLink> list3 = new List<PathLink>();
			List<PathNode> list4 = new List<PathNode>();
			List<PathNode> unconnectedNodeList = new List<PathNode>();
			List<PathNode> secondaryNodeList = new List<PathNode>();
			List<PathFinder.Point> list5 = new List<PathFinder.Point>();
			List<PathFinder.Point> list6 = new List<PathFinder.Point>();
			List<PathFinder.Point> list7 = new List<PathFinder.Point>();
			foreach (DungeonGridInfo item3 in list)
			{
				DungeonGridInfo entrance = item3;
				TerrainPathConnect[] componentsInChildren = entrance.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type != ConnectionType)
					{
						continue;
					}
					Vector2i cellPos = worldSpaceGrid.WorldToGridCoords(terrainPathConnect.transform.position);
					if (array6[cellPos.x, cellPos.y] == int.MaxValue)
					{
						continue;
					}
					PathFinder.Node stationNode = pathFinder.FindClosestWalkable(new PathFinder.Point(cellPos.x, cellPos.y), 1);
					if (stationNode == null)
					{
						continue;
					}
					Prefab<DungeonGridCell> prefab = ((cellPos.x > num) ? worldSpaceGrid[cellPos.x - 1, cellPos.y] : null);
					Prefab<DungeonGridCell> prefab2 = ((cellPos.x < num2) ? worldSpaceGrid[cellPos.x + 1, cellPos.y] : null);
					Prefab<DungeonGridCell> prefab3 = ((cellPos.y > num) ? worldSpaceGrid[cellPos.x, cellPos.y - 1] : null);
					Prefab<DungeonGridCell> prefab4 = ((cellPos.y < num2) ? worldSpaceGrid[cellPos.x, cellPos.y + 1] : null);
					Prefab<DungeonGridCell> prefab5 = null;
					float num3 = float.MaxValue;
					ArrayEx.Shuffle(array2, ref seed);
					Prefab<DungeonGridCell>[] array7 = array2;
					foreach (Prefab<DungeonGridCell> prefab6 in array7)
					{
						if ((prefab != null && prefab6.Component.West != prefab.Component.East) || (prefab2 != null && prefab6.Component.East != prefab2.Component.West) || (prefab3 != null && prefab6.Component.South != prefab3.Component.North) || (prefab4 != null && prefab6.Component.North != prefab4.Component.South))
						{
							continue;
						}
						DungeonVolume componentInChildren = prefab6.Object.GetComponentInChildren<DungeonVolume>();
						DungeonVolume componentInChildren2 = entrance.GetComponentInChildren<DungeonVolume>();
						OBB bounds = componentInChildren.GetBounds(worldSpaceGrid.GridToWorldCoords(cellPos), Quaternion.identity);
						OBB bounds2 = componentInChildren2.GetBounds(entrance.transform.position, Quaternion.identity);
						if (!bounds.Intersects2D(bounds2))
						{
							DungeonGridLink componentInChildren3 = prefab6.Object.GetComponentInChildren<DungeonGridLink>();
							Vector3 vector = worldSpaceGrid.GridToWorldCoords(new Vector2i(cellPos.x, cellPos.y)) + componentInChildren3.UpSocket.localPosition;
							float num4 = (terrainPathConnect.transform.position - vector).Magnitude2D();
							if (!(num3 < num4))
							{
								prefab5 = prefab6;
								num3 = num4;
							}
						}
					}
					bool isStartPoint;
					if (prefab5 != null)
					{
						worldSpaceGrid[cellPos.x, cellPos.y] = prefab5;
						array6[cellPos.x, cellPos.y] = int.MaxValue;
						isStartPoint = secondaryNodeList.Count == 0;
						secondaryNodeList.RemoveAll((PathNode x) => x.node.point == stationNode.point);
						unconnectedNodeList.RemoveAll((PathNode x) => x.node.point == stationNode.point);
						if (prefab5.Component.West != 0)
						{
							AddNode(cellPos.x - 1, cellPos.y);
						}
						if (prefab5.Component.East != 0)
						{
							AddNode(cellPos.x + 1, cellPos.y);
						}
						if (prefab5.Component.South != 0)
						{
							AddNode(cellPos.x, cellPos.y - 1);
						}
						if (prefab5.Component.North != 0)
						{
							AddNode(cellPos.x, cellPos.y + 1);
						}
						PathLink pathLink = new PathLink();
						DungeonGridLink componentInChildren4 = entrance.gameObject.GetComponentInChildren<DungeonGridLink>();
						Vector3 position = entrance.transform.position;
						Vector3 eulerAngles = entrance.transform.rotation.eulerAngles;
						DungeonGridLink componentInChildren5 = prefab5.Object.GetComponentInChildren<DungeonGridLink>();
						Vector3 position2 = worldSpaceGrid.GridToWorldCoords(new Vector2i(cellPos.x, cellPos.y));
						Vector3 zero = Vector3.zero;
						pathLink.downwards = new PathLinkSide();
						pathLink.downwards.origin = new PathLinkSegment();
						pathLink.downwards.origin.position = position;
						pathLink.downwards.origin.rotation = Quaternion.Euler(eulerAngles);
						pathLink.downwards.origin.scale = Vector3.one;
						pathLink.downwards.origin.link = componentInChildren4;
						pathLink.downwards.segments = new List<PathLinkSegment>();
						pathLink.upwards = new PathLinkSide();
						pathLink.upwards.origin = new PathLinkSegment();
						pathLink.upwards.origin.position = position2;
						pathLink.upwards.origin.rotation = Quaternion.Euler(zero);
						pathLink.upwards.origin.scale = Vector3.one;
						pathLink.upwards.origin.link = componentInChildren5;
						pathLink.upwards.segments = new List<PathLinkSegment>();
						list3.Add(pathLink);
					}
					void AddNode(int x, int y)
					{
						PathFinder.Node node8 = pathFinder.FindClosestWalkable(new PathFinder.Point(x, y), 1);
						if (node8 != null)
						{
							PathNode item2 = new PathNode
							{
								monument = (TerrainMeta.Path ? TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, entrance.transform.position) : entrance.transform.GetComponentInParent<MonumentInfo>()),
								node = node8
							};
							if (isStartPoint)
							{
								secondaryNodeList.Add(item2);
							}
							else
							{
								unconnectedNodeList.Add(item2);
							}
							DungeonGridConnectionHash dungeonGridConnectionHash4 = hashmap[node8.point.x, node8.point.y];
							DungeonGridConnectionHash dungeonGridConnectionHash5 = hashmap[stationNode.point.x, stationNode.point.y];
							if (node8.point.x > stationNode.point.x)
							{
								dungeonGridConnectionHash4.West = true;
								dungeonGridConnectionHash5.East = true;
							}
							if (node8.point.x < stationNode.point.x)
							{
								dungeonGridConnectionHash4.East = true;
								dungeonGridConnectionHash5.West = true;
							}
							if (node8.point.y > stationNode.point.y)
							{
								dungeonGridConnectionHash4.South = true;
								dungeonGridConnectionHash5.North = true;
							}
							if (node8.point.y < stationNode.point.y)
							{
								dungeonGridConnectionHash4.North = true;
								dungeonGridConnectionHash5.South = true;
							}
							hashmap[node8.point.x, node8.point.y] = dungeonGridConnectionHash4;
							hashmap[stationNode.point.x, stationNode.point.y] = dungeonGridConnectionHash5;
						}
					}
				}
			}
			while (unconnectedNodeList.Count != 0 || secondaryNodeList.Count != 0)
			{
				if (unconnectedNodeList.Count == 0)
				{
					PathNode node3 = secondaryNodeList[0];
					unconnectedNodeList.AddRange(secondaryNodeList.Where((PathNode x) => x.monument == node3.monument));
					secondaryNodeList.RemoveAll((PathNode x) => x.monument == node3.monument);
					Vector2i vector2i = worldSpaceGrid.WorldToGridCoords(node3.monument.transform.position);
					pathFinder.PushPoint = new PathFinder.Point(vector2i.x, vector2i.y);
					pathFinder.PushRadius = (pathFinder.PushDistance = 2);
					pathFinder.PushMultiplier = 16;
				}
				list7.Clear();
				list7.AddRange(unconnectedNodeList.Select((PathNode x) => x.node.point));
				list6.Clear();
				list6.AddRange(list4.Select((PathNode x) => x.node.point));
				list6.AddRange(secondaryNodeList.Select((PathNode x) => x.node.point));
				list6.AddRange(list5);
				PathFinder.Node node4 = pathFinder.FindPathUndirected(list6, list7, 100000);
				if (node4 == null)
				{
					PathNode node2 = unconnectedNodeList[0];
					secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => x.monument == node2.monument));
					unconnectedNodeList.RemoveAll((PathNode x) => x.monument == node2.monument);
					secondaryNodeList.Remove(node2);
					list4.Add(node2);
					continue;
				}
				PathSegment segment = new PathSegment();
				for (PathFinder.Node node5 = node4; node5 != null; node5 = node5.next)
				{
					if (node5 == node4)
					{
						segment.start = node5;
					}
					if (node5.next == null)
					{
						segment.end = node5;
					}
				}
				list2.Add(segment);
				PathNode node = unconnectedNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => x.monument == node.monument));
				unconnectedNodeList.RemoveAll((PathNode x) => x.monument == node.monument);
				secondaryNodeList.Remove(node);
				list4.Add(node);
				PathNode pathNode = secondaryNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				if (pathNode != null)
				{
					secondaryNodeList.Remove(pathNode);
					list4.Add(pathNode);
				}
				for (PathFinder.Node node6 = node4; node6 != null; node6 = node6.next)
				{
					if (node6 != node4 && node6.next != null)
					{
						list5.Add(node6.point);
					}
				}
			}
			foreach (PathSegment item4 in list2)
			{
				PathFinder.Node node7 = item4.start;
				while (node7 != null && node7.next != null)
				{
					DungeonGridConnectionHash dungeonGridConnectionHash = hashmap[node7.point.x, node7.point.y];
					DungeonGridConnectionHash dungeonGridConnectionHash2 = hashmap[node7.next.point.x, node7.next.point.y];
					if (node7.point.x > node7.next.point.x)
					{
						dungeonGridConnectionHash.West = true;
						dungeonGridConnectionHash2.East = true;
					}
					if (node7.point.x < node7.next.point.x)
					{
						dungeonGridConnectionHash.East = true;
						dungeonGridConnectionHash2.West = true;
					}
					if (node7.point.y > node7.next.point.y)
					{
						dungeonGridConnectionHash.South = true;
						dungeonGridConnectionHash2.North = true;
					}
					if (node7.point.y < node7.next.point.y)
					{
						dungeonGridConnectionHash.North = true;
						dungeonGridConnectionHash2.South = true;
					}
					hashmap[node7.point.x, node7.point.y] = dungeonGridConnectionHash;
					hashmap[node7.next.point.x, node7.next.point.y] = dungeonGridConnectionHash2;
					node7 = node7.next;
				}
			}
			for (int m = 0; m < worldSpaceGrid.CellCount; m++)
			{
				for (int n = 0; n < worldSpaceGrid.CellCount; n++)
				{
					if (array6[m, n] == int.MaxValue)
					{
						continue;
					}
					DungeonGridConnectionHash dungeonGridConnectionHash3 = hashmap[m, n];
					if (dungeonGridConnectionHash3.Value == 0)
					{
						continue;
					}
					ArrayEx.Shuffle(array, ref seed);
					Prefab<DungeonGridCell>[] array7 = array;
					foreach (Prefab<DungeonGridCell> prefab7 in array7)
					{
						Prefab<DungeonGridCell> prefab8 = ((m > num) ? worldSpaceGrid[m - 1, n] : null);
						if (((prefab8 != null) ? ((prefab7.Component.West == prefab8.Component.East) ? 1 : 0) : (dungeonGridConnectionHash3.West ? ((int)prefab7.Component.West) : ((prefab7.Component.West == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab9 = ((m < num2) ? worldSpaceGrid[m + 1, n] : null);
						if (((prefab9 != null) ? ((prefab7.Component.East == prefab9.Component.West) ? 1 : 0) : (dungeonGridConnectionHash3.East ? ((int)prefab7.Component.East) : ((prefab7.Component.East == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab10 = ((n > num) ? worldSpaceGrid[m, n - 1] : null);
						if (((prefab10 != null) ? ((prefab7.Component.South == prefab10.Component.North) ? 1 : 0) : (dungeonGridConnectionHash3.South ? ((int)prefab7.Component.South) : ((prefab7.Component.South == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab11 = ((n < num2) ? worldSpaceGrid[m, n + 1] : null);
						if (((prefab11 != null) ? (prefab7.Component.North == prefab11.Component.South) : (dungeonGridConnectionHash3.North ? ((byte)prefab7.Component.North != 0) : (prefab7.Component.North == DungeonGridConnectionType.None))) && (prefab7.Component.West == DungeonGridConnectionType.None || prefab8 == null || !prefab7.Component.ShouldAvoid(prefab8.ID)) && (prefab7.Component.East == DungeonGridConnectionType.None || prefab9 == null || !prefab7.Component.ShouldAvoid(prefab9.ID)) && (prefab7.Component.South == DungeonGridConnectionType.None || prefab10 == null || !prefab7.Component.ShouldAvoid(prefab10.ID)) && (prefab7.Component.North == DungeonGridConnectionType.None || prefab11 == null || !prefab7.Component.ShouldAvoid(prefab11.ID)))
						{
							worldSpaceGrid[m, n] = prefab7;
							bool num5 = prefab8 == null || prefab7.Component.WestVariant == prefab8.Component.EastVariant;
							bool flag = prefab10 == null || prefab7.Component.SouthVariant == prefab10.Component.NorthVariant;
							if (num5 && flag)
							{
								break;
							}
						}
					}
				}
			}
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			Vector3 vector2 = Vector3.up * 10f;
			Vector3 vector3 = Vector3.up * (LinkTransition + 1f);
			do
			{
				zero3 = zero2;
				for (int num6 = 0; num6 < worldSpaceGrid.CellCount; num6++)
				{
					for (int num7 = 0; num7 < worldSpaceGrid.CellCount; num7++)
					{
						Prefab<DungeonGridCell> prefab12 = worldSpaceGrid[num6, num7];
						if (prefab12 != null)
						{
							Vector2i cellPos2 = new Vector2i(num6, num7);
							Vector3 vector4 = worldSpaceGrid.GridToWorldCoords(cellPos2);
							while (!prefab12.CheckEnvironmentVolumesInsideTerrain(zero2 + vector4 + vector2, Quaternion.identity, Vector3.one, EnvironmentType.Underground) || prefab12.CheckEnvironmentVolumes(zero2 + vector4 + vector3, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building) || prefab12.CheckEnvironmentVolumes(zero2 + vector4, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building))
							{
								zero2.y -= 9f;
							}
						}
					}
				}
			}
			while (zero2 != zero3);
			foreach (PathLink item5 in list3)
			{
				item5.upwards.origin.position += zero2;
			}
			foreach (PathLink item6 in list3)
			{
				Vector3 vector5 = item6.upwards.origin.position + item6.upwards.origin.rotation * Vector3.Scale(item6.upwards.origin.upSocket.localPosition, item6.upwards.origin.scale);
				Vector3 vector6 = item6.downwards.origin.position + item6.downwards.origin.rotation * Vector3.Scale(item6.downwards.origin.downSocket.localPosition, item6.downwards.origin.scale) - vector5;
				Vector3[] array8 = new Vector3[2]
				{
					new Vector3(0f, 1f, 0f),
					new Vector3(1f, 1f, 1f)
				};
				for (int k = 0; k < array8.Length; k++)
				{
					Vector3 b2 = array8[k];
					int num8 = 0;
					int num9 = 0;
					while (vector6.magnitude > 1f && (num8 < 8 || num9 < 8))
					{
						bool flag2 = num8 > 2 && num9 > 2;
						bool flag3 = num8 > 4 && num9 > 4;
						Prefab<DungeonGridLink> prefab13 = null;
						Vector3 vector7 = Vector3.zero;
						int num10 = int.MinValue;
						Vector3 position3 = Vector3.zero;
						Quaternion rotation = Quaternion.identity;
						PathLinkSegment prevSegment = item6.downwards.prevSegment;
						Vector3 vector8 = prevSegment.position + prevSegment.rotation * Vector3.Scale(prevSegment.scale, prevSegment.downSocket.localPosition);
						Quaternion quaternion = prevSegment.rotation * prevSegment.downSocket.localRotation;
						Prefab<DungeonGridLink>[] array9 = array5;
						foreach (Prefab<DungeonGridLink> prefab14 in array9)
						{
							float num11 = SeedRandom.Value(ref seed);
							DungeonGridLink component = prefab14.Component;
							if (prevSegment.downType != component.UpType)
							{
								continue;
							}
							switch (component.DownType)
							{
							case DungeonGridLinkType.Elevator:
								if (flag2 || b2.x != 0f || b2.z != 0f)
								{
									continue;
								}
								break;
							case DungeonGridLinkType.Transition:
								if (b2.x != 0f || b2.z != 0f)
								{
									continue;
								}
								break;
							}
							int num12 = ((!flag2) ? component.Priority : 0);
							if (num10 > num12)
							{
								continue;
							}
							Quaternion quaternion2 = quaternion * Quaternion.Inverse(component.UpSocket.localRotation);
							Quaternion quaternion3 = quaternion2 * component.DownSocket.localRotation;
							PathLinkSegment prevSegment2 = item6.upwards.prevSegment;
							Quaternion quaternion4 = prevSegment2.rotation * prevSegment2.upSocket.localRotation;
							if (component.Rotation > 0)
							{
								if (Quaternion.Angle(quaternion4, quaternion3) > (float)component.Rotation)
								{
									continue;
								}
								Quaternion quaternion5 = quaternion4 * Quaternion.Inverse(quaternion3);
								quaternion2 *= quaternion5;
								quaternion3 *= quaternion5;
							}
							Vector3 vector9 = vector8 - quaternion2 * component.UpSocket.localPosition;
							Vector3 vector10 = quaternion2 * (component.DownSocket.localPosition - component.UpSocket.localPosition);
							Vector3 a2 = vector6 + vector7;
							Vector3 a3 = vector6 + vector10;
							float magnitude = a2.magnitude;
							float magnitude2 = a3.magnitude;
							Vector3 vector11 = Vector3.Scale(a2, b2);
							Vector3 vector12 = Vector3.Scale(a3, b2);
							float magnitude3 = vector11.magnitude;
							float magnitude4 = vector12.magnitude;
							if (vector7 != Vector3.zero)
							{
								if (magnitude3 < magnitude4 || (magnitude3 == magnitude4 && magnitude < magnitude2) || (magnitude3 == magnitude4 && magnitude == magnitude2 && num11 < 0.5f))
								{
									continue;
								}
							}
							else if (magnitude3 <= magnitude4)
							{
								continue;
							}
							if (Mathf.Abs(vector12.x) - Mathf.Abs(vector11.x) > 0.01f || (Mathf.Abs(vector12.x) > 0.01f && a2.x * a3.x < 0f) || Mathf.Abs(vector12.y) - Mathf.Abs(vector11.y) > 0.01f || (Mathf.Abs(vector12.y) > 0.01f && a2.y * a3.y < 0f) || Mathf.Abs(vector12.z) - Mathf.Abs(vector11.z) > 0.01f || (Mathf.Abs(vector12.z) > 0.01f && a2.z * a3.z < 0f) || (flag2 && b2.x == 0f && b2.z == 0f && component.DownType == DungeonGridLinkType.Default && ((Mathf.Abs(a3.x) > 0.01f && Mathf.Abs(a3.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(a3.z) > 0.01f && Mathf.Abs(a3.z) < LinkRadius * 2f - 0.1f))))
							{
								continue;
							}
							num10 = num12;
							if (b2.x == 0f && b2.z == 0f)
							{
								if (!flag2 && Mathf.Abs(a3.y) < LinkTransition - 0.1f)
								{
									continue;
								}
							}
							else if ((!flag2 && magnitude4 > 0.01f && (Mathf.Abs(a3.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(a3.z) < LinkRadius * 2f - 0.1f)) || (!flag3 && magnitude4 > 0.01f && (Mathf.Abs(a3.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(a3.z) < LinkRadius * 1f - 0.1f)))
							{
								continue;
							}
							if (!flag2 || !(magnitude4 < 0.01f) || !(magnitude2 < 0.01f) || !(Quaternion.Angle(quaternion4, quaternion3) > 10f))
							{
								prefab13 = prefab14;
								vector7 = vector10;
								num10 = num12;
								position3 = vector9;
								rotation = quaternion2;
							}
						}
						if (vector7 != Vector3.zero)
						{
							PathLinkSegment pathLinkSegment = new PathLinkSegment();
							pathLinkSegment.position = position3;
							pathLinkSegment.rotation = rotation;
							pathLinkSegment.scale = Vector3.one;
							pathLinkSegment.prefab = prefab13;
							pathLinkSegment.link = prefab13.Component;
							item6.downwards.segments.Add(pathLinkSegment);
							vector6 += vector7;
						}
						else
						{
							num9++;
						}
						if (b2.x > 0f || b2.z > 0f)
						{
							Prefab<DungeonGridLink> prefab15 = null;
							Vector3 vector13 = Vector3.zero;
							int num13 = int.MinValue;
							Vector3 position4 = Vector3.zero;
							Quaternion rotation2 = Quaternion.identity;
							PathLinkSegment prevSegment3 = item6.upwards.prevSegment;
							Vector3 vector14 = prevSegment3.position + prevSegment3.rotation * Vector3.Scale(prevSegment3.scale, prevSegment3.upSocket.localPosition);
							Quaternion quaternion6 = prevSegment3.rotation * prevSegment3.upSocket.localRotation;
							array9 = array5;
							foreach (Prefab<DungeonGridLink> prefab16 in array9)
							{
								float num14 = SeedRandom.Value(ref seed);
								DungeonGridLink component2 = prefab16.Component;
								if (prevSegment3.upType != component2.DownType)
								{
									continue;
								}
								switch (component2.DownType)
								{
								case DungeonGridLinkType.Elevator:
									if (flag2 || b2.x != 0f || b2.z != 0f)
									{
										continue;
									}
									break;
								case DungeonGridLinkType.Transition:
									if (b2.x != 0f || b2.z != 0f)
									{
										continue;
									}
									break;
								}
								int num15 = ((!flag2) ? component2.Priority : 0);
								if (num13 > num15)
								{
									continue;
								}
								Quaternion quaternion7 = quaternion6 * Quaternion.Inverse(component2.DownSocket.localRotation);
								Quaternion quaternion8 = quaternion7 * component2.UpSocket.localRotation;
								PathLinkSegment prevSegment4 = item6.downwards.prevSegment;
								Quaternion quaternion9 = prevSegment4.rotation * prevSegment4.downSocket.localRotation;
								if (component2.Rotation > 0)
								{
									if (Quaternion.Angle(quaternion9, quaternion8) > (float)component2.Rotation)
									{
										continue;
									}
									Quaternion quaternion10 = quaternion9 * Quaternion.Inverse(quaternion8);
									quaternion7 *= quaternion10;
									quaternion8 *= quaternion10;
								}
								Vector3 vector15 = vector14 - quaternion7 * component2.DownSocket.localPosition;
								Vector3 vector16 = quaternion7 * (component2.UpSocket.localPosition - component2.DownSocket.localPosition);
								Vector3 a4 = vector6 - vector13;
								Vector3 a5 = vector6 - vector16;
								float magnitude5 = a4.magnitude;
								float magnitude6 = a5.magnitude;
								Vector3 vector17 = Vector3.Scale(a4, b2);
								Vector3 vector18 = Vector3.Scale(a5, b2);
								float magnitude7 = vector17.magnitude;
								float magnitude8 = vector18.magnitude;
								if (vector13 != Vector3.zero)
								{
									if (magnitude7 < magnitude8 || (magnitude7 == magnitude8 && magnitude5 < magnitude6) || (magnitude7 == magnitude8 && magnitude5 == magnitude6 && num14 < 0.5f))
									{
										continue;
									}
								}
								else if (magnitude7 <= magnitude8)
								{
									continue;
								}
								if (Mathf.Abs(vector18.x) - Mathf.Abs(vector17.x) > 0.01f || (Mathf.Abs(vector18.x) > 0.01f && a4.x * a5.x < 0f) || Mathf.Abs(vector18.y) - Mathf.Abs(vector17.y) > 0.01f || (Mathf.Abs(vector18.y) > 0.01f && a4.y * a5.y < 0f) || Mathf.Abs(vector18.z) - Mathf.Abs(vector17.z) > 0.01f || (Mathf.Abs(vector18.z) > 0.01f && a4.z * a5.z < 0f) || (flag2 && b2.x == 0f && b2.z == 0f && component2.UpType == DungeonGridLinkType.Default && ((Mathf.Abs(a5.x) > 0.01f && Mathf.Abs(a5.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(a5.z) > 0.01f && Mathf.Abs(a5.z) < LinkRadius * 2f - 0.1f))))
								{
									continue;
								}
								num13 = num15;
								if (b2.x == 0f && b2.z == 0f)
								{
									if (!flag2 && Mathf.Abs(a5.y) < LinkTransition - 0.1f)
									{
										continue;
									}
								}
								else if ((!flag2 && magnitude8 > 0.01f && (Mathf.Abs(a5.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(a5.z) < LinkRadius * 2f - 0.1f)) || (!flag3 && magnitude8 > 0.01f && (Mathf.Abs(a5.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(a5.z) < LinkRadius * 1f - 0.1f)))
								{
									continue;
								}
								if (!flag2 || !(magnitude8 < 0.01f) || !(magnitude6 < 0.01f) || !(Quaternion.Angle(quaternion9, quaternion8) > 10f))
								{
									prefab15 = prefab16;
									vector13 = vector16;
									num13 = num15;
									position4 = vector15;
									rotation2 = quaternion7;
								}
							}
							if (vector13 != Vector3.zero)
							{
								PathLinkSegment pathLinkSegment2 = new PathLinkSegment();
								pathLinkSegment2.position = position4;
								pathLinkSegment2.rotation = rotation2;
								pathLinkSegment2.scale = Vector3.one;
								pathLinkSegment2.prefab = prefab15;
								pathLinkSegment2.link = prefab15.Component;
								item6.upwards.segments.Add(pathLinkSegment2);
								vector6 -= vector13;
							}
							else
							{
								num8++;
							}
						}
						else
						{
							num8++;
						}
					}
				}
			}
			foreach (PathLink item7 in list3)
			{
				foreach (PathLinkSegment segment2 in item7.downwards.segments)
				{
					World.AddPrefab("Dungeon", segment2.prefab, segment2.position, segment2.rotation, segment2.scale);
				}
				foreach (PathLinkSegment segment3 in item7.upwards.segments)
				{
					World.AddPrefab("Dungeon", segment3.prefab, segment3.position, segment3.rotation, segment3.scale);
				}
			}
			if (TerrainMeta.Path.Rails.Count > 0)
			{
				List<PrefabReplacement> list8 = new List<PrefabReplacement>();
				for (int num16 = 0; num16 < worldSpaceGrid.CellCount; num16++)
				{
					for (int num17 = 0; num17 < worldSpaceGrid.CellCount; num17++)
					{
						Prefab<DungeonGridCell> prefab17 = worldSpaceGrid[num16, num17];
						if (prefab17 == null || !prefab17.Component.Replaceable)
						{
							continue;
						}
						Vector2i vector2i2 = new Vector2i(num16, num17);
						Vector3 vector19 = worldSpaceGrid.GridToWorldCoords(vector2i2) + zero2;
						Prefab<DungeonGridCell>[] array7 = array3;
						foreach (Prefab<DungeonGridCell> prefab18 in array7)
						{
							if (prefab17.Component.North != prefab18.Component.North || prefab17.Component.South != prefab18.Component.South || prefab17.Component.West != prefab18.Component.West || prefab17.Component.East != prefab18.Component.East || !prefab18.CheckEnvironmentVolumesInsideTerrain(vector19 + vector2, Quaternion.identity, Vector3.one, EnvironmentType.TrainTunnels) || prefab18.CheckEnvironmentVolumes(vector19 + vector3, Quaternion.identity, Vector3.one, EnvironmentType.Underground) || prefab18.CheckEnvironmentVolumes(vector19, Quaternion.identity, Vector3.one, EnvironmentType.Underground) || !prefab18.ApplyTerrainChecks(vector19, Quaternion.identity, Vector3.one) || !prefab18.ApplyTerrainFilters(vector19, Quaternion.identity, Vector3.one))
							{
								continue;
							}
							MonumentInfo componentInChildren6 = prefab18.Object.GetComponentInChildren<MonumentInfo>();
							Vector3 vector20 = vector19;
							if ((bool)componentInChildren6)
							{
								vector20 += componentInChildren6.transform.position;
							}
							if (!(vector20.y < 1f))
							{
								float distanceToAboveGroundRail = GetDistanceToAboveGroundRail(vector20);
								if (!(distanceToAboveGroundRail < 200f))
								{
									PrefabReplacement item = default(PrefabReplacement);
									item.gridPosition = vector2i2;
									item.worldPosition = vector20;
									item.distance = Mathf.RoundToInt(distanceToAboveGroundRail);
									item.prefab = prefab18;
									list8.Add(item);
								}
							}
						}
					}
				}
				list8.Shuffle(ref seed);
				list8.Sort((PrefabReplacement a, PrefabReplacement b) => a.distance.CompareTo(b.distance));
				int num18 = 2;
				while (num18 > 0 && list8.Count > 0)
				{
					num18--;
					PrefabReplacement replacement = list8[0];
					worldSpaceGrid[replacement.gridPosition.x, replacement.gridPosition.y] = replacement.prefab;
					list8.RemoveAll((PrefabReplacement a) => (a.worldPosition - replacement.worldPosition).magnitude < 1500f);
				}
			}
			for (int num19 = 0; num19 < worldSpaceGrid.CellCount; num19++)
			{
				for (int num20 = 0; num20 < worldSpaceGrid.CellCount; num20++)
				{
					Prefab<DungeonGridCell> prefab19 = worldSpaceGrid[num19, num20];
					if (prefab19 != null)
					{
						Vector2i cellPos3 = new Vector2i(num19, num20);
						Vector3 vector21 = worldSpaceGrid.GridToWorldCoords(cellPos3);
						World.AddPrefab("Dungeon", prefab19, zero2 + vector21, Quaternion.identity, Vector3.one);
					}
				}
			}
			for (int num21 = 0; num21 < worldSpaceGrid.CellCount - 1; num21++)
			{
				for (int num22 = 0; num22 < worldSpaceGrid.CellCount - 1; num22++)
				{
					Prefab<DungeonGridCell> prefab20 = worldSpaceGrid[num21, num22];
					Prefab<DungeonGridCell> prefab21 = worldSpaceGrid[num21 + 1, num22];
					Prefab<DungeonGridCell> prefab22 = worldSpaceGrid[num21, num22 + 1];
					Prefab<DungeonGridCell>[] array7;
					if (prefab20 != null && prefab21 != null && prefab20.Component.EastVariant != prefab21.Component.WestVariant)
					{
						ArrayEx.Shuffle(array4, ref seed);
						array7 = array4;
						foreach (Prefab<DungeonGridCell> prefab23 in array7)
						{
							if (prefab23.Component.West == prefab20.Component.East && prefab23.Component.East == prefab21.Component.West && prefab23.Component.WestVariant == prefab20.Component.EastVariant && prefab23.Component.EastVariant == prefab21.Component.WestVariant)
							{
								Vector2i cellPos4 = new Vector2i(num21, num22);
								Vector3 vector22 = worldSpaceGrid.GridToWorldCoords(cellPos4) + new Vector3(worldSpaceGrid.CellSizeHalf, 0f, 0f);
								World.AddPrefab("Dungeon", prefab23, zero2 + vector22, Quaternion.identity, Vector3.one);
								break;
							}
						}
					}
					if (prefab20 == null || prefab22 == null || prefab20.Component.NorthVariant == prefab22.Component.SouthVariant)
					{
						continue;
					}
					ArrayEx.Shuffle(array4, ref seed);
					array7 = array4;
					foreach (Prefab<DungeonGridCell> prefab24 in array7)
					{
						if (prefab24.Component.South == prefab20.Component.North && prefab24.Component.North == prefab22.Component.South && prefab24.Component.SouthVariant == prefab20.Component.NorthVariant && prefab24.Component.NorthVariant == prefab22.Component.SouthVariant)
						{
							Vector2i cellPos5 = new Vector2i(num21, num22);
							Vector3 vector23 = worldSpaceGrid.GridToWorldCoords(cellPos5) + new Vector3(0f, 0f, worldSpaceGrid.CellSizeHalf);
							World.AddPrefab("Dungeon", prefab24, zero2 + vector23, Quaternion.identity, Vector3.one);
							break;
						}
					}
				}
			}
			if ((bool)TerrainMeta.Path)
			{
				TerrainMeta.Path.DungeonGridRoot = HierarchyUtil.GetRoot("Dungeon");
			}
		}
	}

	private float GetDistanceToAboveGroundRail(Vector3 pos)
	{
		float num = float.MaxValue;
		if ((bool)TerrainMeta.Path)
		{
			foreach (PathList rail in TerrainMeta.Path.Rails)
			{
				Vector3[] points = rail.Path.Points;
				foreach (Vector3 a in points)
				{
					num = Mathf.Min(num, Vector3Ex.Distance2D(a, pos));
				}
			}
		}
		return num;
	}
}
