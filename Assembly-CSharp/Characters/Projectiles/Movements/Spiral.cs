using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Spiral : Movement
	{
		private enum RotateMethod
		{
			Constant,
			Lerp,
			Slerp
		}

		[Serializable]
		private class MoveInfo
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<MoveInfo>
			{
			}

			[SerializeField]
			private AnimationCurve _curve;

			[SerializeField]
			private float _length;

			[SerializeField]
			private float _targetSpeed;

			[SerializeField]
			private bool _clearHitHistory;

			public AnimationCurve curve => _curve;

			public float length => _length;

			public float targetSpeed => _targetSpeed;

			public bool clearHitHistory => _clearHitHistory;
		}

		[Serializable]
		private class RotationInfo
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<RotationInfo>
			{
			}

			[SerializeField]
			private float _length;

			[SerializeField]
			private float _rotateSpeed;

			[SerializeField]
			private float _angle;

			[SerializeField]
			private RotateMethod _rotateMethod;

			public float length => _length;

			public float rotateSpeed => _rotateSpeed;

			public float angle => _angle;

			public RotateMethod rotateMethod => _rotateMethod;
		}

		[SerializeField]
		private float _delay;

		[SerializeField]
		private float _startSpeed;

		[SerializeField]
		private MoveInfo.Reorderable _moveInfos;

		[SerializeField]
		private RotationInfo.Reorderable _rotationInfos;

		private int _currentMoveIndex;

		private Quaternion _rotation;

		public override void Initialize(Projectile projectile, float direction)
		{
			base.Initialize(projectile, direction);
			_rotation = Quaternion.Euler(0f, 0f, direction);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			if (time >= _delay)
			{
				float num = time;
				for (int i = 0; i < _rotationInfos.values.Length; i++)
				{
					RotationInfo rotationInfo = _rotationInfos.values[i];
					if (num > rotationInfo.length)
					{
						num -= rotationInfo.length;
					}
					else
					{
						UpdateDirection(deltaTime, rotationInfo);
					}
				}
			}
			float num2 = _startSpeed;
			for (int j = 0; j < _moveInfos.values.Length; j++)
			{
				MoveInfo moveInfo = _moveInfos.values[j];
				if (time > moveInfo.length)
				{
					num2 = moveInfo.targetSpeed;
					time -= moveInfo.length;
					continue;
				}
				if (moveInfo.clearHitHistory && _currentMoveIndex != j)
				{
					base.projectile.ClearHitHistroy();
				}
				_currentMoveIndex = j;
				float num3 = num2 + (moveInfo.targetSpeed - num2) * moveInfo.curve.Evaluate(time / moveInfo.length);
				return new ValueTuple<Vector2, float>(base.directionVector, num3 * base.projectile.speedMultiplier);
			}
			return new ValueTuple<Vector2, float>(base.directionVector, num2 * base.projectile.speedMultiplier);
		}

		private void UpdateDirection(float deltaTime, RotationInfo info)
		{
			float angle = base.direction + info.angle;
			switch (info.rotateMethod)
			{
			case RotateMethod.Constant:
				_rotation = Quaternion.RotateTowards(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), info.rotateSpeed * 100f * deltaTime);
				break;
			case RotateMethod.Lerp:
				_rotation = Quaternion.Lerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), info.rotateSpeed * deltaTime);
				break;
			case RotateMethod.Slerp:
				_rotation = Quaternion.Slerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), info.rotateSpeed * deltaTime);
				break;
			}
			base.direction = _rotation.eulerAngles.z;
			base.directionVector = _rotation * Vector3.right;
		}
	}
}
