using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class ActivateGameObjectOperation : CharacterOperation
	{
		[SerializeField]
		private GameObject _gameObject;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _deactivate;

		[SerializeField]
		private Transform _activatedPosition;

		private CoroutineReference _stopCoroutineReference;

		public override void Run(Character owner)
		{
			if (_deactivate)
			{
				_gameObject.SetActive(false);
				return;
			}
			if (_activatedPosition != null)
			{
				_gameObject.transform.position = _activatedPosition.position;
			}
			_gameObject.SetActive(true);
			RuntimeAnimatorController component = _gameObject.GetComponent<RuntimeAnimatorController>();
			if (component != null && _duration == 0f)
			{
				_duration = component.animationClips[0].length;
			}
			if (_duration > 0f)
			{
				_stopCoroutineReference = this.StartCoroutineWithReference(CStop(owner.chronometer.animation));
			}
		}

		private IEnumerator CStop(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_duration);
			Stop();
		}

		public override void Stop()
		{
			_stopCoroutineReference.Stop();
			if (_gameObject != null)
			{
				_gameObject.SetActive(false);
			}
		}
	}
}
