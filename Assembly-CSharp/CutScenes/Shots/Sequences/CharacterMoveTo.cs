using System.Collections;
using Runnables;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public class CharacterMoveTo : Sequence
	{
		[SerializeField]
		private Target _target;

		[SerializeField]
		private Transform _point;

		[SerializeField]
		private float _epsilon = 0.1f;

		public override IEnumerator CRun()
		{
			while (true)
			{
				float num = _point.position.x - _target.character.transform.position.x;
				if (!(Mathf.Abs(num) < _epsilon))
				{
					Vector2 move = ((num > 0f) ? Vector2.right : Vector2.left);
					_target.character.movement.move = move;
					yield return null;
					continue;
				}
				break;
			}
		}
	}
}
