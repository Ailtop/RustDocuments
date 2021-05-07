using System.Collections;
using Characters;
using Characters.Actions;
using UnityEngine;

namespace Level.Traps
{
	public class LionStatue : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Range(0f, 1f)]
		private float _startTiming;

		[SerializeField]
		private float _interval = 4f;

		[SerializeField]
		private Action _attackAction;

		private void Awake()
		{
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			float elapsed = (0f - _startTiming) * _interval;
			while (true)
			{
				elapsed += Chronometer.global.deltaTime;
				if (elapsed >= _interval)
				{
					_attackAction.TryStart();
					elapsed -= _interval;
				}
				yield return null;
			}
		}
	}
}
