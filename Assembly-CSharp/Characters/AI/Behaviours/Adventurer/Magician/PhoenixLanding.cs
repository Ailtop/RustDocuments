using System.Collections;
using Characters.Actions;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Magician
{
	public class PhoenixLanding : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(AttachAbility))]
		private AttachAbility _attachSpeedAbility;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToDestinationWithFly;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(SkipableIdle))]
		private SkipableIdle _idle;

		[SerializeField]
		private Action _ready;

		[SerializeField]
		private Action _attack;

		[SerializeField]
		private Action _landing;

		[SerializeField]
		private Transform _landingEffectPoint;

		[SerializeField]
		private float _height;

		[SerializeField]
		private Transform _origin;

		private float _originY;

		private void Start()
		{
			_originY = _origin.position.y;
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character target = controller.target;
			Character character = controller.character;
			Collider2D collider;
			Bounds bounds = ((character.movement.controller.collisionState.lastStandingCollider != null) ? character.movement.controller.collisionState.lastStandingCollider.bounds : ((!character.movement.TryGetClosestBelowCollider(out collider, character.movement.controller.terrainMask)) ? target.movement.controller.collisionState.lastStandingCollider.bounds : collider.bounds));
			controller.destination = new Vector2(target.transform.position.x, bounds.max.y + _height);
			_landingEffectPoint.position = new Vector2(controller.destination.x, bounds.max.y);
			_attachSpeedAbility.Run(character);
			yield return _moveToDestinationWithFly.CRun(controller);
			_attachSpeedAbility.Stop();
			if (base.result != Result.Doing)
			{
				yield break;
			}
			_ready.TryStart();
			while (_ready.running)
			{
				if (base.result != Result.Doing)
				{
					yield break;
				}
				yield return null;
			}
			_attack.TryStart();
			while (_attack.running)
			{
				if (base.result != Result.Doing)
				{
					yield break;
				}
				yield return null;
			}
			_landing.TryStart();
			while (_landing.running)
			{
				if (base.result != Result.Doing)
				{
					yield break;
				}
				yield return null;
			}
			yield return _idle.CRun(controller);
			if (base.result == Result.Doing)
			{
				controller.destination = new Vector2(character.transform.position.x, _originY);
				if (base.result == Result.Doing)
				{
					yield return _moveToDestinationWithFly.CRun(controller);
					base.result = Result.Done;
				}
			}
		}

		public bool CanUse()
		{
			if (_attack.canUse)
			{
				return _landing.canUse;
			}
			return false;
		}
	}
}
