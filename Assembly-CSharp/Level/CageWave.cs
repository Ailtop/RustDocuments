using UnityEngine;

namespace Level
{
	public class CageWave : Wave
	{
		[SerializeField]
		private Wave _formerWave;

		[SerializeField]
		private Cage _cage;

		public override void Initialize()
		{
			base.state = State.Spawned;
			_cage.onDestroyed += Clear;
			if (_formerWave == null)
			{
				_cage.Activate();
				return;
			}
			_formerWave.onClear += _cage.Activate;
			_cage.Deactivate();
		}

		private void Clear()
		{
			base.state = State.Cleared;
			_onClear?.Invoke();
		}
	}
}
