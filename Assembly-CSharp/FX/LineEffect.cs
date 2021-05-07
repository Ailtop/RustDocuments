using System.Collections;
using UnityEngine;

namespace FX
{
	public class LineEffect : MonoBehaviour
	{
		[SerializeField]
		private Transform _body;

		[SerializeField]
		private float _duration = 0.1f;

		private Vector2 _startPoint;

		private Vector2 _endPoint;

		public Vector2 startPoint
		{
			get
			{
				return _startPoint;
			}
			set
			{
				_startPoint = value;
			}
		}

		public Vector2 endPoint
		{
			get
			{
				return _endPoint;
			}
			set
			{
				_endPoint = value;
			}
		}

		public void Run()
		{
			_body.gameObject.SetActive(false);
			float y = Vector2.Distance(_startPoint, _endPoint);
			_body.localScale = new Vector2(1f, y);
			Vector3 vector = _endPoint - _startPoint;
			float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f - 90f;
			_body.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			base.transform.position = new Vector2(_startPoint.x, _startPoint.y + 1f);
			_body.gameObject.SetActive(true);
			StartCoroutine(CDeactive());
		}

		public void Hide()
		{
			_body.gameObject.SetActive(false);
		}

		private IEnumerator CDeactive()
		{
			yield return Chronometer.global.WaitForSeconds(_duration);
			_body.gameObject.SetActive(false);
		}
	}
}
