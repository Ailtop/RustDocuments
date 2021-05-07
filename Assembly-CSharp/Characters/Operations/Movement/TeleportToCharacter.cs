using System.Collections.Generic;
using Characters.Movements;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class TeleportToCharacter : CharacterOperation
	{
		public enum FindingMethod
		{
			Random,
			Closest
		}

		private const int _maxTargets = 32;

		private static readonly NonAllocOverlapper _overlapper;

		private static readonly List<Character> _characters;

		[Header("Find")]
		[SerializeField]
		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		[SerializeField]
		private Collider2D _findingRange;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizeFindingRange = true;

		[SerializeField]
		private FindingMethod _findingMethod;

		[SerializeField]
		private bool _onlyGroundedTarget;

		[Header("Position")]
		[SerializeField]
		private float _xOffset = 1f;

		[SerializeField]
		private bool _flipXOffsetByTargetDirection;

		static TeleportToCharacter()
		{
			_overlapper = new NonAllocOverlapper(32);
			_characters = new List<Character>(32);
			_overlapper.contactFilter.SetLayerMask(1024);
		}

		private void Awake()
		{
			if (_optimizeFindingRange)
			{
				_findingRange.enabled = false;
			}
		}

		public override void Run(Character owner)
		{
			Character character;
			using (new UsingCollider(_findingRange, _optimizeFindingRange))
			{
				character = FindTargetCharacter(owner.collider, _findingRange, _layer.Evaluate(owner.gameObject), _findingMethod, _onlyGroundedTarget);
			}
			if (!(character == null))
			{
				bool flag = false;
				if (_flipXOffsetByTargetDirection)
				{
					flag = ((character.movement.config.type != Characters.Movements.Movement.Config.Type.Walking) ? MMMaths.RandomBool() : (character.lookingDirection == Character.LookingDirection.Left));
				}
				Vector2 destination = character.transform.position;
				Character.LookingDirection lookingDirection;
				if (flag)
				{
					destination.x -= _xOffset;
					lookingDirection = Character.LookingDirection.Right;
				}
				else
				{
					destination.x += _xOffset;
					lookingDirection = Character.LookingDirection.Left;
				}
				if (owner.movement.controller.TeleportUponGround(destination) || owner.movement.controller.Teleport(destination))
				{
					owner.ForceToLookAt(lookingDirection);
					owner.movement.verticalVelocity = 0f;
				}
			}
		}

		private static Character FindTargetCharacter(Collider2D origin, Collider2D range, LayerMask layerMask, FindingMethod findingMethod, bool onlyGroundedTarget)
		{
			_overlapper.contactFilter.SetLayerMask(layerMask);
			List<Target> components = _overlapper.OverlapCollider(range).GetComponents<Target>();
			if (components.Count == 0)
			{
				return null;
			}
			if (components.Count == 1)
			{
				return components[0].character;
			}
			_characters.Clear();
			foreach (Target item in components)
			{
				if (!(item.character == null) && (!onlyGroundedTarget || item.character.movement.isGrounded))
				{
					_characters.Add(item.character);
				}
			}
			if (_characters.Count == 0)
			{
				return null;
			}
			if (_characters.Count == 1)
			{
				return _characters[0];
			}
			switch (findingMethod)
			{
			case FindingMethod.Random:
				return _characters.Random();
			case FindingMethod.Closest:
			{
				float num = float.MaxValue;
				Character result = null;
				{
					foreach (Character character in _characters)
					{
						float distance = Physics2D.Distance(origin, character.collider).distance;
						if (num > distance)
						{
							result = character;
							num = distance;
						}
					}
					return result;
				}
			}
			default:
				return null;
			}
		}
	}
}
