using UnityEngine;

namespace Characters.Operations
{
	public class JustActivate : CharacterOperation
	{
		[SerializeField]
		private GameObject _gameObject;

		public override void Run(Character owner)
		{
			_gameObject.SetActive(true);
		}
	}
}
