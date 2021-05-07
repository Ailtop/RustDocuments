using UnityEngine;

namespace Runnables
{
	public sealed class ClearStatus : Runnable
	{
		[SerializeField]
		private Target _target;

		public override void Run()
		{
			_target.character.status.RemoveAllStatus();
		}
	}
}
