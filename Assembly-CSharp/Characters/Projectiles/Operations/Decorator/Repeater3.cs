using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class Repeater3 : Operation
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

		internal IEnumerator CRun(Projectile projectile)
		{
			int operationIndex = 0;
			float time = 0f;
			float[] timesToTrigger = _timesToTrigger.values;
			while (operationIndex < timesToTrigger.Length)
			{
				for (; operationIndex < timesToTrigger.Length && time >= timesToTrigger[operationIndex]; operationIndex++)
				{
					StartCoroutine(_operations.CRun(projectile));
				}
				yield return null;
				time += projectile.owner.chronometer.projectile.deltaTime;
			}
		}

		public override void Run(Projectile projectile)
		{
			Run(projectile);
		}
	}
}
