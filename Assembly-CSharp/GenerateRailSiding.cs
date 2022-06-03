using System.Collections.Generic;
using UnityEngine;

public class GenerateRailSiding : ProceduralComponent
{
	public const float Width = 4f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 1f;

	public const float OuterFade = 16f;

	public const float RandomScale = 1f;

	public const float MeshOffset = 0f;

	public const float TerrainOffset = -0.125f;

	private static Quaternion rotRight = Quaternion.Euler(0f, 90f, 0f);

	private static Quaternion rotLeft = Quaternion.Euler(0f, -90f, 0f);

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
			Splat = 128,
			Hierarchy = 2
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
		int min2 = Mathf.RoundToInt(13.333333f);
		int max2 = Mathf.RoundToInt(20f);
		int num = 8;
		float num2 = 16f * 16f;
		List<PathList> list = new List<PathList>();
		int[,] array = TerrainPath.CreateRailCostmap(ref seed);
		new PathFinder(array);
		array.GetLength(0);
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			foreach (PathList rail2 in TerrainMeta.Path.Rails)
			{
				if (rail == rail2)
				{
					continue;
				}
				Vector3[] points = rail.Path.Points;
				foreach (Vector3 vector in points)
				{
					Vector3[] points2 = rail2.Path.Points;
					foreach (Vector3 vector2 in points2)
					{
						if ((vector - vector2).sqrMagnitude < num2)
						{
							hashSet.Add(vector);
							break;
						}
					}
				}
			}
		}
		foreach (PathList rail3 in TerrainMeta.Path.Rails)
		{
			PathInterpolator path = rail3.Path;
			Vector3[] points3 = path.Points;
			Vector3[] tangents = path.Tangents;
			int num3 = path.MinIndex + 1 + num * 2;
			int num4 = path.MaxIndex - 1 - num * 2;
			for (int k = num3; k <= num4; k++)
			{
				list2.Clear();
				list3.Clear();
				int num5 = SeedRandom.Range(ref seed, min2, max2);
				int num6 = SeedRandom.Range(ref seed, min, max);
				int num7 = k;
				int num8 = k + num5;
				if (num8 >= num4)
				{
					continue;
				}
				Vector3 from = tangents[num7];
				Vector3 to = tangents[num8];
				if (Vector3.Angle(from, to) > 30f)
				{
					continue;
				}
				Vector3 to2 = tangents[num7];
				Vector3 to3 = tangents[num8];
				Vector3 from2 = Vector3.Normalize(points3[num7 + num] - points3[num7]);
				Vector3 from3 = Vector3.Normalize(points3[num8] - points3[num8 - num]);
				float num9 = Vector3.SignedAngle(from2, to2, Vector3.up);
				float f = 0f - Vector3.SignedAngle(from3, to3, Vector3.up);
				if (Mathf.Sign(num9) != Mathf.Sign(f) || Mathf.Abs(num9) > 60f || Mathf.Abs(f) > 60f)
				{
					continue;
				}
				float num10 = 4f;
				Quaternion quaternion = ((num9 > 0f) ? rotRight : rotLeft);
				for (int l = num7 - num; l <= num8 + num; l++)
				{
					Vector3 item = points3[l];
					if (hashSet.Contains(item))
					{
						list2.Clear();
						list3.Clear();
						break;
					}
					Vector3 vector3 = tangents[l];
					Vector3 vector4 = quaternion * vector3;
					if (l < num7 + num)
					{
						float t = Mathf.InverseLerp(num7 - num, num7, l);
						float num11 = Mathf.SmoothStep(0f, 1f, t) * num10;
						item += vector4 * num11;
					}
					else if (l > num8 - num)
					{
						float t2 = Mathf.InverseLerp(num8 + num, num8, l);
						float num12 = Mathf.SmoothStep(0f, 1f, t2) * num10;
						item += vector4 * num12;
					}
					else
					{
						item += vector4 * num10;
					}
					list2.Add(item);
					list3.Add(vector3);
				}
				if (list2.Count >= 2)
				{
					int number = TerrainMeta.Path.Rails.Count + list.Count;
					PathList pathList = CreateSegment(number, list2.ToArray());
					pathList.Start = false;
					pathList.End = false;
					list.Add(pathList);
					k += num5;
				}
				k += num6;
			}
		}
		foreach (PathList item2 in list)
		{
			item2.Path.RecalculateLength();
			item2.Path.Resample(7.5f);
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
