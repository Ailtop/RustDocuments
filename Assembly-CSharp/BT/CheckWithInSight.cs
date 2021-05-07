using BT.SharedValues;
using Characters;
using PhysicsUtils;
using UnityEngine;

namespace BT
{
	public sealed class CheckWithInSight : Node
	{
		[SerializeField]
		private TargetLayer _targetLayer;

		[SerializeField]
		private Collider2D _range;

		private Character _owner;

		private static readonly NonAllocOverlapper _overlapper;

		static CheckWithInSight()
		{
			_overlapper = new NonAllocOverlapper(31);
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			if (_owner == null)
			{
				_owner = context.Get<Character>(Key.OwnerCharacter);
			}
			Character character = FindTarget();
			if (character == null)
			{
				return NodeState.Fail;
			}
			context.Set(Key.Target, new SharedValue<Character>(character));
			return NodeState.Success;
		}

		private Character FindTarget()
		{
			_overlapper.contactFilter.SetLayerMask(_targetLayer.Evaluate(_owner.gameObject));
			return TargetFinder.FindClosestTarget(_overlapper, _range, _owner.collider);
		}
	}
}
