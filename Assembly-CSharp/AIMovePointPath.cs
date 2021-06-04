using System.Collections.Generic;
using UnityEngine;

public class AIMovePointPath : MonoBehaviour
{
	public enum Mode
	{
		Loop,
		Reverse
	}

	public enum PathDirection
	{
		Forwards,
		Backwards
	}

	public Color DebugPathColor = Color.green;

	public Mode LoopMode;

	public List<AIMovePoint> Points = new List<AIMovePoint>();

	public void Clear()
	{
		Points.Clear();
	}

	public void AddPoint(AIMovePoint point)
	{
		Points.Add(point);
	}

	public AIMovePoint FindNearestPoint(Vector3 position)
	{
		float num = float.MaxValue;
		AIMovePoint result = null;
		foreach (AIMovePoint point in Points)
		{
			float num2 = Vector3.SqrMagnitude(position - point.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = point;
			}
		}
		return result;
	}

	public AIMovePoint GetNextPoint(AIMovePoint current, ref PathDirection pathDirection)
	{
		int num = 0;
		foreach (AIMovePoint point in Points)
		{
			if (point == current)
			{
				return GetNextPoint(num, ref pathDirection);
			}
			num++;
		}
		return null;
	}

	private AIMovePoint GetNextPoint(int currentPointIndex, ref PathDirection pathDirection)
	{
		int num = currentPointIndex + ((pathDirection == PathDirection.Forwards) ? 1 : (-1));
		if (num < 0)
		{
			if (LoopMode == Mode.Loop)
			{
				num = Points.Count - 1;
			}
			else
			{
				num = 1;
				pathDirection = PathDirection.Forwards;
			}
		}
		else if (num >= Points.Count)
		{
			if (LoopMode == Mode.Loop)
			{
				num = 0;
			}
			else
			{
				num = Points.Count - 2;
				pathDirection = PathDirection.Backwards;
			}
		}
		return Points[num];
	}

	private void OnDrawGizmos()
	{
		Color color = Gizmos.color;
		Gizmos.color = DebugPathColor;
		int num = -1;
		foreach (AIMovePoint point in Points)
		{
			num++;
			if (!(point == null))
			{
				if (num + 1 < Points.Count)
				{
					Gizmos.DrawLine(point.transform.position, Points[num + 1].transform.position);
				}
				else if (LoopMode == Mode.Loop)
				{
					Gizmos.DrawLine(point.transform.position, Points[0].transform.position);
				}
			}
		}
		Gizmos.color = color;
	}

	private void OnDrawGizmosSelected()
	{
		if (Points == null)
		{
			return;
		}
		foreach (AIMovePoint point in Points)
		{
			point.DrawLookAtPoints();
		}
	}
}
