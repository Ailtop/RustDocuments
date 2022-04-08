using System;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/Diamond Graph")]
public class DiamondGraph : UIPrimitiveBase
{
	[SerializeField]
	private float m_a = 1f;

	[SerializeField]
	private float m_b = 1f;

	[SerializeField]
	private float m_c = 1f;

	[SerializeField]
	private float m_d = 1f;

	public float A
	{
		get
		{
			return m_a;
		}
		set
		{
			m_a = value;
		}
	}

	public float B
	{
		get
		{
			return m_b;
		}
		set
		{
			m_b = value;
		}
	}

	public float C
	{
		get
		{
			return m_c;
		}
		set
		{
			m_c = value;
		}
	}

	public float D
	{
		get
		{
			return m_d;
		}
		set
		{
			m_d = value;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		float num = base.rectTransform.rect.width / 2f;
		m_a = Math.Min(1f, Math.Max(0f, m_a));
		m_b = Math.Min(1f, Math.Max(0f, m_b));
		m_c = Math.Min(1f, Math.Max(0f, m_c));
		m_d = Math.Min(1f, Math.Max(0f, m_d));
		Color32 color = this.color;
		vh.AddVert(new Vector3((0f - num) * m_a, 0f), color, new Vector2(0f, 0f));
		vh.AddVert(new Vector3(0f, num * m_b), color, new Vector2(0f, 1f));
		vh.AddVert(new Vector3(num * m_c, 0f), color, new Vector2(1f, 1f));
		vh.AddVert(new Vector3(0f, (0f - num) * m_d), color, new Vector2(1f, 0f));
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}
