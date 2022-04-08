using System;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Polygon")]
public class UIPolygon : UIPrimitiveBase
{
	public bool fill = true;

	public float thickness = 5f;

	[Range(3f, 360f)]
	public int sides = 3;

	[Range(0f, 360f)]
	public float rotation;

	[Range(0f, 1f)]
	public float[] VerticesDistances = new float[3];

	private float size;

	public void DrawPolygon(int _sides)
	{
		sides = _sides;
		VerticesDistances = new float[_sides + 1];
		for (int i = 0; i < _sides; i++)
		{
			VerticesDistances[i] = 1f;
		}
		rotation = 0f;
		SetAllDirty();
	}

	public void DrawPolygon(int _sides, float[] _VerticesDistances)
	{
		sides = _sides;
		VerticesDistances = _VerticesDistances;
		rotation = 0f;
		SetAllDirty();
	}

	public void DrawPolygon(int _sides, float[] _VerticesDistances, float _rotation)
	{
		sides = _sides;
		VerticesDistances = _VerticesDistances;
		rotation = _rotation;
		SetAllDirty();
	}

	private void Update()
	{
		size = base.rectTransform.rect.width;
		if (base.rectTransform.rect.width > base.rectTransform.rect.height)
		{
			size = base.rectTransform.rect.height;
		}
		else
		{
			size = base.rectTransform.rect.width;
		}
		thickness = Mathf.Clamp(thickness, 0f, size / 2f);
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		Vector2 vector3 = new Vector2(0f, 0f);
		Vector2 vector4 = new Vector2(0f, 1f);
		Vector2 vector5 = new Vector2(1f, 1f);
		Vector2 vector6 = new Vector2(1f, 0f);
		float num = 360f / (float)sides;
		int num2 = sides + 1;
		if (VerticesDistances.Length != num2)
		{
			VerticesDistances = new float[num2];
			for (int i = 0; i < num2 - 1; i++)
			{
				VerticesDistances[i] = 1f;
			}
		}
		VerticesDistances[num2 - 1] = VerticesDistances[0];
		for (int j = 0; j < num2; j++)
		{
			float num3 = (0f - base.rectTransform.pivot.x) * size * VerticesDistances[j];
			float num4 = (0f - base.rectTransform.pivot.x) * size * VerticesDistances[j] + thickness;
			float f = (float)Math.PI / 180f * ((float)j * num + rotation);
			float num5 = Mathf.Cos(f);
			float num6 = Mathf.Sin(f);
			vector3 = new Vector2(0f, 1f);
			vector4 = new Vector2(1f, 1f);
			vector5 = new Vector2(1f, 0f);
			vector6 = new Vector2(0f, 0f);
			Vector2 vector7 = vector;
			Vector2 vector8 = new Vector2(num3 * num5, num3 * num6);
			Vector2 vector9;
			Vector2 vector10;
			if (fill)
			{
				vector9 = Vector2.zero;
				vector10 = Vector2.zero;
			}
			else
			{
				vector9 = new Vector2(num4 * num5, num4 * num6);
				vector10 = vector2;
			}
			vector = vector8;
			vector2 = vector9;
			vh.AddUIVertexQuad(SetVbo(new Vector2[4] { vector7, vector8, vector9, vector10 }, new Vector2[4] { vector3, vector4, vector5, vector6 }));
		}
	}
}
