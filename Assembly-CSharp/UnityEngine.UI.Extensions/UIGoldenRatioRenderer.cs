using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

public class UIGoldenRatioRenderer : UILineRenderer
{
	private enum Orientations
	{
		Left = 0,
		Top = 1,
		Right = 2,
		Bottom = 3
	}

	private readonly List<Vector2> _points = new List<Vector2>();

	private readonly List<Rect> _rects = new List<Rect>();

	private int canvasWidth;

	private int canvasHeight;

	public float lineThickness2 = 1f;

	private void DrawSpiral(VertexHelper vh)
	{
		_points.Clear();
		_rects.Clear();
		float num = (1f + Mathf.Sqrt(5f)) / 2f;
		canvasWidth = (int)base.canvas.pixelRect.width;
		canvasHeight = (int)base.canvas.pixelRect.height;
		Orientations orientation;
		float num2;
		float num3;
		if (canvasWidth > canvasHeight)
		{
			orientation = Orientations.Left;
			if ((float)canvasWidth / (float)canvasHeight > num)
			{
				num2 = canvasHeight;
				num3 = num2 * num;
			}
			else
			{
				num3 = canvasWidth;
				num2 = num3 / num;
			}
		}
		else
		{
			orientation = Orientations.Top;
			if ((float)canvasHeight / (float)canvasWidth > num)
			{
				num3 = canvasWidth;
				num2 = num3 * num;
			}
			else
			{
				num2 = canvasHeight;
				num3 = num2 / num;
			}
		}
		float num4 = -canvasWidth / 2;
		float num5 = canvasHeight / 2;
		num4 += ((float)canvasWidth - num3) / 2f;
		num5 += ((float)canvasHeight - num2) / 2f;
		List<Vector2> list = new List<Vector2>();
		DrawPhiRectangles(vh, list, num4, num5, num3, num2, orientation);
		if (list.Count > 1)
		{
			Vector2 vector = list[0];
			Vector2 vector2 = list[list.Count - 1];
			float num6 = vector.x - vector2.x;
			float num7 = vector.y - vector2.y;
			float num8 = Mathf.Sqrt(num6 * num6 + num7 * num7);
			float num9 = Mathf.Atan2(num7, num6);
			float num10 = (float)Math.PI / 50f;
			float num11 = 1f - 1f / num / 25f * 0.78f;
			while (num8 > 32f)
			{
				Vector2 item = new Vector2(vector2.x + num8 * Mathf.Cos(num9), (float)canvasHeight - (vector2.y + num8 * Mathf.Sin(num9)));
				_points.Add(item);
				num9 += num10;
				num8 *= num11;
			}
		}
	}

	private void DrawPhiRectangles(VertexHelper vh, List<Vector2> points, float x, float y, float width, float height, Orientations orientation)
	{
		if (!(width < 1f) && !(height < 1f))
		{
			if (width >= 10f && height >= 10f)
			{
				_rects.Add(new Rect(x, y, width, height));
			}
			switch (orientation)
			{
			case Orientations.Left:
				points.Add(new Vector2(x, y + height));
				x += height;
				width -= height;
				orientation = Orientations.Top;
				break;
			case Orientations.Top:
				points.Add(new Vector2(x, y));
				y += width;
				height -= width;
				orientation = Orientations.Right;
				break;
			case Orientations.Right:
				points.Add(new Vector2(x + width, y));
				width -= height;
				orientation = Orientations.Bottom;
				break;
			case Orientations.Bottom:
				points.Add(new Vector2(x + width, y + height));
				height -= width;
				orientation = Orientations.Left;
				break;
			}
			DrawPhiRectangles(vh, points, x, y, width, height, orientation);
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (base.canvas == null)
		{
			return;
		}
		relativeSize = false;
		DrawSpiral(vh);
		m_points = _points.ToArray();
		base.OnPopulateMesh(vh);
		foreach (Rect rect in _rects)
		{
			DrawRect(vh, new Rect(rect.x, rect.y - lineThickness2 * 0.5f, rect.width, lineThickness2));
			DrawRect(vh, new Rect(rect.x - lineThickness2 * 0.5f, rect.y, lineThickness2, rect.height));
			DrawRect(vh, new Rect(rect.x, rect.y + rect.height - lineThickness2 * 0.5f, rect.width, lineThickness2));
			DrawRect(vh, new Rect(rect.x + rect.width - lineThickness2 * 0.5f, rect.y, lineThickness2, rect.height));
		}
	}

	private void DrawRect(VertexHelper vh, Rect rect)
	{
		Vector2[] array = new Vector2[4]
		{
			new Vector2(rect.x, rect.y),
			new Vector2(rect.x + rect.width, rect.y),
			new Vector2(rect.x + rect.width, rect.y + rect.height),
			new Vector2(rect.x, rect.y + rect.height)
		};
		UIVertex[] array2 = new UIVertex[4];
		for (int i = 0; i < array2.Length; i++)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = color;
			simpleVert.position = array[i].WithY(base.canvas.pixelRect.height - array[i].y);
			array2[i] = simpleVert;
		}
		vh.AddUIVertexQuad(array2);
	}
}
