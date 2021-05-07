using System.Collections;
using Characters;
using Characters.Actions;
using UnityEngine;

namespace Level.Traps
{
	public class FlameFlower : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private GameObject _horizontalBody;

		[SerializeField]
		private GameObject _verticalBody;

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
			yield return Chronometer.global.WaitForSeconds(_startTiming * _interval);
			while (true)
			{
				_attackAction.TryStart();
				yield return Chronometer.global.WaitForSeconds(_interval);
			}
		}
	}
}
