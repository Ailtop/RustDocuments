using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldSplineData
{
	public Vector3[] inputPoints;

	public Vector3[] inputTangents;

	public float inputLUTInterval;

	public List<float> LUTDistanceKeys;

	public List<Vector3> LUTPosValues;

	public float Length;

	[SerializeField]
	private int maxPointsIndex;

	public Vector3 GetStartPoint()
	{
		return inputPoints[0];
	}

	public Vector3 GetEndPoint()
	{
		return inputPoints[maxPointsIndex];
	}

	public Vector3 GetStartTangent()
	{
		return inputTangents[0];
	}

	public Vector3 GetEndTangent()
	{
		return inputTangents[maxPointsIndex];
	}

	public Vector3 GetPointCubicHermite(float distance)
	{
		if (distance < 0f)
		{
			return GetStartPoint();
		}
		if (distance > Length)
		{
			return GetEndPoint();
		}
		Vector3 a = GetStartPoint();
		float a2 = 0f;
		for (int i = 0; i < LUTDistanceKeys.Count; i++)
		{
			float num = LUTDistanceKeys[i];
			if (num > distance)
			{
				float t = Mathf.InverseLerp(a2, num, distance);
				return Vector3.Lerp(a, LUTPosValues[i], t);
			}
			a2 = num;
			a = LUTPosValues[i];
		}
		return GetEndPoint();
	}

	public virtual Vector3 GetTangent(float distance)
	{
		float num = distance / Length * (float)(inputTangents.Length - 1);
		int num2 = (int)num;
		if (num <= 0f)
		{
			return GetStartTangent();
		}
		if (num >= (float)maxPointsIndex)
		{
			return GetEndTangent();
		}
		Vector3 a = inputTangents[num2];
		Vector3 b = inputTangents[num2 + 1];
		float t = num - (float)num2;
		return Vector3.Slerp(a, b, t);
	}

	public void SetDefaultTangents(WorldSpline worldSpline)
	{
		PathInterpolator pathInterpolator = new PathInterpolator(worldSpline.points, worldSpline.tangents);
		pathInterpolator.RecalculateTangents();
		worldSpline.tangents = pathInterpolator.Tangents;
	}

	private void CreateLookupTable(WorldSpline worldSpline)
	{
		PathInterpolator pathInterpolator = new PathInterpolator(worldSpline.points, worldSpline.tangents);
		Vector3 b = pathInterpolator.GetPointCubicHermite(0f);
		Length = 0f;
		AddEntry(0f, GetStartPoint());
		Vector3 pointCubicHermite;
		for (float num = worldSpline.lutInterval; num < pathInterpolator.Length; num += worldSpline.lutInterval)
		{
			pointCubicHermite = pathInterpolator.GetPointCubicHermite(num);
			Length += Vector3.Distance(pointCubicHermite, b);
			AddEntry(Length, pathInterpolator.GetPointCubicHermite(num));
			b = pointCubicHermite;
		}
		pointCubicHermite = GetEndPoint();
		Length += Vector3.Distance(pointCubicHermite, b);
		AddEntry(Length, pointCubicHermite);
	}

	private void AddEntry(float distance, Vector3 pos)
	{
		if (!LUTDistanceKeys.Contains(distance))
		{
			LUTDistanceKeys.Add(distance);
			LUTPosValues.Add(pos);
		}
	}
}
