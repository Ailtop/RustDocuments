using System.Collections;
using Characters.AI;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class TakeAimContinuously : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private float _duration;

		private float _originalDirection;

		private bool _stop;

		private void Awake()
		{
			_originalDirection = 0f;
		}

		public override void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			Character character = Singleton<Service>.Instance.levelManager.player;
			if (_controller != null)
			{
				character = _controller.target;
			}
			Transform targetTransform = character.transform;
			float targetHalfHeight = character.collider.bounds.extents.y;
			float elapsed = 0f;
			_stop = false;
			while (!_stop && elapsed < _duration)
			{
				yield return null;
				Vector3 vector = new Vector3(targetTransform.position.x, targetTransform.position.y + targetHalfHeight) - _centerAxisPosition.transform.position;
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				_centerAxisPosition.rotation = Quaternion.Euler(0f, 0f, _originalDirection + num);
				elapsed += owner.chronometer.master.deltaTime;
			}
		}

		public override void Stop()
		{
			_stop = true;
		}
	}
}
