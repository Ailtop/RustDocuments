namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UIGridRenderer")]
public class UIGridRenderer : UILineRenderer
{
	[SerializeField]
	private int m_GridColumns = 10;

	[SerializeField]
	private int m_GridRows = 10;

	public int GridColumns
	{
		get
		{
			return m_GridColumns;
		}
		set
		{
			if (m_GridColumns != value)
			{
				m_GridColumns = value;
				SetAllDirty();
			}
		}
	}

	public int GridRows
	{
		get
		{
			return m_GridRows;
		}
		set
		{
			if (m_GridRows != value)
			{
				m_GridRows = value;
				SetAllDirty();
			}
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		relativeSize = true;
		int num = GridRows * 3 + 1;
		if (GridRows % 2 == 0)
		{
			num++;
		}
		num += GridColumns * 3 + 1;
		m_points = new Vector2[num];
		int num2 = 0;
		for (int i = 0; i < GridRows; i++)
		{
			float x = 1f;
			float x2 = 0f;
			if (i % 2 == 0)
			{
				x = 0f;
				x2 = 1f;
			}
			float y = (float)i / (float)GridRows;
			m_points[num2].x = x;
			m_points[num2].y = y;
			num2++;
			m_points[num2].x = x2;
			m_points[num2].y = y;
			num2++;
			m_points[num2].x = x2;
			m_points[num2].y = (float)(i + 1) / (float)GridRows;
			num2++;
		}
		if (GridRows % 2 == 0)
		{
			m_points[num2].x = 1f;
			m_points[num2].y = 1f;
			num2++;
		}
		m_points[num2].x = 0f;
		m_points[num2].y = 1f;
		num2++;
		for (int j = 0; j < GridColumns; j++)
		{
			float y2 = 1f;
			float y3 = 0f;
			if (j % 2 == 0)
			{
				y2 = 0f;
				y3 = 1f;
			}
			float x3 = (float)j / (float)GridColumns;
			m_points[num2].x = x3;
			m_points[num2].y = y2;
			num2++;
			m_points[num2].x = x3;
			m_points[num2].y = y3;
			num2++;
			m_points[num2].x = (float)(j + 1) / (float)GridColumns;
			m_points[num2].y = y3;
			num2++;
		}
		if (GridColumns % 2 == 0)
		{
			m_points[num2].x = 1f;
			m_points[num2].y = 1f;
		}
		else
		{
			m_points[num2].x = 1f;
			m_points[num2].y = 0f;
		}
		base.OnPopulateMesh(vh);
	}
}
