using System.Collections;
using Characters.Controllers;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class Move : CharacterOperation
	{
		private const float directionThreshold = 0.66f;

		[SerializeField]
		private bool _useDashDistanceStat;

		[SerializeField]
		private float _movementSpeedFactor;

		[SerializeField]
		private Force _force;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private bool _needDirectionInput = true;

		private CoroutineReference _coroutineReference;

		public override void Run(Character owner)
		{
			if (owner.movement == null)
			{
				return;
			}
			if (_needDirectionInput)
			{
				PlayerInput component = owner.GetComponent<PlayerInput>();
				if (component != null && ((owner.lookingDirection == Character.LookingDirection.Left && component.direction.x >= -0.66f) || (owner.lookingDirection == Character.LookingDirection.Right && component.direction.x <= 0.66f)))
				{
					return;
				}
			}
			float extraPower = 0f;
			if (_movementSpeedFactor > 0f)
			{
				float num = Mathf.Abs((float)owner.stat.Get(Stat.Category.Constant, Stat.Kind.MovementSpeed));
				float num2 = Mathf.Abs((float)owner.stat.GetFinal(Stat.Kind.MovementSpeed));
				extraPower = Mathf.Max(0f, num2 - num) * _curve.duration * _movementSpeedFactor;
			}
			Vector2 vector = _force.Evaluate(owner, extraPower);
			if (_useDashDistanceStat)
			{
				vector *= (float)owner.stat.GetFinal(Stat.Kind.DashDistance);
			}
			if (_curve.duration > 0f)
			{
				_coroutineReference.Stop();
				_coroutineReference = owner.StartCoroutineWithReference(CMove(owner, _curve, vector));
			}
			else
			{
				owner.movement.force += vector;
			}
		}

		internal static IEnumerator CMove(Character character, Curve curve, Vector2 distance)
		{
			float t = 0f;
			float amountBefore = 0f;
			for (; t < curve.duration; t += character.chronometer.animation.deltaTime)
			{
				if (character == null)
				{
					break;
				}
				if (!character.liveAndActive)
				{
					break;
				}
				float num = curve.Evaluate(t);
				character.movement.force += distance * (num - amountBefore);
				amountBefore = num;
				yield return null;
			}
		}

		public override void Stop()
		{
			_coroutineReference.Stop();
		}
	}
}
