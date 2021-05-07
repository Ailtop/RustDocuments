using UnityEngine;

namespace Runnables
{
	public class PrintDebugLog : Runnable
	{
		[SerializeField]
		private string _log;

		public override void Run()
		{
			Debug.Log(_log);
		}
	}
}
