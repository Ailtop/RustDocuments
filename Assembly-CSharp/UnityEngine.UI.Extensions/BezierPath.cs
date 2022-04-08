using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

public class BezierPath
{
	public int SegmentsPerCurve = 10;

	public float MINIMUM_SQR_DISTANCE = 0.01f;

	public float DIVISION_THRESHOLD = -0.99f;

	private List<Vector2> controlPoints;

	private int curveCount;

	public BezierPath()
	{
		controlPoints = new List<Vector2>();
	}

	public void SetControlPoints(List<Vector2> newControlPoints)
	{
		controlPoints.Clear();
		controlPoints.AddRange(newControlPoints);
		curveCount = (controlPoints.Count - 1) / 3;
	}

	public void SetControlPoints(Vector2[] newControlPoints)
	{
		controlPoints.Clear();
		controlPoints.AddRange(newControlPoints);
		curveCount = (controlPoints.Count - 1) / 3;
	}

	public List<Vector2> GetControlPoints()
	{
		return controlPoints;
	}

	public void Interpolate(List<Vector2> segmentPoints, float scale)
	{
		controlPoints.Clear();
		if (segmentPoints.Count < 2)
		{
			return;
		}
		for (int i = 0; i < segmentPoints.Count; i++)
		{
			if (i == 0)
			{
				Vector2 vector = segmentPoints[i];
				Vector2 vector2 = segmentPoints[i + 1] - vector;
				Vector2 item = vector + scale * vector2;
				controlPoints.Add(vector);
				controlPoints.Add(item);
			}
			else if (i == segmentPoints.Count - 1)
			{
				Vector2 vector3 = segmentPoints[i - 1];
				Vector2 vector4 = segmentPoints[i];
				Vector2 vector5 = vector4 - vector3;
				Vector2 item2 = vector4 - scale * vector5;
				controlPoints.Add(item2);
				controlPoints.Add(vector4);
			}
			else
			{
				Vector2 vector6 = segmentPoints[i - 1];
				Vector2 vector7 = segmentPoints[i];
				Vector2 vector8 = segmentPoints[i + 1];
				Vector2 normalized = (vector8 - vector6).normalized;
				Vector2 item3 = vector7 - scale * normalized * (vector7 - vector6).magnitude;
				Vector2 item4 = vector7 + scale * normalized * (vector8 - vector7).magnitude;
				controlPoints.Add(item3);
				controlPoints.Add(vector7);
				controlPoints.Add(item4);
			}
		}
		curveCount = (controlPoints.Count - 1) / 3;
	}

	public void SamplePoints(List<Vector2> sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
	{
		if (sourcePoints.Count < 2)
		{
			return;
		}
		Stack<Vector2> stack = new Stack<Vector2>();
		stack.Push(sourcePoints[0]);
		Vector2 vector = sourcePoints[1];
		int num = 2;
		for (num = 2; num < sourcePoints.Count; num++)
		{
			if ((vector - sourcePoints[num]).sqrMagnitude > minSqrDistance && (stack.Peek() - sourcePoints[num]).sqrMagnitude > maxSqrDistance)
			{
				stack.Push(vector);
			}
			vector = sourcePoints[num];
		}
		Vector2 vector2 = stack.Pop();
		Vector2 vector3 = stack.Peek();
		Vector2 normalized = (vector3 - vector).normalized;
		float magnitude = (vector - vector2).magnitude;
		float magnitude2 = (vector2 - vector3).magnitude;
		vector2 += normalized * ((magnitude2 - magnitude) / 2f);
		stack.Push(vector2);
		stack.Push(vector);
		Interpolate(new List<Vector2>(stack), scale);
	}

	public Vector2 CalculateBezierPoint(int curveIndex, float t)
	{
		int num = curveIndex * 3;
		Vector2 p = controlPoints[num];
		Vector2 p2 = controlPoints[num + 1];
		Vector2 p3 = controlPoints[num + 2];
		Vector2 p4 = controlPoints[num + 3];
		return CalculateBezierPoint(t, p, p2, p3, p4);
	}

	public List<Vector2> GetDrawingPoints0()
	{
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < curveCount; i++)
		{
			if (i == 0)
			{
				list.Add(CalculateBezierPoint(i, 0f));
			}
			for (int j = 1; j <= SegmentsPerCurve; j++)
			{
				float t = (float)j / (float)SegmentsPerCurve;
				list.Add(CalculateBezierPoint(i, t));
			}
		}
		return list;
	}

	public List<Vector2> GetDrawingPoints1()
	{
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < controlPoints.Count - 3; i += 3)
		{
			Vector2 p = controlPoints[i];
			Vector2 p2 = controlPoints[i + 1];
			Vector2 p3 = controlPoints[i + 2];
			Vector2 p4 = controlPoints[i + 3];
			if (i == 0)
			{
				list.Add(CalculateBezierPoint(0f, p, p2, p3, p4));
			}
			for (int j = 1; j <= SegmentsPerCurve; j++)
			{
				float t = (float)j / (float)SegmentsPerCurve;
				list.Add(CalculateBezierPoint(t, p, p2, p3, p4));
			}
		}
		return list;
	}

	public List<Vector2> GetDrawingPoints2()
	{
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < curveCount; i++)
		{
			List<Vector2> list2 = FindDrawingPoints(i);
			if (i != 0)
			{
				list2.RemoveAt(0);
			}
			list.AddRange(list2);
		}
		return list;
	}

	private List<Vector2> FindDrawingPoints(int curveIndex)
	{
		List<Vector2> list = new List<Vector2>();
		Vector2 item = CalculateBezierPoint(curveIndex, 0f);
		Vector2 item2 = CalculateBezierPoint(curveIndex, 1f);
		list.Add(item);
		list.Add(item2);
		FindDrawingPoints(curveIndex, 0f, 1f, list, 1);
		return list;
	}

	private int FindDrawingPoints(int curveIndex, float t0, float t1, List<Vector2> pointList, int insertionIndex)
	{
		Vector2 vector = CalculateBezierPoint(curveIndex, t0);
		Vector2 vector2 = CalculateBezierPoint(curveIndex, t1);
		if ((vector - vector2).sqrMagnitude < MINIMUM_SQR_DISTANCE)
		{
			return 0;
		}
		float num = (t0 + t1) / 2f;
		Vector2 vector3 = CalculateBezierPoint(curveIndex, num);
		Vector2 normalized = (vector - vector3).normalized;
		Vector2 normalized2 = (vector2 - vector3).normalized;
		if (Vector2.Dot(normalized, normalized2) > DIVISION_THRESHOLD || Mathf.Abs(num - 0.5f) < 0.0001f)
		{
			int num2 = 0;
			num2 += FindDrawingPoints(curveIndex, t0, num, pointList, insertionIndex);
			pointList.Insert(insertionIndex + num2, vector3);
			num2++;
			return num2 + FindDrawingPoints(curveIndex, num, t1, pointList, insertionIndex + num2);
		}
		return 0;
	}

	private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float num = 1f - t;
		float num2 = t * t;
		float num3 = num * num;
		float num4 = num3 * num;
		float num5 = num2 * t;
		return num4 * p0 + 3f * num3 * t * p1 + 3f * num * num2 * p2 + num5 * p3;
	}
}
