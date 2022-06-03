using System.Collections.Generic;
using UnityEngine;

namespace Rust.Interpolation;

public class Interpolator<T> where T : ISnapshot<T>, new()
{
	public struct Segment
	{
		public T tick;

		public T prev;

		public T next;
	}

	public List<T> list;

	public T last;

	public Interpolator(int listCount)
	{
		list = new List<T>(listCount);
	}

	public void Add(T tick)
	{
		last = tick;
		list.Add(tick);
	}

	public void Cull(float beforeTime)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Time < beforeTime)
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public void Clear()
	{
		list.Clear();
	}

	public Segment Query(float time, float interpolation, float extrapolation, float smoothing, ref T t)
	{
		Segment result = default(Segment);
		if (list.Count == 0)
		{
			result.prev = last;
			result.next = last;
			result.tick = last;
			return result;
		}
		float num = time - interpolation - smoothing * 0.5f;
		float num2 = Mathf.Min(time - interpolation, last.Time);
		float num3 = num2 - smoothing;
		T prev = list[0];
		T val = last;
		T prev2 = list[0];
		T val2 = last;
		foreach (T item in list)
		{
			if (item.Time < num3)
			{
				prev = item;
			}
			else if (val.Time >= item.Time)
			{
				val = item;
			}
			if (item.Time < num2)
			{
				prev2 = item;
			}
			else if (val2.Time >= item.Time)
			{
				val2 = item;
			}
		}
		T @new = t.GetNew();
		if (val.Time - prev.Time <= Mathf.Epsilon)
		{
			@new.Time = num3;
			@new.MatchValuesTo(val);
		}
		else
		{
			@new.Time = num3;
			@new.Lerp(prev, val, (num3 - prev.Time) / (val.Time - prev.Time));
		}
		result.prev = @new;
		T new2 = t.GetNew();
		if (val2.Time - prev2.Time <= Mathf.Epsilon)
		{
			new2.Time = num2;
			new2.MatchValuesTo(val2);
		}
		else
		{
			new2.Time = num2;
			new2.Lerp(prev2, val2, (num2 - prev2.Time) / (val2.Time - prev2.Time));
		}
		result.next = new2;
		if (new2.Time - @new.Time <= Mathf.Epsilon)
		{
			result.prev = new2;
			result.tick = new2;
			return result;
		}
		if (num - new2.Time > extrapolation)
		{
			result.prev = new2;
			result.tick = new2;
			return result;
		}
		T new3 = t.GetNew();
		new3.Time = num;
		new3.Lerp(@new, new2, Mathf.Min(num - @new.Time, new2.Time + extrapolation - @new.Time) / (new2.Time - @new.Time));
		result.tick = new3;
		return result;
	}
}
