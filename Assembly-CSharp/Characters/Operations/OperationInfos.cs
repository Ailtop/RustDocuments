using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations
{
	public class OperationInfos : MonoBehaviour
	{
		[SerializeField]
		private float _duration;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operationsOnEnd;

		private bool _running;

		private Character _owner;

		public float duration => _duration;

		public event Action onEnd;

		private void OnDisable()
		{
			if (_running)
			{
				_operations.StopAll();
				_running = false;
				this.onEnd?.Invoke();
			}
		}

		public void Initialize()
		{
			_operations.Initialize();
			_operationsOnEnd.Initialize();
		}

		public void Run(Character owner)
		{
			_owner = owner;
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			_running = true;
			int operationIndex = 0;
			float time = 0f;
			OperationInfo[] components = _operations.components;
			while ((_duration == 0f && operationIndex < components.Length) || (_duration > 0f && time < _duration))
			{
				for (time += Chronometer.global.deltaTime; operationIndex < components.Length && time >= components[operationIndex].timeToTrigger; operationIndex++)
				{
					components[operationIndex].operation.Run(_owner);
				}
				yield return null;
				if (_owner == null || !_owner.gameObject.activeSelf)
				{
					break;
				}
			}
			_operations.StopAll();
			_running = false;
			this.onEnd?.Invoke();
			base.gameObject.SetActive(false);
		}

		public void Stop()
		{
			_operations.StopAll();
			if (_owner != null)
			{
				_operationsOnEnd.Run(_owner);
			}
			_running = false;
			this.onEnd?.Invoke();
			_owner = null;
			base.gameObject.SetActive(false);
		}
	}
}
