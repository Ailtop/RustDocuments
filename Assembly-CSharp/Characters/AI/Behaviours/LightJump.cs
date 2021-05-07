using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class LightJump : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Teleport))]
		private Teleport _teleport;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _fall;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(EscapeTeleport))]
		private EscapeTeleport _escapeTeleport;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ShiftObject))]
		private ShiftObject _shiftObject;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Hide))]
		private Hide _hide;

		[SerializeField]
		private Action _teleportStart;

		[SerializeField]
		private Action _teleportEnd;

		private void Awake()
		{
			_childs = new List<Behaviour> { _teleport, _fall, _attack, _escapeTeleport };
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _hide.CRun(controller);
			if (!controller.character.movement.controller.Teleport(_destination.position) || !_teleportEnd.TryStart())
			{
				yield break;
			}
			while (_teleportEnd.running && base.result == Result.Doing)
			{
				yield return null;
			}
			if (base.result != Result.Doing)
			{
				yield break;
			}
			StartCoroutine(_fall.CRun(controller));
			while (_fall.result == Result.Doing)
			{
				if (controller.character.movement.isGrounded)
				{
					controller.character.CancelAction();
					_fall.StopPropagation();
					yield break;
				}
				yield return null;
			}
			if (base.result == Result.Doing)
			{
				yield return _attack.CRun(controller);
				if (base.result == Result.Doing)
				{
					yield return _escapeTeleport.CRun(controller);
					base.result = Result.Done;
				}
			}
		}

		public bool CanUse(Character character)
		{
			if (!_fall.CanUse() || !_attack.CanUse())
			{
				return false;
			}
			if (!character.movement.isGrounded)
			{
				return false;
			}
			Bounds bounds = character.collider.bounds;
			bounds.center = new Vector2(_destination.position.x, _destination.position.y + (bounds.center.y - bounds.min.y));
			NonAllocOverlapper.shared.contactFilter.SetLayerMask((int)Layers.terrainMask | 0x11);
			if (NonAllocOverlapper.shared.OverlapBox(bounds.center, bounds.size, 0f).results.Count == 0)
			{
				return true;
			}
			return false;
		}
	}
}
