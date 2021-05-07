using UnityEngine;

namespace Level.MapEvent.Condition
{
	public class PropDestroyed : Condition
	{
		[SerializeField]
		private Prop _prop;

		private void Awake()
		{
			_prop.onDestroy += base.Run;
		}
	}
}
