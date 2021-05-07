using System;
using System.Runtime.CompilerServices;
using Characters.Projectiles.Customs;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class WreckMovement : Movement
	{
		[Serializable]
		private class Info
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<Info>
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

		[SerializeField]
		private TerrainCollisionDetector _terrainCollisionDetector;

		[SerializeField]
		private float _startSpeed;

		[SerializeField]
		private Info.Reorderable _infos;

		private int _currentIndex;

		public override void Initialize(Projectile projectile, float direction)
		{
			base.Initialize(projectile, direction);
			_currentIndex = 0;
			_terrainCollisionDetector.Run();
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			float num = _startSpeed;
			for (int i = 0; i < _infos.values.Length; i++)
			{
				Info info = _infos.values[i];
				if (time > info.length)
				{
					num = info.targetSpeed;
					time -= info.length;
					continue;
				}
				if (info.clearHitHistory && _currentIndex != i)
				{
					base.projectile.ClearHitHistroy();
				}
				_currentIndex = i;
				float num2 = num + (info.targetSpeed - num) * info.curve.Evaluate(time / info.length);
				return new ValueTuple<Vector2, float>(base.directionVector, num2 * base.projectile.speedMultiplier);
			}
			return new ValueTuple<Vector2, float>(base.directionVector, num * base.projectile.speedMultiplier);
		}
	}
}
