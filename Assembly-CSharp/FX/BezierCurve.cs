using UnityEngine;

namespace FX
{
	[RequireComponent(typeof(LineRenderer))]
	public class BezierCurve : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer _lineRenderer;

		[SerializeField]
		private Vector2[] _points = new Vector2[4];

		public int count => _points.Length;

		public void SetVector(int index, Vector2 vector)
		{
			_points[index] = vector;
		}

		public void SetStart(Vector2 vector)
		{
			SetVector(0, vector);
		}

		public void SetEnd(Vector2 vector)
		{
			SetVector(count - 1, vector);
		}

		public void UpdateCurve()
		{
			for (int i = 0; i < _lineRenderer.positionCount; i++)
			{
				float time = (float)i / (float)(_lineRenderer.positionCount - 1);
				_lineRenderer.SetPosition(i, MMMaths.BezierCurve(_points, time));
			}
		}
	}
}
