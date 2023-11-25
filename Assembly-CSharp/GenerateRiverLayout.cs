using System.Collections.Generic;
using UnityEngine;

public class GenerateRiverLayout : ProceduralComponent
{
	public const float Width = 36f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 10f;

	public const float OuterFade = 20f;

	public const float RandomScale = 0.75f;

	public const float MeshOffset = -0.5f;

	public const float TerrainOffset = -1.5f;

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Rivers.Clear();
			TerrainMeta.Path.Rivers.AddRange(World.GetPaths("River"));
		}
		else
		{
			if (!World.Config.Rivers)
			{
				return;
			}
			List<PathList> list = new List<PathList>();
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
			List<Vector3> list2 = new List<Vector3>();
			for (float num = TerrainMeta.Position.z; num < TerrainMeta.Position.z + TerrainMeta.Size.z; num += 50f)
			{
				for (float num2 = TerrainMeta.Position.x; num2 < TerrainMeta.Position.x + TerrainMeta.Size.x; num2 += 50f)
				{
					Vector3 vector = new Vector3(num2, 0f, num);
					float num3 = (vector.y = heightMap.GetHeight(vector));
					if (vector.y <= 5f)
					{
						continue;
					}
					Vector3 normal = heightMap.GetNormal(vector);
					if (normal.y <= 0.01f)
					{
						continue;
					}
					Vector2 normalized = new Vector2(normal.x, normal.z).normalized;
					list2.Add(vector);
					float radius = 18f;
					int num4 = 18;
					for (int i = 0; i < 10000; i++)
					{
						vector.x += normalized.x;
						vector.z += normalized.y;
						if (heightMap.GetSlope(vector) > 30f)
						{
							break;
						}
						float height = heightMap.GetHeight(vector);
						if (height > num3 + 10f)
						{
							break;
						}
						float num5 = Mathf.Min(height, num3);
						vector.y = Mathf.Lerp(vector.y, num5, 0.15f);
						int topology = topologyMap.GetTopology(vector, radius);
						int topology2 = topologyMap.GetTopology(vector);
						int num6 = 3742724;
						int num7 = 128;
						if ((topology & num6) != 0)
						{
							list2.Add(vector);
							break;
						}
						if ((topology2 & num7) != 0 && --num4 <= 0)
						{
							list2.Add(vector);
							if (list2.Count >= 25)
							{
								PathList pathList = new PathList("River " + (TerrainMeta.Path.Rivers.Count + list.Count), list2.ToArray());
								pathList.Spline = true;
								pathList.Width = 36f;
								pathList.InnerPadding = 1f;
								pathList.OuterPadding = 1f;
								pathList.InnerFade = 10f;
								pathList.OuterFade = 20f;
								pathList.RandomScale = 0.75f;
								pathList.MeshOffset = -0.5f;
								pathList.TerrainOffset = -1.5f;
								pathList.Topology = 16384;
								pathList.Splat = 64;
								pathList.Start = true;
								pathList.End = true;
								list.Add(pathList);
							}
							break;
						}
						if (i % 12 == 0)
						{
							list2.Add(vector);
						}
						normal = heightMap.GetNormal(vector);
						normalized = new Vector2(normalized.x + 0.15f * normal.x, normalized.y + 0.15f * normal.z).normalized;
						num3 = num5;
					}
					list2.Clear();
				}
			}
			list.Sort((PathList a, PathList b) => b.Path.Points.Length.CompareTo(a.Path.Points.Length));
			int num8 = Mathf.RoundToInt(10f * TerrainMeta.Size.x * TerrainMeta.Size.z * 1E-06f);
			int num9 = Mathf.NextPowerOfTwo((int)((float)World.Size / 36f));
			bool[,] array = new bool[num9, num9];
			for (int j = 0; j < list.Count; j++)
			{
				if (j >= num8)
				{
					list.RemoveUnordered(j--);
					continue;
				}
				PathList pathList2 = list[j];
				bool flag = false;
				for (int k = 0; k < j; k++)
				{
					if (Vector3.Distance(list[k].Path.GetStartPoint(), pathList2.Path.GetStartPoint()) < 100f)
					{
						list.RemoveUnordered(j--);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				int num10 = -1;
				int num11 = -1;
				for (int l = 0; l < pathList2.Path.Points.Length; l++)
				{
					Vector3 vector2 = pathList2.Path.Points[l];
					int num12 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(vector2.x) * (float)num9), 0, num9 - 1);
					int num13 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(vector2.z) * (float)num9), 0, num9 - 1);
					if (num10 == num12 && num11 == num13)
					{
						continue;
					}
					if (array[num13, num12])
					{
						list.RemoveUnordered(j--);
						flag = true;
						break;
					}
					if (num10 != num12 && num11 != num13)
					{
						if (num10 != -1)
						{
							array[num13, num10] = true;
						}
						if (num11 != -1)
						{
							array[num11, num12] = true;
						}
						num10 = num12;
						num11 = num13;
						array[num13, num12] = true;
					}
					else
					{
						num10 = num12;
						num11 = num13;
						array[num13, num12] = true;
					}
				}
			}
			for (int m = 0; m < list.Count; m++)
			{
				list[m].Name = "River " + (TerrainMeta.Path.Rivers.Count + m);
			}
			foreach (PathList item in list)
			{
				item.Path.Smoothen(4, new Vector3(1f, 0f, 1f));
				item.Path.Smoothen(8, new Vector3(0f, 1f, 0f));
				item.Path.Resample(7.5f);
				item.Path.RecalculateTangents();
			}
			TerrainMeta.Path.Rivers.AddRange(list);
		}
	}
}
