using UnityEngine;

public class TickHistory
{
	private Deque<Vector3> points = new Deque<Vector3>();

	public int Count => points.Count;

	public void Reset()
	{
		points.Clear();
	}

	public void Reset(Vector3 point)
	{
		Reset();
		AddPoint(point);
	}

	public float Distance(Vector3 point, Bounds bounds)
	{
		if (points.Count == 0)
		{
			return 0f;
		}
		if (points.Count == 1)
		{
			Vector3 a = points[0];
			return new Bounds(a + bounds.center, bounds.size).SqrDistance(point);
		}
		float num = float.MaxValue;
		for (int i = 1; i < points.Count; i++)
		{
			Vector3 a2 = new Line(points[i - 1], points[i]).ClosestPoint(point);
			num = Mathf.Min(num, new Bounds(a2 + bounds.center, bounds.size).SqrDistance(point));
		}
		return Mathf.Sqrt(num);
	}

	public void AddPoint(Vector3 point, int limit = -1)
	{
		while (limit > 0 && points.Count >= limit)
		{
			points.PopFront();
		}
		points.PushBack(point);
	}

	public void TransformEntries(Matrix4x4 matrix)
	{
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point = points[i];
			point = matrix.MultiplyPoint3x4(point);
			points[i] = point;
		}
	}
}
