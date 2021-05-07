using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class RangeAttack : MonoBehaviour
	{
		[SerializeField]
		private Transform[] _attackPositions;

		[SerializeField]
		private Characters.Actions.Action _action;

		[SerializeField]
		[MinMaxSlider(15f, 90f)]
		private Vector2Int _angleOfMeteorInAir;

		[SerializeField]
		private float _distanceFromCenter = 6.5f;

		private Vector2 GetRightPosition(int minAngle, int maxAngle)
		{
			float angle = UnityEngine.Random.Range(minAngle, maxAngle);
			return RotateVector(Vector2.right, angle) * _distanceFromCenter;
		}

		private Vector2 GetLeftPosition(int minAngle, int maxAngle)
		{
			float num = UnityEngine.Random.Range(minAngle, maxAngle);
			return RotateVector(Vector2.right, num + 90f) * _distanceFromCenter;
		}

		private Vector2 RotateVector(Vector2 v, float angle)
		{
			float f = angle * ((float)Math.PI / 180f);
			float x = v.x * Mathf.Cos(f) - v.y * Mathf.Sin(f);
			float y = v.x * Mathf.Sin(f) + v.y * Mathf.Cos(f);
			return new Vector2(x, y);
		}

		private void SetSpawnPosition()
		{
			bool num = MMMaths.RandomBool();
			List<Vector2> list = new List<Vector2>(3);
			if (num)
			{
				list.Add(GetLeftPosition(_angleOfMeteorInAir.x, _angleOfMeteorInAir.y));
				Vector2 rightPosition = GetRightPosition(_angleOfMeteorInAir.x, _angleOfMeteorInAir.y);
				float num2 = Mathf.Atan2(rightPosition.y, rightPosition.x) * 57.29578f;
				list.Add(rightPosition);
				if (num2 >= 45f)
				{
					list.Add(GetRightPosition(_angleOfMeteorInAir.x, 40));
				}
				else
				{
					list.Add(GetRightPosition(50, _angleOfMeteorInAir.y));
				}
			}
			else
			{
				list.Add(GetRightPosition(_angleOfMeteorInAir.x, _angleOfMeteorInAir.y));
				Vector2 leftPosition = GetLeftPosition(_angleOfMeteorInAir.x, _angleOfMeteorInAir.y);
				float num3 = Mathf.Atan2(leftPosition.y, leftPosition.x) * 57.29578f;
				list.Add(leftPosition);
				if (num3 >= 45f)
				{
					list.Add(GetLeftPosition(_angleOfMeteorInAir.x, 40));
				}
				else
				{
					list.Add(GetLeftPosition(50, _angleOfMeteorInAir.y));
				}
			}
			for (int i = 0; i < _attackPositions.Length; i++)
			{
				_attackPositions[i].position = list[i];
			}
		}

		public IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			SetSpawnPosition();
			_action.TryStart();
			while (_action.running && !character.health.dead)
			{
				yield return null;
			}
		}
	}
}
