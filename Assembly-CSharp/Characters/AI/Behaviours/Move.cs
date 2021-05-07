using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public abstract class Move : Behaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[6]
			{
				typeof(MoveForDuration),
				typeof(MoveToDestination),
				typeof(MoveToTarget),
				typeof(MoveToBehindWithFly),
				typeof(MoveForDurationWithFly),
				typeof(Fly)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[HideInInspector]
		public Vector2 direction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		protected Behaviour idle;

		[SerializeField]
		protected bool checkWithinSight;

		[SerializeField]
		protected bool wander;

		protected void Start()
		{
			_childs = new List<Behaviour> { idle };
		}

		protected bool LookAround(AIController controller)
		{
			Character character = controller.FindClosestPlayerBody(controller.stopTrigger);
			if (character != null)
			{
				controller.target = character;
				return true;
			}
			return false;
		}
	}
}
