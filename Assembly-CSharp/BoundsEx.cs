using UnityEngine;

public static class BoundsEx
{
	private static Vector3[] pts = new Vector3[8];

	public static Bounds XZ3D(this Bounds bounds)
	{
		return new Bounds(bounds.center.XZ3D(), bounds.size.XZ3D());
	}

	public static Bounds Transform(this Bounds bounds, Matrix4x4 matrix)
	{
		Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
		Vector3 extents = bounds.extents;
		Vector3 vector = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
		Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
		Vector3 vector3 = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
		extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
		extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
		extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
		Bounds result = default(Bounds);
		result.center = center;
		result.extents = extents;
		return result;
	}

	public static Rect ToScreenRect(this Bounds b, Camera cam)
	{
		using (TimeWarning.New("Bounds.ToScreenRect"))
		{
			pts[0] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
			pts[1] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
			pts[2] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
			pts[3] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
			pts[4] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
			pts[5] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
			pts[6] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
			pts[7] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
			Vector3 lhs = pts[0];
			Vector3 lhs2 = pts[0];
			for (int i = 1; i < pts.Length; i++)
			{
				lhs = Vector3.Min(lhs, pts[i]);
				lhs2 = Vector3.Max(lhs2, pts[i]);
			}
			return Rect.MinMaxRect(lhs.x, lhs.y, lhs2.x, lhs2.y);
		}
	}

	public static Rect ToCanvasRect(this Bounds b, RectTransform target, Camera cam)
	{
		Rect result = ToScreenRect(b, cam);
		result.min = result.min.ToCanvas(target);
		result.max = result.max.ToCanvas(target);
		return result;
	}

	public static float InnerDistToEdge2D(this Bounds b, Vector3 point)
	{
		float num = Mathf.Abs(point.x - b.min.x);
		float num2 = Mathf.Abs(point.x - b.max.x);
		float num3 = Mathf.Abs(point.z - b.min.z);
		float num4 = Mathf.Abs(point.z - b.max.z);
		return Mathf.Min(num, num2, num3, num4);
	}

	public static float MaxExtent(this Bounds b)
	{
		return Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
	}
}
