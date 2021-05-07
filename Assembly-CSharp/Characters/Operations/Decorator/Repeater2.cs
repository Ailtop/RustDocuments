using System;
using System.Collections;
using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class Repeater2 : CharacterOperation
	{
		[SerializeField]
		private ReorderableFloatArray _timesToTrigger = new ReorderableFloatArray(default(float));

		[SerializeField]
		[Subcomponent]
		private Subcomponents _toRepeat;

		private CoroutineReference _repeatCoroutineReference;

		private void Awake()
		{
			Array.Sort(_timesToTrigger.values);
		}

		public override void Initialize()
		{
			_toRepeat.Initialize();
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
					_toRepeat.Run(owner, target);
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
			_repeatCoroutineReference = this.StartCoroutineWithReference(CRun(owner, target));
		}

		public override void Stop()
		{
			_toRepeat.Stop();
			_repeatCoroutineReference.Stop();
		}
	}
}
