using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations
{
	public class BoundsHeal : CharacterOperation
	{
		private enum Type
		{
			Percent,
			Constnat
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private CustomFloat _amount;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfos))]
		private OperationInfos _ToTargetOperations;

		private static readonly NonAllocOverlapper _targetOverlapper;

		static BoundsHeal()
		{
			_targetOverlapper = new NonAllocOverlapper(15);
			_targetOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Awake()
		{
			_ToTargetOperations.Initialize();
		}

		public override void Run(Character owner)
		{
			foreach (Character component in _targetOverlapper.OverlapCollider(_range).GetComponents<Character>())
			{
				component.health.Heal(GetAmount(component));
				_ToTargetOperations.Run(component);
			}
		}

		private double GetAmount(Character target)
		{
			switch (_type)
			{
			case Type.Percent:
				return (double)_amount.value * target.health.maximumHealth * 0.01;
			case Type.Constnat:
				return _amount.value;
			default:
				return 0.0;
			}
		}
	}
}
