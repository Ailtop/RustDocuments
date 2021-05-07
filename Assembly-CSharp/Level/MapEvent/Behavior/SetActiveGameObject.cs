using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class SetActiveGameObject : Behavior
	{
		[SerializeField]
		private GameObject _gameObject;

		[SerializeField]
		private bool _active = true;

		public override void Run()
		{
			_gameObject.SetActive(_active);
		}
	}
}
