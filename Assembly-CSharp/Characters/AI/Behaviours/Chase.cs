using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Chase : Behaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[2]
			{
				typeof(Chase),
				typeof(FlyChase)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToTarget))]
		private MoveToTarget _moveToTarget;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wanderWhenChaseFail;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		private Idle _beforeRangeWander;

		private void Start()
		{
			_childs = new List<Behaviour> { _moveToTarget, _wanderWhenChaseFail, _beforeRangeWander };
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			base.result = Result.Doing;
			if (target == null)
			{
				base.result = Result.Fail;
			}
			else if (character.movement.controller.isGrounded && target.movement.controller.isGrounded)
			{
				yield return _moveToTarget.CRun(controller);
				if (_moveToTarget.result == Result.Fail)
				{
					yield return _beforeRangeWander.CRun(controller);
					base.result = Result.Fail;
					yield return _wanderWhenChaseFail.CRun(controller);
				}
				else
				{
					base.result = Result.Success;
				}
			}
		}
	}
}
