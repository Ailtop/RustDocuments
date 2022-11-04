using System.Collections.Generic;
using Facepunch;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/WorldPositionGenerator")]
public class WorldPositionGenerator : ScriptableObject
{
	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public bool aboveWater;

	public float MaxSlopeRadius;

	public float MaxSlopeDegrees = 90f;

	public float CheckSphereRadius;

	public LayerMask CheckSphereMask;

	private Vector3 _origin;

	private Vector3 _area;

	private ByteQuadtree _quadtree;

	public bool TrySample(Vector3 origin, float minDist, float maxDist, out Vector3 position, List<Vector3> blocked = null)
	{
		if (_quadtree == null)
		{
			PrecalculatePositions();
		}
		Rect inclusion = new Rect(origin.x - maxDist, origin.z - maxDist, maxDist * 2f, maxDist * 2f);
		Rect exclusion = new Rect(origin.x - minDist, origin.z - minDist, minDist * 2f, minDist * 2f);
		List<Rect> blockedRects = Pool.GetList<Rect>();
		if (blocked != null)
		{
			float num = 10f;
			foreach (Vector3 item2 in blocked)
			{
				Rect item = new Rect(item2.x - num, item2.z - num, num * 2f, num * 2f);
				blockedRects.Add(item);
			}
		}
		List<ByteQuadtree.Element> candidates = Pool.GetList<ByteQuadtree.Element>();
		candidates.Add(_quadtree.Root);
		for (int i = 0; i < candidates.Count; i++)
		{
			ByteQuadtree.Element element2 = candidates[i];
			if (!element2.IsLeaf)
			{
				candidates.RemoveUnordered(i--);
				EvaluateCandidate(element2.Child1);
				EvaluateCandidate(element2.Child2);
				EvaluateCandidate(element2.Child3);
				EvaluateCandidate(element2.Child4);
			}
		}
		if (candidates.Count == 0)
		{
			position = origin;
			Pool.FreeList(ref candidates);
			Pool.FreeList(ref blockedRects);
			return false;
		}
		Vector3 vector;
		if (CheckSphereRadius <= float.Epsilon)
		{
			ByteQuadtree.Element random = candidates.GetRandom();
			Rect rect = GetElementRect(random);
			vector = (rect.min + rect.size * new Vector2(Random.value, Random.value)).XZ3D();
		}
		else
		{
			Vector3 vector2;
			while (true)
			{
				if (candidates.Count == 0)
				{
					position = Vector3.zero;
					return false;
				}
				int index = Random.Range(0, candidates.Count);
				ByteQuadtree.Element element3 = candidates[index];
				vector2 = GetElementRect(element3).center.XZ3D();
				vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
				if (!Physics.CheckSphere(vector2, CheckSphereRadius, CheckSphereMask.value))
				{
					break;
				}
				candidates.RemoveAt(index);
			}
			vector = vector2;
		}
		position = vector.WithY(aboveWater ? TerrainMeta.WaterMap.GetHeight(vector) : TerrainMeta.HeightMap.GetHeight(vector));
		Pool.FreeList(ref candidates);
		Pool.FreeList(ref blockedRects);
		return true;
		void EvaluateCandidate(ByteQuadtree.Element child)
		{
			if (child.Value != 0)
			{
				Rect elementRect = GetElementRect(child);
				if (elementRect.Overlaps(inclusion) && (!exclusion.Contains(elementRect.min) || !exclusion.Contains(elementRect.max)))
				{
					if (blockedRects.Count > 0)
					{
						foreach (Rect item3 in blockedRects)
						{
							if (item3.Contains(elementRect.min) && item3.Contains(elementRect.max))
							{
								return;
							}
						}
					}
					candidates.Add(child);
				}
			}
		}
		Rect GetElementRect(ByteQuadtree.Element element)
		{
			int num2 = 1 << element.Depth;
			float num3 = 1f / (float)num2;
			Vector2 vector3 = element.Coords * num3;
			return new Rect(_origin.x + vector3.x * _area.x, _origin.z + vector3.y * _area.z, _area.x * num3, _area.z * num3);
		}
	}

	public void PrecalculatePositions()
	{
		int res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		byte[] map = new byte[res * res];
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				float normX = ((float)i + 0.5f) / (float)res;
				float normZ = ((float)z + 0.5f) / (float)res;
				float factor = Filter.GetFactor(normX, normZ);
				if (factor > 0f && MaxSlopeRadius > 0f)
				{
					TerrainMeta.HeightMap.ForEach(normX, normZ, MaxSlopeRadius / (float)res, delegate(int slopeX, int slopeZ)
					{
						if (TerrainMeta.HeightMap.GetSlope(slopeX, slopeZ) > MaxSlopeDegrees)
						{
							factor = 0f;
						}
					});
				}
				map[z * res + i] = (byte)((factor >= FilterCutoff) ? (255f * factor) : 0f);
			}
		});
		_origin = TerrainMeta.Position;
		_area = TerrainMeta.Size;
		_quadtree = new ByteQuadtree();
		_quadtree.UpdateValues(map);
	}
}
