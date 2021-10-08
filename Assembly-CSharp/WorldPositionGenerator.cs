using System.Collections.Generic;
using Facepunch;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/WorldPositionGenerator")]
public class WorldPositionGenerator : ScriptableObject
{
	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public bool aboveWater;

	private Vector3 _origin;

	private Vector3 _area;

	private ByteQuadtree _quadtree;

	public bool TrySample(Vector3 origin, float minDist, float maxDist, out Vector3 position, List<Vector3> blocked = null)
	{
		_003C_003Ec__DisplayClass6_0 _003C_003Ec__DisplayClass6_ = default(_003C_003Ec__DisplayClass6_0);
		_003C_003Ec__DisplayClass6_._003C_003E4__this = this;
		if (_quadtree == null)
		{
			PrecalculatePositions();
		}
		_003C_003Ec__DisplayClass6_.inclusion = new Rect(origin.x - maxDist, origin.z - maxDist, maxDist * 2f, maxDist * 2f);
		_003C_003Ec__DisplayClass6_.exclusion = new Rect(origin.x - minDist, origin.z - minDist, minDist * 2f, minDist * 2f);
		_003C_003Ec__DisplayClass6_.blockedRects = Pool.GetList<Rect>();
		if (blocked != null)
		{
			float num = 10f;
			foreach (Vector3 item2 in blocked)
			{
				Rect item = new Rect(item2.x - num, item2.z - num, num * 2f, num * 2f);
				_003C_003Ec__DisplayClass6_.blockedRects.Add(item);
			}
		}
		_003C_003Ec__DisplayClass6_.candidates = Pool.GetList<ByteQuadtree.Element>();
		_003C_003Ec__DisplayClass6_.candidates.Add(_quadtree.Root);
		for (int i = 0; i < _003C_003Ec__DisplayClass6_.candidates.Count; i++)
		{
			ByteQuadtree.Element element = _003C_003Ec__DisplayClass6_.candidates[i];
			if (!element.IsLeaf)
			{
				_003C_003Ec__DisplayClass6_.candidates.RemoveUnordered(i--);
				_003CTrySample_003Eg__EvaluateCandidate_007C6_0(element.Child1, ref _003C_003Ec__DisplayClass6_);
				_003CTrySample_003Eg__EvaluateCandidate_007C6_0(element.Child2, ref _003C_003Ec__DisplayClass6_);
				_003CTrySample_003Eg__EvaluateCandidate_007C6_0(element.Child3, ref _003C_003Ec__DisplayClass6_);
				_003CTrySample_003Eg__EvaluateCandidate_007C6_0(element.Child4, ref _003C_003Ec__DisplayClass6_);
			}
		}
		if (_003C_003Ec__DisplayClass6_.candidates.Count == 0)
		{
			position = origin;
			Pool.FreeList(ref _003C_003Ec__DisplayClass6_.candidates);
			Pool.FreeList(ref _003C_003Ec__DisplayClass6_.blockedRects);
			return false;
		}
		ByteQuadtree.Element random = _003C_003Ec__DisplayClass6_.candidates.GetRandom();
		Rect rect = _003CTrySample_003Eg__GetElementRect_007C6_1(random, ref _003C_003Ec__DisplayClass6_);
		Vector3 vector = (rect.min + rect.size * new Vector2(Random.value, Random.value)).XZ3D();
		position = vector.WithY(aboveWater ? TerrainMeta.WaterMap.GetHeight(vector) : TerrainMeta.HeightMap.GetHeight(vector));
		Pool.FreeList(ref _003C_003Ec__DisplayClass6_.candidates);
		Pool.FreeList(ref _003C_003Ec__DisplayClass6_.blockedRects);
		return true;
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
				map[z * res + i] = (byte)((factor >= FilterCutoff) ? (255f * factor) : 0f);
			}
		});
		_origin = TerrainMeta.Position;
		_area = TerrainMeta.Size;
		_quadtree = new ByteQuadtree();
		_quadtree.UpdateValues(map);
	}
}
