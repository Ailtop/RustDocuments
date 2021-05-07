using System.Collections;
using Characters;
using UnityEngine;

namespace Runnables
{
	public sealed class CharacterTranslateTo : CRunnable
	{
		[SerializeField]
		private Target _target;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Curve curve;

		public override IEnumerator CRun()
		{
			Character character = _target.character;
			Vector3 start = character.transform.position;
			Vector3 end = _destination.position;
			for (float elapsed = 0f; elapsed < curve.duration; elapsed += Chronometer.global.deltaTime)
			{
				yield return null;
				character.transform.position = Vector2.Lerp(start, end, curve.Evaluate(elapsed));
			}
			character.transform.position = end;
		}
	}
}
