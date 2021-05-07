using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Wander : Behaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[3]
			{
				typeof(Wander),
				typeof(RangeWander),
				typeof(WanderForDuration)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[SerializeField]
		[Move.Subcomponent(true)]
		protected Move _move;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		protected Idle _idleWhenEndWander;

		[SerializeField]
		protected Collider2D _sightRange;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				yield return null;
				if (controller.target != null)
				{
					base.result = Result.Success;
					break;
				}
				if (Precondition.CanMove(character) && character.movement.controller.isGrounded)
				{
					_move.direction = (MMMaths.RandomBool() ? Vector2.left : Vector2.right);
					yield return _move.CRun(controller);
				}
			}
			yield return _idleWhenEndWander.CRun(controller);
		}
	}
}
