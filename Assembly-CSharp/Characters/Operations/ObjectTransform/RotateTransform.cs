using System.Collections;
using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class RotateTransform : CharacterOperation
	{
		private enum Direction
		{
			Left,
			Right,
			Random
		}

		[SerializeField]
		private Direction _directipnType = Direction.Right;

		[SerializeField]
		private Transform _transform;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _speed;

		public override void Run(Character owner)
		{
			StartCoroutine(CRotate(owner));
		}

		private int GetDirectionSign()
		{
			switch (_directipnType)
			{
			case Direction.Left:
				return -1;
			case Direction.Right:
				return 1;
			case Direction.Random:
				if (!MMMaths.RandomBool())
				{
					return -1;
				}
				return 1;
			default:
				return 1;
			}
		}

		private IEnumerator CRotate(Character owner)
		{
			float elpased = 0f;
			int direction = GetDirectionSign();
			do
			{
				yield return null;
				_transform.Rotate(Vector3.forward * _speed * direction);
				elpased += owner.chronometer.master.deltaTime;
			}
			while (!(elpased >= _duration));
		}
	}
}
