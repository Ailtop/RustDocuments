using System;
using System.Collections;
using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class Repeater2 : Operation
	{
		[SerializeField]
		private ReorderableFloatArray _timesToTrigger = new ReorderableFloatArray(default(float));

		[SerializeField]
		[Subcomponent]
		private Subcomponents _toRepeat;

		private void Awake()
		{
			Array.Sort(_timesToTrigger.values);
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
					_toRepeat.Run(projectile);
				}
				yield return null;
				time += projectile.owner.chronometer.projectile.deltaTime;
			}
		}

		public override void Run(Projectile projectile)
		{
			StartCoroutine(CRun(projectile));
		}
	}
}
