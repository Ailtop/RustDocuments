using UnityEngine;

namespace Runnables
{
	public sealed class DestroyObject : Runnable
	{
		[SerializeField]
		private GameObject _object;

		public override void Run()
		{
			Object.Destroy(_object);
		}
	}
}
