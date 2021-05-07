using System.Collections;
using UnityEngine;

namespace Runnables.Customs
{
	public class RepeatInvoker : MonoBehaviour
	{
		[SerializeField]
		[MinMaxSlider(0f, 60f)]
		private Vector2 _interval;

		[SerializeField]
		private Runnable _execute;

		private void OnEnable()
		{
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			while (true)
			{
				yield return Chronometer.global.WaitForSeconds(Random.Range(_interval.x, _interval.y));
				_execute.Run();
			}
		}
	}
}
