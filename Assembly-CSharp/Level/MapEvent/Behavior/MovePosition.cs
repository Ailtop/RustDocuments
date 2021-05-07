using System.Collections;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class MovePosition : Behavior
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Vector3 _offset;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private float _duration;

		public override void Run()
		{
			if (_duration <= 0f)
			{
				_target.localPosition += _offset;
			}
			else
			{
				StartCoroutine(CRun());
			}
		}

		private IEnumerator CRun()
		{
			Vector3 start = _target.localPosition;
			float elapsed = 0f;
			while (elapsed < _duration)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				_target.localPosition = start + _offset * _curve.Evaluate(elapsed / _duration);
			}
			_target.localPosition = start + _offset;
		}
	}
}
