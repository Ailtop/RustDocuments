using System.Collections;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class WaitForTransfom : Sequence
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private float _epsilon = 1f;

		[SerializeField]
		private int _maxTime = 5;

		public override IEnumerator CRun()
		{
			for (float elapsed = 0f; elapsed <= (float)_maxTime; elapsed += Chronometer.global.deltaTime)
			{
				yield return null;
				if (Vector2.Distance(_target.position, _destination.position) < _epsilon)
				{
					break;
				}
			}
		}
	}
}
