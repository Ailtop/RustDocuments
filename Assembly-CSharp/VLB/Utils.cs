using System;
using Unity.Mathematics;
using UnityEngine;

namespace VLB;

public static class Utils
{
	public enum FloatPackingPrecision
	{
		High = 64,
		Low = 8,
		Undef = 0
	}

	private static FloatPackingPrecision ms_FloatPackingPrecision;

	private const int kFloatPackingHighMinShaderLevel = 35;

	public static string GetPath(Transform current)
	{
		if (current.parent == null)
		{
			return "/" + current.name;
		}
		return GetPath(current.parent) + "/" + current.name;
	}

	public static T NewWithComponent<T>(string name) where T : Component
	{
		return new GameObject(name, typeof(T)).GetComponent<T>();
	}

	public static T GetOrAddComponent<T>(this GameObject self) where T : Component
	{
		T val = self.GetComponent<T>();
		if (val == null)
		{
			val = self.AddComponent<T>();
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
	{
		return GetOrAddComponent<T>(self.gameObject);
	}

	public static bool HasFlag(this Enum mask, Enum flags)
	{
		return ((int)(object)mask & (int)(object)flags) == (int)(object)flags;
	}

	public static Vector2 xy(this Vector3 aVector)
	{
		return new Vector2(aVector.x, aVector.y);
	}

	public static Vector2 xz(this Vector3 aVector)
	{
		return new Vector2(aVector.x, aVector.z);
	}

	public static Vector2 yz(this Vector3 aVector)
	{
		return new Vector2(aVector.y, aVector.z);
	}

	public static Vector2 yx(this Vector3 aVector)
	{
		return new Vector2(aVector.y, aVector.x);
	}

	public static Vector2 zx(this Vector3 aVector)
	{
		return new Vector2(aVector.z, aVector.x);
	}

	public static Vector2 zy(this Vector3 aVector)
	{
		return new Vector2(aVector.z, aVector.y);
	}

	public static float GetVolumeCubic(this Bounds self)
	{
		return self.size.x * self.size.y * self.size.z;
	}

	public static float GetMaxArea2D(this Bounds self)
	{
		return Mathf.Max(Mathf.Max(self.size.x * self.size.y, self.size.y * self.size.z), self.size.x * self.size.z);
	}

	public static Color Opaque(this Color self)
	{
		return new Color(self.r, self.g, self.b, 1f);
	}

	public static void GizmosDrawPlane(Vector3 normal, Vector3 position, Color color, float size = 1f)
	{
		Vector3 vector = Vector3.Cross(normal, (Mathf.Abs(Vector3.Dot(normal, Vector3.forward)) < 0.999f) ? Vector3.forward : Vector3.up).normalized * size;
		Vector3 vector2 = position + vector;
		Vector3 vector3 = position - vector;
		vector = Quaternion.AngleAxis(90f, normal) * vector;
		Vector3 vector4 = position + vector;
		Vector3 vector5 = position - vector;
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = color;
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector4, vector5);
		Gizmos.DrawLine(vector2, vector4);
		Gizmos.DrawLine(vector4, vector3);
		Gizmos.DrawLine(vector3, vector5);
		Gizmos.DrawLine(vector5, vector2);
	}

	public static Plane TranslateCustom(this Plane plane, Vector3 translation)
	{
		plane.distance += Vector3.Dot(translation.normalized, plane.normal) * translation.magnitude;
		return plane;
	}

	public static bool IsValid(this Plane plane)
	{
		return plane.normal.sqrMagnitude > 0.5f;
	}

	public static Matrix4x4 SampleInMatrix(this Gradient self, int floatPackingPrecision)
	{
		Matrix4x4 result = default(Matrix4x4);
		for (int i = 0; i < 16; i++)
		{
			Color color = self.Evaluate(Mathf.Clamp01((float)i / 15f));
			result[i] = PackToFloat(color, floatPackingPrecision);
		}
		return result;
	}

	public static Color[] SampleInArray(this Gradient self, int samplesCount)
	{
		Color[] array = new Color[samplesCount];
		for (int i = 0; i < samplesCount; i++)
		{
			array[i] = self.Evaluate(Mathf.Clamp01((float)i / (float)(samplesCount - 1)));
		}
		return array;
	}

	private static Vector4 Vector4_Floor(Vector4 vec)
	{
		return new Vector4(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z), Mathf.Floor(vec.w));
	}

	public static float4 ToFloat4(this Color color)
	{
		return new float4(color.r, color.g, color.b, color.a);
	}

	public static float PackToFloat(this Color color, int floatPackingPrecision)
	{
		Vector4 vector = Vector4_Floor(color * (floatPackingPrecision - 1));
		return 0f + vector.x * (float)floatPackingPrecision * (float)floatPackingPrecision * (float)floatPackingPrecision + vector.y * (float)floatPackingPrecision * (float)floatPackingPrecision + vector.z * (float)floatPackingPrecision + vector.w;
	}

	public static FloatPackingPrecision GetFloatPackingPrecision()
	{
		if (ms_FloatPackingPrecision == FloatPackingPrecision.Undef)
		{
			ms_FloatPackingPrecision = ((SystemInfo.graphicsShaderLevel >= 35) ? FloatPackingPrecision.High : FloatPackingPrecision.Low);
		}
		return ms_FloatPackingPrecision;
	}

	public static void MarkCurrentSceneDirty()
	{
	}
}
