using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Ring")]
public class UIRing : UIPrimitiveBase
{
	public float innerRadius = 16f;

	public float outerRadius = 32f;

	[Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
	[Range(0f, 1000f)]
	public int ArcSteps = 100;

	private List<int> indices = new List<int>();

	private List<UIVertex> vertices = new List<UIVertex>();

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		float num = innerRadius * 2f;
		float num2 = outerRadius * 2f;
		vh.Clear();
		indices.Clear();
		vertices.Clear();
		int item = 0;
		int num3 = 1;
		float num4 = 360f / (float)ArcSteps;
		float num5 = Mathf.Cos(0f);
		float num6 = Mathf.Sin(0f);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		simpleVert.position = new Vector2(num2 * num5, num2 * num6);
		vertices.Add(simpleVert);
		Vector2 vector = new Vector2(num * num5, num * num6);
		simpleVert.position = vector;
		vertices.Add(simpleVert);
		for (int i = 1; i <= ArcSteps; i++)
		{
			float f = (float)Math.PI / 180f * ((float)i * num4);
			num5 = Mathf.Cos(f);
			num6 = Mathf.Sin(f);
			simpleVert.color = color;
			simpleVert.position = new Vector2(num2 * num5, num2 * num6);
			vertices.Add(simpleVert);
			simpleVert.position = new Vector2(num * num5, num * num6);
			vertices.Add(simpleVert);
			int item2 = num3;
			indices.Add(item);
			indices.Add(num3 + 1);
			indices.Add(num3);
			num3++;
			item = num3;
			num3++;
			indices.Add(item);
			indices.Add(num3);
			indices.Add(item2);
		}
		vh.AddUIVertexStream(vertices, indices);
	}

	public void SetArcSteps(int steps)
	{
		ArcSteps = steps;
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
}
