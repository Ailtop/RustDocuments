using Level;
using UnityEngine;

namespace Runnables.Triggers
{
	public class CageOnDestroyed : Trigger
	{
		[SerializeField]
		private Cage _cage;

		private bool _destroyed;

		private void Start()
		{
			_cage.onDestroyed += delegate
			{
				_destroyed = true;
			};
		}

		protected override bool Check()
		{
			return _destroyed;
		}
	}
}
