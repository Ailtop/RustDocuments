using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class Repeater3 : CharacterOperation
	{
		[SerializeField]
		private ReorderableFloatArray _timesToTrigger = new ReorderableFloatArray(default(float));

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private CoroutineReference[] _repeatCoroutineReferences;

		private void Awake()
		{
			Array.Sort(_timesToTrigger.values);
			_repeatCoroutineReferences = new CoroutineReference[_timesToTrigger.values.Length];
		}

		public override void Initialize()
		{
			_operations.Initialize();
		}

		internal IEnumerator CRun(Character owner, Character target)
		{
			int operationIndex = 0;
			float time = 0f;
			float[] timesToTrigger = _timesToTrigger.values;
			while (operationIndex < timesToTrigger.Length)
			{
				for (; operationIndex < timesToTrigger.Length && time >= timesToTrigger[operationIndex]; operationIndex++)
				{
					StartCoroutine(_operations.CRun(owner, target));
				}
				yield return null;
				time += owner.chronometer.animation.deltaTime * runSpeed;
			}
		}

		public override void Run(Character owner)
		{
			Run(owner, owner);
		}

		public override void Run(Character owner, Character target)
		{
			StartCoroutine(CRun(owner, target));
		}

		public override void Stop()
		{
			_operations.StopAll();
			StopAllCoroutines();
		}
	}
}
