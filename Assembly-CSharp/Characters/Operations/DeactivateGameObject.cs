using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class DeactivateGameObject : CharacterOperation
	{
		[SerializeField]
		private GameObject _gameObject;

		[SerializeField]
		private float _duration;

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner.chronometer.master));
		}

		private IEnumerator CRun(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_duration);
			_gameObject.SetActive(false);
		}
	}
}
