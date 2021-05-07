using System.Collections;
using UnityEngine;

namespace Level
{
	public class ReactiveProp : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		protected Animator _animator;

		[SerializeField]
		private float _angle;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private Curve _curve;

		private Vector2 _direction;

		protected bool _flying;

		private Vector3 _originPosition;

		private Vector3 _destination;

		private void Awake()
		{
			_originPosition = base.transform.localPosition;
		}

		public void ResetPosition()
		{
			base.transform.localPosition = _originPosition;
		}

		protected void Activate()
		{
			_flying = true;
			_direction = Quaternion.AngleAxis(_angle, Vector3.forward) * Vector3.right;
			if (_direction.x < 0f)
			{
				base.transform.localScale.Set(-1f, 1f, 1f);
			}
			_destination = _originPosition + (Vector3)_direction * _distance;
			StartCoroutine(CFlyAway());
		}

		private IEnumerator CReadyToFly()
		{
			_animator.Play("Ready");
			yield return Chronometer.global.WaitForSeconds(0.4f);
		}

		private IEnumerator CFlyAway()
		{
			float t = 0f;
			_animator.Play("Fly");
			for (; t < _curve.duration; t += Chronometer.global.deltaTime)
			{
				float t2 = _curve.Evaluate(t);
				base.transform.localPosition = Vector3.Lerp(_originPosition, _destination, t2);
				yield return null;
			}
			base.gameObject.SetActive(false);
		}
	}
}
