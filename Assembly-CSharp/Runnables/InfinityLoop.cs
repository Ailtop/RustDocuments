using System.Collections;
using UnityEngine;

namespace Runnables
{
	public class InfinityLoop : Runnable
	{
		[SerializeField]
		private float _interval;

		[SerializeField]
		[Subcomponent]
		private Runnable _runnable;

		public override void Run()
		{
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			while (true)
			{
				_runnable.Run();
				yield return Chronometer.global.WaitForSeconds(_interval);
			}
		}
	}
}
