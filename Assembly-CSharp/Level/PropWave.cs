using UnityEngine;

namespace Level
{
	public class PropWave : Wave
	{
		[SerializeField]
		private Wave _formerWave;

		[SerializeField]
		private Prop _prop;

		public override void Initialize()
		{
			base.state = State.Spawned;
			_prop.onDestroy += Clear;
		}

		private void Clear()
		{
			base.state = State.Cleared;
			_onClear?.Invoke();
		}
	}
}
