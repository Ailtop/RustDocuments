using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class TerrainMap : TerrainExtension
{
	internal int res;

	public void ApplyFilter(float normX, float normZ, float radius, float fade, Action<int, int, float> action)
	{
		float num = TerrainMeta.OneOverSize.x * radius;
		float num2 = TerrainMeta.OneOverSize.x * fade;
		float num3 = (float)res * (num - num2);
		float num4 = (float)res * num;
		float num5 = normX * (float)res;
		float num6 = normZ * (float)res;
		int num7 = Index(normX - num);
		int num8 = Index(normX + num);
		int num9 = Index(normZ - num);
		int num10 = Index(normZ + num);
		if (num3 != num4)
		{
			for (int i = num9; i <= num10; i++)
			{
				for (int j = num7; j <= num8; j++)
				{
					float magnitude = new Vector2((float)j + 0.5f - num5, (float)i + 0.5f - num6).magnitude;
					float arg = Mathf.InverseLerp(num4, num3, magnitude);
					action(j, i, arg);
				}
			}
			return;
		}
		for (int k = num9; k <= num10; k++)
		{
			for (int l = num7; l <= num8; l++)
			{
				float arg2 = ((new Vector2((float)l + 0.5f - num5, (float)k + 0.5f - num6).magnitude < num4) ? 1 : 0);
				action(l, k, arg2);
			}
		}
	}

	public void ForEach(Vector3 worldPos, float radius, Action<int, int> action)
	{
		int num = Index(TerrainMeta.NormalizeX(worldPos.x - radius));
		int num2 = Index(TerrainMeta.NormalizeX(worldPos.x + radius));
		int num3 = Index(TerrainMeta.NormalizeZ(worldPos.z - radius));
		int num4 = Index(TerrainMeta.NormalizeZ(worldPos.z + radius));
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				action(j, i);
			}
		}
	}

	public void ForEach(float normX, float normZ, float normRadius, Action<int, int> action)
	{
		int num = Index(normX - normRadius);
		int num2 = Index(normX + normRadius);
		int num3 = Index(normZ - normRadius);
		int num4 = Index(normZ + normRadius);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				action(j, i);
			}
		}
	}

	public void ForEachParallel(Vector3 v0, Vector3 v1, Vector3 v2, Action<int, int> action)
	{
		Vector2i v3 = new Vector2i(Index(TerrainMeta.NormalizeX(v0.x)), Index(TerrainMeta.NormalizeZ(v0.z)));
		Vector2i v4 = new Vector2i(Index(TerrainMeta.NormalizeX(v1.x)), Index(TerrainMeta.NormalizeZ(v1.z)));
		Vector2i v5 = new Vector2i(Index(TerrainMeta.NormalizeX(v2.x)), Index(TerrainMeta.NormalizeZ(v2.z)));
		ForEachParallel(v3, v4, v5, action);
	}

	public void ForEachParallel(Vector2i v0, Vector2i v1, Vector2i v2, Action<int, int> action)
	{
		int x = Mathx.Min(v0.x, v1.x, v2.x);
		int x2 = Mathx.Max(v0.x, v1.x, v2.x);
		int y = Mathx.Min(v0.y, v1.y, v2.y);
		int y2 = Mathx.Max(v0.y, v1.y, v2.y);
		Vector2i base_min = new Vector2i(x, y);
		Vector2i vector2i = new Vector2i(x2, y2);
		Vector2i base_count = vector2i - base_min + Vector2i.one;
		Parallel.Call(delegate(int thread_id, int thread_count)
		{
			Vector2i min = base_min + base_count * thread_id / thread_count;
			Vector2i max = base_min + base_count * (thread_id + 1) / thread_count - Vector2i.one;
			ForEachInternal(v0, v1, v2, action, min, max);
		});
	}

	public void ForEach(Vector3 v0, Vector3 v1, Vector3 v2, Action<int, int> action)
	{
		Vector2i v3 = new Vector2i(Index(TerrainMeta.NormalizeX(v0.x)), Index(TerrainMeta.NormalizeZ(v0.z)));
		Vector2i v4 = new Vector2i(Index(TerrainMeta.NormalizeX(v1.x)), Index(TerrainMeta.NormalizeZ(v1.z)));
		Vector2i v5 = new Vector2i(Index(TerrainMeta.NormalizeX(v2.x)), Index(TerrainMeta.NormalizeZ(v2.z)));
		ForEach(v3, v4, v5, action);
	}

	public void ForEach(Vector2i v0, Vector2i v1, Vector2i v2, Action<int, int> action)
	{
		Vector2i min = new Vector2i(int.MinValue, int.MinValue);
		Vector2i max = new Vector2i(int.MaxValue, int.MaxValue);
		ForEachInternal(v0, v1, v2, action, min, max);
	}

	private void ForEachInternal(Vector2i v0, Vector2i v1, Vector2i v2, Action<int, int> action, Vector2i min, Vector2i max)
	{
		int x = Mathf.Max(min.x, Mathx.Min(v0.x, v1.x, v2.x));
		int num = Mathf.Min(max.x, Mathx.Max(v0.x, v1.x, v2.x));
		int y = Mathf.Max(min.y, Mathx.Min(v0.y, v1.y, v2.y));
		int num2 = Mathf.Min(max.y, Mathx.Max(v0.y, v1.y, v2.y));
		int num3 = v0.y - v1.y;
		int num4 = v1.x - v0.x;
		int num5 = v1.y - v2.y;
		int num6 = v2.x - v1.x;
		int num7 = v2.y - v0.y;
		int num8 = v0.x - v2.x;
		Vector2i vector2i = new Vector2i(x, y);
		int num9 = (v2.x - v1.x) * (vector2i.y - v1.y) - (v2.y - v1.y) * (vector2i.x - v1.x);
		int num10 = (v0.x - v2.x) * (vector2i.y - v2.y) - (v0.y - v2.y) * (vector2i.x - v2.x);
		int num11 = (v1.x - v0.x) * (vector2i.y - v0.y) - (v1.y - v0.y) * (vector2i.x - v0.x);
		vector2i.y = y;
		while (vector2i.y <= num2)
		{
			int num12 = num9;
			int num13 = num10;
			int num14 = num11;
			vector2i.x = x;
			while (vector2i.x <= num)
			{
				if ((num12 | num13 | num14) >= 0)
				{
					action(vector2i.x, vector2i.y);
				}
				num12 += num5;
				num13 += num7;
				num14 += num3;
				vector2i.x++;
			}
			num9 += num6;
			num10 += num8;
			num11 += num4;
			vector2i.y++;
		}
	}

	public void ForEachParallel(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Action<int, int> action)
	{
		Vector2i v4 = new Vector2i(Index(TerrainMeta.NormalizeX(v0.x)), Index(TerrainMeta.NormalizeZ(v0.z)));
		Vector2i v5 = new Vector2i(Index(TerrainMeta.NormalizeX(v1.x)), Index(TerrainMeta.NormalizeZ(v1.z)));
		Vector2i v6 = new Vector2i(Index(TerrainMeta.NormalizeX(v2.x)), Index(TerrainMeta.NormalizeZ(v2.z)));
		Vector2i v7 = new Vector2i(Index(TerrainMeta.NormalizeX(v3.x)), Index(TerrainMeta.NormalizeZ(v3.z)));
		ForEachParallel(v4, v5, v6, v7, action);
	}

	public void ForEachParallel(Vector2i v0, Vector2i v1, Vector2i v2, Vector2i v3, Action<int, int> action)
	{
		int x = Mathx.Min(v0.x, v1.x, v2.x, v3.x);
		int x2 = Mathx.Max(v0.x, v1.x, v2.x, v3.x);
		int y = Mathx.Min(v0.y, v1.y, v2.y, v3.y);
		int y2 = Mathx.Max(v0.y, v1.y, v2.y, v3.y);
		Vector2i base_min = new Vector2i(x, y);
		Vector2i vector2i = new Vector2i(x2, y2) - base_min + Vector2i.one;
		Vector2i size_x = new Vector2i(vector2i.x, 0);
		Vector2i size_y = new Vector2i(0, vector2i.y);
		Parallel.Call(delegate(int thread_id, int thread_count)
		{
			Vector2i min = base_min + size_y * thread_id / thread_count;
			Vector2i max = base_min + size_y * (thread_id + 1) / thread_count + size_x - Vector2i.one;
			ForEachInternal(v0, v1, v2, v3, action, min, max);
		});
	}

	public void ForEach(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Action<int, int> action)
	{
		Vector2i v4 = new Vector2i(Index(TerrainMeta.NormalizeX(v0.x)), Index(TerrainMeta.NormalizeZ(v0.z)));
		Vector2i v5 = new Vector2i(Index(TerrainMeta.NormalizeX(v1.x)), Index(TerrainMeta.NormalizeZ(v1.z)));
		Vector2i v6 = new Vector2i(Index(TerrainMeta.NormalizeX(v2.x)), Index(TerrainMeta.NormalizeZ(v2.z)));
		Vector2i v7 = new Vector2i(Index(TerrainMeta.NormalizeX(v3.x)), Index(TerrainMeta.NormalizeZ(v3.z)));
		ForEach(v4, v5, v6, v7, action);
	}

	public void ForEach(Vector2i v0, Vector2i v1, Vector2i v2, Vector2i v3, Action<int, int> action)
	{
		Vector2i min = new Vector2i(int.MinValue, int.MinValue);
		Vector2i max = new Vector2i(int.MaxValue, int.MaxValue);
		ForEachInternal(v0, v1, v2, v3, action, min, max);
	}

	private void ForEachInternal(Vector2i v0, Vector2i v1, Vector2i v2, Vector2i v3, Action<int, int> action, Vector2i min, Vector2i max)
	{
		int x = Mathf.Max(min.x, Mathx.Min(v0.x, v1.x, v2.x, v3.x));
		int num = Mathf.Min(max.x, Mathx.Max(v0.x, v1.x, v2.x, v3.x));
		int y = Mathf.Max(min.y, Mathx.Min(v0.y, v1.y, v2.y, v3.y));
		int num2 = Mathf.Min(max.y, Mathx.Max(v0.y, v1.y, v2.y, v3.y));
		int num3 = v0.y - v1.y;
		int num4 = v1.x - v0.x;
		int num5 = v1.y - v2.y;
		int num6 = v2.x - v1.x;
		int num7 = v2.y - v0.y;
		int num8 = v0.x - v2.x;
		int num9 = v3.y - v2.y;
		int num10 = v2.x - v3.x;
		int num11 = v2.y - v1.y;
		int num12 = v1.x - v2.x;
		int num13 = v1.y - v3.y;
		int num14 = v3.x - v1.x;
		Vector2i vector2i = new Vector2i(x, y);
		int num15 = (v2.x - v1.x) * (vector2i.y - v1.y) - (v2.y - v1.y) * (vector2i.x - v1.x);
		int num16 = (v0.x - v2.x) * (vector2i.y - v2.y) - (v0.y - v2.y) * (vector2i.x - v2.x);
		int num17 = (v1.x - v0.x) * (vector2i.y - v0.y) - (v1.y - v0.y) * (vector2i.x - v0.x);
		int num18 = (v1.x - v2.x) * (vector2i.y - v2.y) - (v1.y - v2.y) * (vector2i.x - v2.x);
		int num19 = (v3.x - v1.x) * (vector2i.y - v1.y) - (v3.y - v1.y) * (vector2i.x - v1.x);
		int num20 = (v2.x - v3.x) * (vector2i.y - v3.y) - (v2.y - v3.y) * (vector2i.x - v3.x);
		vector2i.y = y;
		while (vector2i.y <= num2)
		{
			int num21 = num15;
			int num22 = num16;
			int num23 = num17;
			int num24 = num18;
			int num25 = num19;
			int num26 = num20;
			vector2i.x = x;
			while (vector2i.x <= num)
			{
				if ((num21 | num22 | num23) >= 0 || (num24 | num25 | num26) >= 0)
				{
					action(vector2i.x, vector2i.y);
				}
				num21 += num5;
				num22 += num7;
				num23 += num3;
				num24 += num11;
				num25 += num13;
				num26 += num9;
				vector2i.x++;
			}
			num15 += num6;
			num16 += num8;
			num17 += num4;
			num18 += num12;
			num19 += num14;
			num20 += num10;
			vector2i.y++;
		}
	}

	public void ForEach(int x_min, int x_max, int z_min, int z_max, Action<int, int> action)
	{
		for (int i = z_min; i <= z_max; i++)
		{
			for (int j = x_min; j <= x_max; j++)
			{
				action(j, i);
			}
		}
	}

	public void ForEach(Action<int, int> action)
	{
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				action(j, i);
			}
		}
	}

	public int Index(float normalized)
	{
		int num = (int)(normalized * (float)res);
		if (num >= 0)
		{
			if (num <= res - 1)
			{
				return num;
			}
			return res - 1;
		}
		return 0;
	}

	public float Coordinate(int index)
	{
		return ((float)index + 0.5f) / (float)res;
	}
}
public abstract class TerrainMap<T> : TerrainMap where T : struct
{
	internal T[] src;

	internal T[] dst;

	public void Push()
	{
		if (src == dst)
		{
			dst = (T[])src.Clone();
		}
	}

	public void Pop()
	{
		if (src != dst)
		{
			Array.Copy(dst, src, src.Length);
			dst = src;
		}
	}

	public IEnumerable<T> ToEnumerable()
	{
		return src.Cast<T>();
	}

	public int BytesPerElement()
	{
		return Marshal.SizeOf(typeof(T));
	}

	public long GetMemoryUsage()
	{
		return (long)BytesPerElement() * (long)src.Length;
	}

	public byte[] ToByteArray()
	{
		byte[] array = new byte[BytesPerElement() * src.Length];
		Buffer.BlockCopy(src, 0, array, 0, array.Length);
		return array;
	}

	public void FromByteArray(byte[] dat)
	{
		Buffer.BlockCopy(dat, 0, dst, 0, dat.Length);
	}
}
