using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Circle Simple")]
public class UICircleSimple : UIPrimitiveBase
{
	[Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
	[Range(0f, 1000f)]
	public int ArcSteps = 100;

	public bool Fill = true;

	public float Thickness = 5f;

	public bool ThicknessIsOutside;

	private List<int> indices = new List<int>();

	private List<UIVertex> vertices = new List<UIVertex>();

	private Vector2 uvCenter = new Vector2(0.5f, 0.5f);

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		float num = ((base.rectTransform.rect.width < base.rectTransform.rect.height) ? base.rectTransform.rect.width : base.rectTransform.rect.height);
		float num2 = (ThicknessIsOutside ? ((0f - base.rectTransform.pivot.x) * num - Thickness) : ((0f - base.rectTransform.pivot.x) * num));
		float num3 = (ThicknessIsOutside ? ((0f - base.rectTransform.pivot.x) * num) : ((0f - base.rectTransform.pivot.x) * num + Thickness));
		vh.Clear();
		indices.Clear();
		vertices.Clear();
		int item = 0;
		int num4 = 1;
		int num5 = 0;
		float num6 = 360f / (float)ArcSteps;
		float num7 = Mathf.Cos(0f);
		float num8 = Mathf.Sin(0f);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		simpleVert.position = new Vector2(num2 * num7, num2 * num8);
		simpleVert.uv0 = new Vector2(simpleVert.position.x / num + 0.5f, simpleVert.position.y / num + 0.5f);
		vertices.Add(simpleVert);
		Vector2 vector = new Vector2(num3 * num7, num3 * num8);
		if (Fill)
		{
			vector = Vector2.zero;
		}
		simpleVert.position = vector;
		simpleVert.uv0 = (Fill ? uvCenter : new Vector2(simpleVert.position.x / num + 0.5f, simpleVert.position.y / num + 0.5f));
		vertices.Add(simpleVert);
		for (int i = 1; i <= ArcSteps; i++)
		{
			float f = (float)Math.PI / 180f * ((float)i * num6);
			num7 = Mathf.Cos(f);
			num8 = Mathf.Sin(f);
			simpleVert.color = color;
			simpleVert.position = new Vector2(num2 * num7, num2 * num8);
			simpleVert.uv0 = new Vector2(simpleVert.position.x / num + 0.5f, simpleVert.position.y / num + 0.5f);
			vertices.Add(simpleVert);
			if (!Fill)
			{
				simpleVert.position = new Vector2(num3 * num7, num3 * num8);
				simpleVert.uv0 = new Vector2(simpleVert.position.x / num + 0.5f, simpleVert.position.y / num + 0.5f);
				vertices.Add(simpleVert);
				num5 = num4;
				indices.Add(item);
				indices.Add(num4 + 1);
				indices.Add(num4);
				num4++;
				item = num4;
				num4++;
				indices.Add(item);
				indices.Add(num4);
				indices.Add(num5);
			}
			else
			{
				indices.Add(item);
				indices.Add(num4 + 1);
				indices.Add(1);
				num4++;
				item = num4;
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

	public void SetArcSteps(int steps)
	{
		ArcSteps = steps;
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

	public void SetThickness(int thickness)
	{
		Thickness = thickness;
		SetVerticesDirty();
	}
}
