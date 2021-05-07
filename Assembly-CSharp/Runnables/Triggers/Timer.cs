using System.Collections;
using UnityEngine;

namespace Runnables.Triggers
{
	public class Timer : Trigger
	{
		[SerializeField]
		private float _time;

		private bool _running;

		protected override bool Check()
		{
			if (_running)
			{
				return false;
			}
			StartCoroutine(CRun());
			return true;
		}

		private IEnumerator CRun()
		{
			_running = true;
			yield return Chronometer.global.WaitForSeconds(_time);
			_running = false;
		}
	}
}
