using System.Collections.Generic;
using UnityEngine;

namespace Rust.Interpolation;

public class Interpolator<T> where T : Interpolator<T>.ISnapshot, new()
{
	public struct Segment
	{
		public T tick;

		public T prev;

		public T next;
	}

	public interface ISnapshot
	{
		float Time { get; set; }

		void MatchValuesTo(T entry);

		void Lerp(T prev, T next, float delta);
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

	public Segment Query(float time, float interpolation, float extrapolation, float smoothing)
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
		T prev3 = new T();
		if (val.Time - prev.Time <= Mathf.Epsilon)
		{
			prev3.Time = num3;
			prev3.MatchValuesTo(val);
		}
		else
		{
			float delta = (num3 - prev.Time) / (val.Time - prev.Time);
			prev3.Time = num3;
			prev3.Lerp(prev, val, delta);
		}
		result.prev = prev3;
		T val3 = new T();
		if (val2.Time - prev2.Time <= Mathf.Epsilon)
		{
			val3.Time = num2;
			val3.MatchValuesTo(val2);
		}
		else
		{
			float delta2 = (num2 - prev2.Time) / (val2.Time - prev2.Time);
			val3.Time = num2;
			val3.Lerp(prev2, val2, delta2);
		}
		result.next = val3;
		if (val3.Time - prev3.Time <= Mathf.Epsilon)
		{
			result.prev = val3;
			result.tick = val3;
			return result;
		}
		if (num - val3.Time > extrapolation)
		{
			result.prev = val3;
			result.tick = val3;
			return result;
		}
		T tick = new T();
		float delta3 = Mathf.Min(num - prev3.Time, val3.Time + extrapolation - prev3.Time) / (val3.Time - prev3.Time);
		tick.Time = num;
		tick.Lerp(prev3, val3, delta3);
		result.tick = tick;
		return result;
	}
}
