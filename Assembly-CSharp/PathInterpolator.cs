using System;
using UnityEngine;

public class PathInterpolator
{
	public Vector3[] Points;

	public Vector3[] Tangents;

	private bool initialized;

	public int MinIndex
	{
		get;
		set;
	}

	public int MaxIndex
	{
		get;
		set;
	}

	public float Length
	{
		get;
		private set;
	}

	public float StepSize
	{
		get;
		private set;
	}

	public bool Circular
	{
		get;
		private set;
	}

	public int DefaultMinIndex => 0;

	public int DefaultMaxIndex => Points.Length - 1;

	public float StartOffset => Length * (float)(MinIndex - DefaultMinIndex) / (float)(DefaultMaxIndex - DefaultMinIndex);

	public float EndOffset => Length * (float)(DefaultMaxIndex - MaxIndex) / (float)(DefaultMaxIndex - DefaultMinIndex);

	public PathInterpolator(Vector3[] points)
	{
		if (points.Length < 2)
		{
			throw new ArgumentException("Point list too short.");
		}
		Points = points;
		MinIndex = DefaultMinIndex;
		MaxIndex = DefaultMaxIndex;
		Circular = (Vector3.Distance(points[0], points[points.Length - 1]) < 0.1f);
	}

	public void RecalculateTangents()
	{
		if (Tangents == null || Tangents.Length != Points.Length)
		{
			Tangents = new Vector3[Points.Length];
		}
		for (int i = 0; i < Points.Length; i++)
		{
			int num = i - 1;
			int num2 = i + 1;
			if (num < 0)
			{
				num = (Circular ? (Points.Length - 2) : 0);
			}
			if (num2 > Points.Length - 1)
			{
				num2 = (Circular ? 1 : (Points.Length - 1));
			}
			Vector3 b = Points[num];
			Vector3 a = Points[num2];
			Tangents[i] = (a - b).normalized;
		}
		RecalculateLength();
		initialized = true;
	}

	private void RecalculateLength()
	{
		float num = 0f;
		for (int i = 0; i < Points.Length - 1; i++)
		{
			Vector3 b = Points[i];
			Vector3 a = Points[i + 1];
			num += (a - b).magnitude;
		}
		Length = num;
		StepSize = num / (float)Points.Length;
	}

	public void Resample(float distance)
	{
		if (!initialized)
		{
			throw new Exception("Tangents have not been calculated yet or are outdated.");
		}
		Vector3[] array = new Vector3[Mathf.RoundToInt(Length / distance)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetPointCubicHermite((float)i * distance);
		}
		Points = array;
		initialized = false;
	}

	public void Smoothen(int iterations)
	{
		Smoothen(iterations, Vector3.one);
	}

	public void Smoothen(int iterations, Vector3 multipliers)
	{
		for (int i = 0; i < iterations; i++)
		{
			for (int j = MinIndex + ((!Circular) ? 1 : 0); j <= MaxIndex - 1; j += 2)
			{
				SmoothenIndex(j, multipliers);
			}
			for (int k = MinIndex + (Circular ? 1 : 2); k <= MaxIndex - 1; k += 2)
			{
				SmoothenIndex(k, multipliers);
			}
		}
		initialized = false;
	}

	private void SmoothenIndex(int i, Vector3 multipliers)
	{
		int num = i - 1;
		int num2 = i + 1;
		if (i == 0)
		{
			num = Points.Length - 2;
		}
		Vector3 a = Points[num];
		Vector3 b = Points[i];
		Vector3 b2 = Points[num2];
		Vector3 vector = (a + b + b + b2) * 0.25f;
		if (multipliers != Vector3.one)
		{
			vector.x = Mathf.LerpUnclamped(b.x, vector.x, multipliers.x);
			vector.y = Mathf.LerpUnclamped(b.y, vector.y, multipliers.y);
			vector.z = Mathf.LerpUnclamped(b.z, vector.z, multipliers.z);
		}
		Points[i] = vector;
		if (i == 0)
		{
			Points[Points.Length - 1] = Points[0];
		}
	}

	public Vector3 GetStartPoint()
	{
		return Points[MinIndex];
	}

	public Vector3 GetEndPoint()
	{
		return Points[MaxIndex];
	}

	public Vector3 GetStartTangent()
	{
		if (!initialized)
		{
			throw new Exception("Tangents have not been calculated yet or are outdated.");
		}
		return Tangents[MinIndex];
	}

	public Vector3 GetEndTangent()
	{
		if (!initialized)
		{
			throw new Exception("Tangents have not been calculated yet or are outdated.");
		}
		return Tangents[MaxIndex];
	}

	public Vector3 GetPoint(float distance)
	{
		float num = distance / Length * (float)(Points.Length - 1);
		int num2 = (int)num;
		if (num <= (float)MinIndex)
		{
			return GetStartPoint();
		}
		if (num >= (float)MaxIndex)
		{
			return GetEndPoint();
		}
		Vector3 a = Points[num2];
		Vector3 b = Points[num2 + 1];
		float t = num - (float)num2;
		return Vector3.Lerp(a, b, t);
	}

	public Vector3 GetTangent(float distance)
	{
		if (!initialized)
		{
			throw new Exception("Tangents have not been calculated yet or are outdated.");
		}
		float num = distance / Length * (float)(Tangents.Length - 1);
		int num2 = (int)num;
		if (num <= (float)MinIndex)
		{
			return GetStartTangent();
		}
		if (num >= (float)MaxIndex)
		{
			return GetEndTangent();
		}
		Vector3 a = Tangents[num2];
		Vector3 b = Tangents[num2 + 1];
		float t = num - (float)num2;
		return Vector3.Slerp(a, b, t);
	}

	public Vector3 GetPointCubicHermite(float distance)
	{
		if (!initialized)
		{
			throw new Exception("Tangents have not been calculated yet or are outdated.");
		}
		float num = distance / Length * (float)(Points.Length - 1);
		int num2 = (int)num;
		if (num <= (float)MinIndex)
		{
			return GetStartPoint();
		}
		if (num >= (float)MaxIndex)
		{
			return GetEndPoint();
		}
		Vector3 a = Points[num2];
		Vector3 a2 = Points[num2 + 1];
		Vector3 a3 = Tangents[num2] * StepSize;
		Vector3 a4 = Tangents[num2 + 1] * StepSize;
		float num3 = num - (float)num2;
		float num4 = num3 * num3;
		float num5 = num3 * num4;
		return (2f * num5 - 3f * num4 + 1f) * a + (num5 - 2f * num4 + num3) * a3 + (-2f * num5 + 3f * num4) * a2 + (num5 - num4) * a4;
	}
}
