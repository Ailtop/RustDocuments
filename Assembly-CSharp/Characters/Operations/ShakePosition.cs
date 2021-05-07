using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class ShakePosition : CharacterOperation
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private float _power = 0.2f;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private float _interval = 0.1f;

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			float elapsed = 0f;
			float intervalElapsed = 0f;
			Vector3 shakeVector = Vector3.zero;
			while (elapsed <= _curve.duration)
			{
				float deltaTime = owner.chronometer.master.deltaTime;
				elapsed += deltaTime;
				intervalElapsed -= deltaTime;
				shakeVector -= shakeVector * 60f * deltaTime;
				if (intervalElapsed <= 0f)
				{
					intervalElapsed = _interval;
					float num = 1f - _curve.Evaluate(elapsed);
					shakeVector.x = Random.Range(0f - _power, _power) * num;
					shakeVector.y = Random.Range(0f - _power, _power) * num;
				}
				_target.localPosition = shakeVector;
				yield return null;
			}
			_target.localPosition = Vector3.zero;
		}
	}
}
