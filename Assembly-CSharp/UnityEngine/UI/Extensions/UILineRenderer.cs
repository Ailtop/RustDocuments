using System;
using System.Collections.Generic;
using UnityEngine.Sprites;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
	[RequireComponent(typeof(RectTransform))]
	public class UILineRenderer : UIPrimitiveBase
	{
		private enum SegmentType
		{
			Start,
			Middle,
			End,
			Full
		}

		public enum JoinType
		{
			Bevel,
			Miter
		}

		public enum BezierType
		{
			None,
			Quick,
			Basic,
			Improved,
			Catenary
		}

		private const float MIN_MITER_JOIN = (float)Math.PI / 12f;

		private const float MIN_BEVEL_NICE_JOIN = (float)Math.PI / 6f;

		private static Vector2 UV_TOP_LEFT;

		private static Vector2 UV_BOTTOM_LEFT;

		private static Vector2 UV_TOP_CENTER_LEFT;

		private static Vector2 UV_TOP_CENTER_RIGHT;

		private static Vector2 UV_BOTTOM_CENTER_LEFT;

		private static Vector2 UV_BOTTOM_CENTER_RIGHT;

		private static Vector2 UV_TOP_RIGHT;

		private static Vector2 UV_BOTTOM_RIGHT;

		private static Vector2[] startUvs;

		private static Vector2[] middleUvs;

		private static Vector2[] endUvs;

		private static Vector2[] fullUvs;

		[SerializeField]
		[Tooltip("Points to draw lines between\n Can be improved using the Resolution Option")]
		internal Vector2[] m_points;

		[SerializeField]
		[Tooltip("Segments to be drawn\n This is a list of arrays of points")]
		internal List<Vector2[]> m_segments;

		[SerializeField]
		[Tooltip("Thickness of the line")]
		internal float lineThickness = 2f;

		[SerializeField]
		[Tooltip("Use the relative bounds of the Rect Transform (0,0 -> 0,1) or screen space coordinates")]
		internal bool relativeSize;

		[SerializeField]
		[Tooltip("Do the points identify a single line or split pairs of lines")]
		internal bool lineList;

		[SerializeField]
		[Tooltip("Add end caps to each line\nMultiple caps when used with Line List")]
		internal bool lineCaps;

		[SerializeField]
		[Tooltip("Resolution of the Bezier curve, different to line Resolution")]
		internal int bezierSegmentsPerCurve = 10;

		[Tooltip("The type of Join used between lines, Square/Mitre or Curved/Bevel")]
		public JoinType LineJoins;

		[Tooltip("Bezier method to apply to line, see docs for options\nCan't be used in conjunction with Resolution as Bezier already changes the resolution")]
		public BezierType BezierMode;

		[HideInInspector]
		public bool drivenExternally;

		public float LineThickness
		{
			get
			{
				return lineThickness;
			}
			set
			{
				lineThickness = value;
				SetAllDirty();
			}
		}

		public bool RelativeSize
		{
			get
			{
				return relativeSize;
			}
			set
			{
				relativeSize = value;
				SetAllDirty();
			}
		}

		public bool LineList
		{
			get
			{
				return lineList;
			}
			set
			{
				lineList = value;
				SetAllDirty();
			}
		}

		public bool LineCaps
		{
			get
			{
				return lineCaps;
			}
			set
			{
				lineCaps = value;
				SetAllDirty();
			}
		}

		public int BezierSegmentsPerCurve
		{
			get
			{
				return bezierSegmentsPerCurve;
			}
			set
			{
				bezierSegmentsPerCurve = value;
			}
		}

		public Vector2[] Points
		{
			get
			{
				return m_points;
			}
			set
			{
				if (m_points != value)
				{
					m_points = value;
					SetAllDirty();
				}
			}
		}

		public List<Vector2[]> Segments
		{
			get
			{
				return m_segments;
			}
			set
			{
				m_segments = value;
				SetAllDirty();
			}
		}

		private void PopulateMesh(VertexHelper vh, Vector2[] pointsToDraw)
		{
			if (BezierMode != 0 && BezierMode != BezierType.Catenary && pointsToDraw.Length > 3)
			{
				BezierPath bezierPath = new BezierPath();
				bezierPath.SetControlPoints(pointsToDraw);
				bezierPath.SegmentsPerCurve = bezierSegmentsPerCurve;
				List<Vector2> list;
				switch (BezierMode)
				{
				case BezierType.Basic:
					list = bezierPath.GetDrawingPoints0();
					break;
				case BezierType.Improved:
					list = bezierPath.GetDrawingPoints1();
					break;
				default:
					list = bezierPath.GetDrawingPoints2();
					break;
				}
				pointsToDraw = list.ToArray();
			}
			if (BezierMode == BezierType.Catenary && pointsToDraw.Length == 2)
			{
				pointsToDraw = new CableCurve(pointsToDraw)
				{
					slack = base.Resoloution,
					steps = BezierSegmentsPerCurve
				}.Points();
			}
			if (base.ImproveResolution != 0)
			{
				pointsToDraw = IncreaseResolution(pointsToDraw);
			}
			float num = ((!relativeSize) ? 1f : base.rectTransform.rect.width);
			float num2 = ((!relativeSize) ? 1f : base.rectTransform.rect.height);
			float num3 = (0f - base.rectTransform.pivot.x) * num;
			float num4 = (0f - base.rectTransform.pivot.y) * num2;
			List<UIVertex[]> list2 = new List<UIVertex[]>();
			if (lineList)
			{
				for (int i = 1; i < pointsToDraw.Length; i += 2)
				{
					Vector2 vector = pointsToDraw[i - 1];
					Vector2 vector2 = pointsToDraw[i];
					vector = new Vector2(vector.x * num + num3, vector.y * num2 + num4);
					vector2 = new Vector2(vector2.x * num + num3, vector2.y * num2 + num4);
					if (lineCaps)
					{
						list2.Add(CreateLineCap(vector, vector2, SegmentType.Start));
					}
					list2.Add(CreateLineSegment(vector, vector2, SegmentType.Middle, (list2.Count > 1) ? list2[list2.Count - 2] : null));
					if (lineCaps)
					{
						list2.Add(CreateLineCap(vector, vector2, SegmentType.End));
					}
				}
			}
			else
			{
				for (int j = 1; j < pointsToDraw.Length; j++)
				{
					Vector2 vector3 = pointsToDraw[j - 1];
					Vector2 vector4 = pointsToDraw[j];
					vector3 = new Vector2(vector3.x * num + num3, vector3.y * num2 + num4);
					vector4 = new Vector2(vector4.x * num + num3, vector4.y * num2 + num4);
					if (lineCaps && j == 1)
					{
						list2.Add(CreateLineCap(vector3, vector4, SegmentType.Start));
					}
					list2.Add(CreateLineSegment(vector3, vector4, SegmentType.Middle));
					if (lineCaps && j == pointsToDraw.Length - 1)
					{
						list2.Add(CreateLineCap(vector3, vector4, SegmentType.End));
					}
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (!lineList && k < list2.Count - 1)
				{
					Vector3 vector5 = list2[k][1].position - list2[k][2].position;
					Vector3 vector6 = list2[k + 1][2].position - list2[k + 1][1].position;
					float num5 = Vector2.Angle(vector5, vector6) * ((float)Math.PI / 180f);
					float num6 = Mathf.Sign(Vector3.Cross(vector5.normalized, vector6.normalized).z);
					float num7 = lineThickness / (2f * Mathf.Tan(num5 / 2f));
					Vector3 position = list2[k][2].position - vector5.normalized * num7 * num6;
					Vector3 position2 = list2[k][3].position + vector5.normalized * num7 * num6;
					JoinType joinType = LineJoins;
					if (joinType == JoinType.Miter)
					{
						if (num7 < vector5.magnitude / 2f && num7 < vector6.magnitude / 2f && num5 > (float)Math.PI / 12f)
						{
							list2[k][2].position = position;
							list2[k][3].position = position2;
							list2[k + 1][0].position = position2;
							list2[k + 1][1].position = position;
						}
						else
						{
							joinType = JoinType.Bevel;
						}
					}
					if (joinType == JoinType.Bevel)
					{
						if (num7 < vector5.magnitude / 2f && num7 < vector6.magnitude / 2f && num5 > (float)Math.PI / 6f)
						{
							if (num6 < 0f)
							{
								list2[k][2].position = position;
								list2[k + 1][1].position = position;
							}
							else
							{
								list2[k][3].position = position2;
								list2[k + 1][0].position = position2;
							}
						}
						UIVertex[] verts = new UIVertex[4]
						{
							list2[k][2],
							list2[k][3],
							list2[k + 1][0],
							list2[k + 1][1]
						};
						vh.AddUIVertexQuad(verts);
					}
				}
				vh.AddUIVertexQuad(list2[k]);
			}
			if (vh.currentVertCount > 64000)
			{
				Debug.LogError("Max Verticies size is 64000, current mesh vertcies count is [" + vh.currentVertCount + "] - Cannot Draw");
				vh.Clear();
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (m_points != null && m_points.Length != 0)
			{
				GeneratedUVs();
				vh.Clear();
				PopulateMesh(vh, m_points);
			}
			else if (m_segments != null && m_segments.Count > 0)
			{
				GeneratedUVs();
				vh.Clear();
				for (int i = 0; i < m_segments.Count; i++)
				{
					Vector2[] pointsToDraw = m_segments[i];
					PopulateMesh(vh, pointsToDraw);
				}
			}
		}

		private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
		{
			switch (type)
			{
			case SegmentType.Start:
			{
				Vector2 start2 = start - (end - start).normalized * lineThickness / 2f;
				return CreateLineSegment(start2, start, SegmentType.Start);
			}
			case SegmentType.End:
			{
				Vector2 end2 = end + (end - start).normalized * lineThickness / 2f;
				return CreateLineSegment(end, end2, SegmentType.End);
			}
			default:
				Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
				return null;
			}
		}

		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, UIVertex[] previousVert = null)
		{
			Vector2 vector = new Vector2(start.y - end.y, end.x - start.x).normalized * lineThickness / 2f;
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			if (previousVert != null)
			{
				zero = new Vector2(previousVert[3].position.x, previousVert[3].position.y);
				zero2 = new Vector2(previousVert[2].position.x, previousVert[2].position.y);
			}
			else
			{
				zero = start - vector;
				zero2 = start + vector;
			}
			Vector2 vector2 = end + vector;
			Vector2 vector3 = end - vector;
			switch (type)
			{
			case SegmentType.Start:
				return SetVbo(new Vector2[4] { zero, zero2, vector2, vector3 }, startUvs);
			case SegmentType.End:
				return SetVbo(new Vector2[4] { zero, zero2, vector2, vector3 }, endUvs);
			case SegmentType.Full:
				return SetVbo(new Vector2[4] { zero, zero2, vector2, vector3 }, fullUvs);
			default:
				return SetVbo(new Vector2[4] { zero, zero2, vector2, vector3 }, middleUvs);
			}
		}

		protected override void GeneratedUVs()
		{
			if (base.activeSprite != null)
			{
				Vector4 outerUV = DataUtility.GetOuterUV(base.activeSprite);
				Vector4 innerUV = DataUtility.GetInnerUV(base.activeSprite);
				UV_TOP_LEFT = new Vector2(outerUV.x, outerUV.y);
				UV_BOTTOM_LEFT = new Vector2(outerUV.x, outerUV.w);
				UV_TOP_CENTER_LEFT = new Vector2(innerUV.x, innerUV.y);
				UV_TOP_CENTER_RIGHT = new Vector2(innerUV.z, innerUV.y);
				UV_BOTTOM_CENTER_LEFT = new Vector2(innerUV.x, innerUV.w);
				UV_BOTTOM_CENTER_RIGHT = new Vector2(innerUV.z, innerUV.w);
				UV_TOP_RIGHT = new Vector2(outerUV.z, outerUV.y);
				UV_BOTTOM_RIGHT = new Vector2(outerUV.z, outerUV.w);
			}
			else
			{
				UV_TOP_LEFT = Vector2.zero;
				UV_BOTTOM_LEFT = new Vector2(0f, 1f);
				UV_TOP_CENTER_LEFT = new Vector2(0.5f, 0f);
				UV_TOP_CENTER_RIGHT = new Vector2(0.5f, 0f);
				UV_BOTTOM_CENTER_LEFT = new Vector2(0.5f, 1f);
				UV_BOTTOM_CENTER_RIGHT = new Vector2(0.5f, 1f);
				UV_TOP_RIGHT = new Vector2(1f, 0f);
				UV_BOTTOM_RIGHT = Vector2.one;
			}
			startUvs = new Vector2[4] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER_LEFT, UV_TOP_CENTER_LEFT };
			middleUvs = new Vector2[4] { UV_TOP_CENTER_LEFT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_CENTER_RIGHT };
			endUvs = new Vector2[4] { UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_RIGHT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
			fullUvs = new Vector2[4] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
		}

		protected override void ResolutionToNativeSize(float distance)
		{
			if (base.UseNativeSize)
			{
				m_Resolution = distance / (base.activeSprite.rect.width / base.pixelsPerUnit);
				lineThickness = base.activeSprite.rect.height / base.pixelsPerUnit;
			}
		}

		private int GetSegmentPointCount()
		{
			List<Vector2[]> segments = Segments;
			if (segments != null && segments.Count > 0)
			{
				int num = 0;
				{
					foreach (Vector2[] segment in Segments)
					{
						num += segment.Length;
					}
					return num;
				}
			}
			return Points.Length;
		}

		public Vector2 GetPosition(int index, int segmentIndex = 0)
		{
			if (segmentIndex > 0)
			{
				return Segments[segmentIndex - 1][index - 1];
			}
			if (Segments.Count > 0)
			{
				int num = 0;
				int num2 = index;
				foreach (Vector2[] segment in Segments)
				{
					if (num2 - segment.Length > 0)
					{
						num2 -= segment.Length;
						num++;
						continue;
					}
					break;
				}
				return Segments[num][num2 - 1];
			}
			return Points[index - 1];
		}

		public Vector2 GetPositionBySegment(int index, int segment)
		{
			return Segments[segment][index - 1];
		}

		public Vector2 GetClosestPoint(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			Vector2 lhs = p3 - p1;
			Vector2 vector = p2 - p1;
			float num = Mathf.Clamp01(Vector2.Dot(lhs, vector.normalized) / vector.magnitude);
			return p1 + vector * num;
		}
	}
}
