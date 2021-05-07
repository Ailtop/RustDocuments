using UnityEngine;

namespace Runnables
{
	public sealed class CharacterSetPositionTo : Runnable
	{
		[SerializeField]
		private Target _target;

		[SerializeField]
		private Transform _point;

		public override void Run()
		{
			_target.character.transform.position = _point.transform.position;
		}
	}
}
