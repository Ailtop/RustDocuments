using System.Collections;
using UnityEngine;

namespace Runnables
{
	public class WaitForTime : CRunnable
	{
		[SerializeField]
		private float _time;

		public override IEnumerator CRun()
		{
			yield return Chronometer.global.WaitForSeconds(_time);
		}
	}
}
