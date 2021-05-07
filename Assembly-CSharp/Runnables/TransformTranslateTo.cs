using System.Collections;
using UnityEngine;

namespace Runnables
{
	public sealed class TransformTranslateTo : CRunnable
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Curve curve;

		public override IEnumerator CRun()
		{
			Vector3 start = _target.transform.position;
			Vector3 end = _destination.position;
			for (float elapsed = 0f; elapsed < curve.duration; elapsed += Chronometer.global.deltaTime)
			{
				yield return null;
				_target.transform.position = Vector2.Lerp(start, end, curve.Evaluate(elapsed));
			}
			_target.transform.position = end;
		}
	}
}
