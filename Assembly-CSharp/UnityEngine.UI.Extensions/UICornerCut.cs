namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/Cut Corners")]
public class UICornerCut : UIPrimitiveBase
{
	public Vector2 cornerSize = new Vector2(16f, 16f);

	[Header("Corners to cut")]
	[SerializeField]
	private bool m_cutUL = true;

	[SerializeField]
	private bool m_cutUR;

	[SerializeField]
	private bool m_cutLL;

	[SerializeField]
	private bool m_cutLR;

	[Tooltip("Up-Down colors become Left-Right colors")]
	[SerializeField]
	private bool m_makeColumns;

	[Header("Color the cut bars differently")]
	[SerializeField]
	private bool m_useColorUp;

	[SerializeField]
	private Color32 m_colorUp;

	[SerializeField]
	private bool m_useColorDown;

	[SerializeField]
	private Color32 m_colorDown;

	public bool CutUL
	{
		get
		{
			return m_cutUL;
		}
		set
		{
			m_cutUL = value;
			SetAllDirty();
		}
	}

	public bool CutUR
	{
		get
		{
			return m_cutUR;
		}
		set
		{
			m_cutUR = value;
			SetAllDirty();
		}
	}

	public bool CutLL
	{
		get
		{
			return m_cutLL;
		}
		set
		{
			m_cutLL = value;
			SetAllDirty();
		}
	}

	public bool CutLR
	{
		get
		{
			return m_cutLR;
		}
		set
		{
			m_cutLR = value;
			SetAllDirty();
		}
	}

	public bool MakeColumns
	{
		get
		{
			return m_makeColumns;
		}
		set
		{
			m_makeColumns = value;
			SetAllDirty();
		}
	}

	public bool UseColorUp
	{
		get
		{
			return m_useColorUp;
		}
		set
		{
			m_useColorUp = value;
		}
	}

	public Color32 ColorUp
	{
		get
		{
			return m_colorUp;
		}
		set
		{
			m_colorUp = value;
		}
	}

	public bool UseColorDown
	{
		get
		{
			return m_useColorDown;
		}
		set
		{
			m_useColorDown = value;
		}
	}

	public Color32 ColorDown
	{
		get
		{
			return m_colorDown;
		}
		set
		{
			m_colorDown = value;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Rect rect = base.rectTransform.rect;
		Rect rect2 = rect;
		Color32 color = this.color;
		bool flag = m_cutUL | m_cutUR;
		bool flag2 = m_cutLL | m_cutLR;
		bool flag3 = m_cutLL | m_cutUL;
		bool flag4 = m_cutLR | m_cutUR;
		if (!(flag || flag2) || !(cornerSize.sqrMagnitude > 0f))
		{
			return;
		}
		vh.Clear();
		if (flag3)
		{
			rect2.xMin += cornerSize.x;
		}
		if (flag2)
		{
			rect2.yMin += cornerSize.y;
		}
		if (flag)
		{
			rect2.yMax -= cornerSize.y;
		}
		if (flag4)
		{
			rect2.xMax -= cornerSize.x;
		}
		Vector2 vector;
		Vector2 vector2;
		Vector2 vector3;
		Vector2 vector4;
		if (m_makeColumns)
		{
			vector = new Vector2(rect.xMin, m_cutUL ? rect2.yMax : rect.yMax);
			vector2 = new Vector2(rect.xMax, m_cutUR ? rect2.yMax : rect.yMax);
			vector3 = new Vector2(rect.xMin, m_cutLL ? rect2.yMin : rect.yMin);
			vector4 = new Vector2(rect.xMax, m_cutLR ? rect2.yMin : rect.yMin);
			if (flag3)
			{
				AddSquare(vector3, vector, new Vector2(rect2.xMin, rect.yMax), new Vector2(rect2.xMin, rect.yMin), rect, m_useColorUp ? m_colorUp : color, vh);
			}
			if (flag4)
			{
				AddSquare(vector2, vector4, new Vector2(rect2.xMax, rect.yMin), new Vector2(rect2.xMax, rect.yMax), rect, m_useColorDown ? m_colorDown : color, vh);
			}
		}
		else
		{
			vector = new Vector2(m_cutUL ? rect2.xMin : rect.xMin, rect.yMax);
			vector2 = new Vector2(m_cutUR ? rect2.xMax : rect.xMax, rect.yMax);
			vector3 = new Vector2(m_cutLL ? rect2.xMin : rect.xMin, rect.yMin);
			vector4 = new Vector2(m_cutLR ? rect2.xMax : rect.xMax, rect.yMin);
			if (flag2)
			{
				AddSquare(vector4, vector3, new Vector2(rect.xMin, rect2.yMin), new Vector2(rect.xMax, rect2.yMin), rect, m_useColorDown ? m_colorDown : color, vh);
			}
			if (flag)
			{
				AddSquare(vector, vector2, new Vector2(rect.xMax, rect2.yMax), new Vector2(rect.xMin, rect2.yMax), rect, m_useColorUp ? m_colorUp : color, vh);
			}
		}
		if (m_makeColumns)
		{
			AddSquare(new Rect(rect2.xMin, rect.yMin, rect2.width, rect.height), rect, color, vh);
		}
		else
		{
			AddSquare(new Rect(rect.xMin, rect2.yMin, rect.width, rect2.height), rect, color, vh);
		}
	}

	private static void AddSquare(Rect rect, Rect rectUV, Color32 color32, VertexHelper vh)
	{
		int num = AddVert(rect.xMin, rect.yMin, rectUV, color32, vh);
		int idx = AddVert(rect.xMin, rect.yMax, rectUV, color32, vh);
		int num2 = AddVert(rect.xMax, rect.yMax, rectUV, color32, vh);
		int idx2 = AddVert(rect.xMax, rect.yMin, rectUV, color32, vh);
		vh.AddTriangle(num, idx, num2);
		vh.AddTriangle(num2, idx2, num);
	}

	private static void AddSquare(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Rect rectUV, Color32 color32, VertexHelper vh)
	{
		int num = AddVert(a.x, a.y, rectUV, color32, vh);
		int idx = AddVert(b.x, b.y, rectUV, color32, vh);
		int num2 = AddVert(c.x, c.y, rectUV, color32, vh);
		int idx2 = AddVert(d.x, d.y, rectUV, color32, vh);
		vh.AddTriangle(num, idx, num2);
		vh.AddTriangle(num2, idx2, num);
	}

	private static int AddVert(float x, float y, Rect area, Color32 color32, VertexHelper vh)
	{
		vh.AddVert(uv0: new Vector2(Mathf.InverseLerp(area.xMin, area.xMax, x), Mathf.InverseLerp(area.yMin, area.yMax, y)), position: new Vector3(x, y), color: color32);
		return vh.currentVertCount - 1;
	}
}
