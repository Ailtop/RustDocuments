using System.Collections;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class TranslateTo : Operation
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Curve _curve;

		private Coroutine _cReference;

		public override void Run()
		{
			if (_cReference != null)
			{
				StopCoroutine(_cReference);
			}
			_cReference = StartCoroutine(CTranslate());
		}

		private IEnumerator CTranslate()
		{
			float elapsed = 0f;
			float duration = _curve.duration;
			Vector3 start = _object.position;
			Vector3 end = _target.position;
			while (elapsed <= duration)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				_object.position = Vector2.Lerp(start, end, _curve.Evaluate(elapsed));
			}
			_object.position = end;
		}

		public override void Stop()
		{
			base.Stop();
			if (_cReference != null)
			{
				StopCoroutine(_cReference);
			}
		}
	}
}
