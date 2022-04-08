using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Circle")]
public class UICircle : UIPrimitiveBase
{
	[Tooltip("The Arc Invert property will invert the construction of the Arc.")]
	public bool ArcInvert = true;

	[Tooltip("The Arc property is a percentage of the entire circumference of the circle.")]
	[Range(0f, 1f)]
	public float Arc = 1f;

	[Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
	[Range(0f, 1000f)]
	public int ArcSteps = 100;

	[Tooltip("The Arc Rotation property permits adjusting the geometry orientation around the Z axis.")]
	[Range(0f, 360f)]
	public int ArcRotation;

	[Tooltip("The Progress property allows the primitive to be used as a progression indicator.")]
	[Range(0f, 1f)]
	public float Progress;

	private float _progress;

	public Color ProgressColor = new Color(255f, 255f, 255f, 255f);

	public bool Fill = true;

	public float Thickness = 5f;

	public int Padding;

	private List<int> indices = new List<int>();

	private List<UIVertex> vertices = new List<UIVertex>();

	private Vector2 uvCenter = new Vector2(0.5f, 0.5f);

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		int num = ((!ArcInvert) ? 1 : (-1));
		float num2 = ((base.rectTransform.rect.width < base.rectTransform.rect.height) ? base.rectTransform.rect.width : base.rectTransform.rect.height) - (float)Padding;
		float num3 = (0f - base.rectTransform.pivot.x) * num2;
		float num4 = (0f - base.rectTransform.pivot.x) * num2 + Thickness;
		vh.Clear();
		indices.Clear();
		vertices.Clear();
		int item = 0;
		int num5 = 1;
		int num6 = 0;
		float num7 = Arc * 360f / (float)ArcSteps;
		_progress = (float)ArcSteps * Progress;
		float f = (float)num * ((float)Math.PI / 180f) * (float)ArcRotation;
		float num8 = Mathf.Cos(f);
		float num9 = Mathf.Sin(f);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = ((_progress > 0f) ? ProgressColor : color);
		simpleVert.position = new Vector2(num3 * num8, num3 * num9);
		simpleVert.uv0 = new Vector2(simpleVert.position.x / num2 + 0.5f, simpleVert.position.y / num2 + 0.5f);
		vertices.Add(simpleVert);
		Vector2 vector = new Vector2(num4 * num8, num4 * num9);
		if (Fill)
		{
			vector = Vector2.zero;
		}
		simpleVert.position = vector;
		simpleVert.uv0 = (Fill ? uvCenter : new Vector2(simpleVert.position.x / num2 + 0.5f, simpleVert.position.y / num2 + 0.5f));
		vertices.Add(simpleVert);
		for (int i = 1; i <= ArcSteps; i++)
		{
			float f2 = (float)num * ((float)Math.PI / 180f) * ((float)i * num7 + (float)ArcRotation);
			num8 = Mathf.Cos(f2);
			num9 = Mathf.Sin(f2);
			simpleVert.color = (((float)i > _progress) ? color : ProgressColor);
			simpleVert.position = new Vector2(num3 * num8, num3 * num9);
			simpleVert.uv0 = new Vector2(simpleVert.position.x / num2 + 0.5f, simpleVert.position.y / num2 + 0.5f);
			vertices.Add(simpleVert);
			if (!Fill)
			{
				simpleVert.position = new Vector2(num4 * num8, num4 * num9);
				simpleVert.uv0 = new Vector2(simpleVert.position.x / num2 + 0.5f, simpleVert.position.y / num2 + 0.5f);
				vertices.Add(simpleVert);
				num6 = num5;
				indices.Add(item);
				indices.Add(num5 + 1);
				indices.Add(num5);
				num5++;
				item = num5;
				num5++;
				indices.Add(item);
				indices.Add(num5);
				indices.Add(num6);
			}
			else
			{
				indices.Add(item);
				indices.Add(num5 + 1);
				if ((float)i > _progress)
				{
					indices.Add(ArcSteps + 2);
				}
				else
				{
					indices.Add(1);
				}
				num5++;
				item = num5;
			}
		}
		if (Fill)
		{
			simpleVert.position = vector;
			simpleVert.color = color;
			simpleVert.uv0 = uvCenter;
			vertices.Add(simpleVert);
		}
		vh.AddUIVertexStream(vertices, indices);
	}

	public void SetProgress(float progress)
	{
		Progress = progress;
		SetVerticesDirty();
	}

	public void SetArcSteps(int steps)
	{
		ArcSteps = steps;
		SetVerticesDirty();
	}

	public void SetInvertArc(bool invert)
	{
		ArcInvert = invert;
		SetVerticesDirty();
	}

	public void SetArcRotation(int rotation)
	{
		ArcRotation = rotation;
		SetVerticesDirty();
	}

	public void SetFill(bool fill)
	{
		Fill = fill;
		SetVerticesDirty();
	}

	public void SetBaseColor(Color color)
	{
		this.color = color;
		SetVerticesDirty();
	}

	public void UpdateBaseAlpha(float value)
	{
		Color color = this.color;
		color.a = value;
		this.color = color;
		SetVerticesDirty();
	}

	public void SetProgressColor(Color color)
	{
		ProgressColor = color;
		SetVerticesDirty();
	}

	public void UpdateProgressAlpha(float value)
	{
		ProgressColor.a = value;
		SetVerticesDirty();
	}

	public void SetPadding(int padding)
	{
		Padding = padding;
		SetVerticesDirty();
	}

	public void SetThickness(int thickness)
	{
		Thickness = thickness;
		SetVerticesDirty();
	}
}
