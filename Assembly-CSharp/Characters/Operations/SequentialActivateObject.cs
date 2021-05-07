using UnityEngine;

namespace Characters.Operations
{
	public class SequentialActivateObject : CharacterOperation
	{
		private enum Order
		{
			Random,
			Increase,
			Decrease
		}

		private enum ParentType
		{
			Random,
			Static
		}

		[SerializeField]
		private Order _order;

		[SerializeField]
		private ParentType _parentType;

		[SerializeField]
		private ParentPool _parentPool;

		private int[] _indics;

		private int _currentIndex;

		private Transform _parent;

		private void Awake()
		{
			SetParent();
			_indics = new int[_parent.childCount];
			SetOrder();
		}

		public override void Run(Character owner)
		{
			ActivateNextObject();
		}

		private void ActivateNextObject()
		{
			if (_currentIndex >= _indics.Length)
			{
				SetParent();
				_indics = new int[_parent.childCount];
				SetOrder();
				_currentIndex = 0;
			}
			DarkRushEffect component = _parent.GetChild(_indics[_currentIndex]).gameObject.GetComponent<DarkRushEffect>();
			component.SetSignEffectOrder(_currentIndex);
			component.SetImpactEffectOrder(_currentIndex);
			component.ShowSign();
			_currentIndex++;
		}

		private void SetParent()
		{
			if (_parentType == ParentType.Static)
			{
				_parent = _parentPool.GetFirstParent();
			}
			else if (_parentType == ParentType.Random)
			{
				_parent = _parentPool.GetRandomParent();
			}
		}

		private void SetOrder()
		{
			for (int i = 0; i < _indics.Length; i++)
			{
				_indics[i] = i;
			}
			if (_order == Order.Random)
			{
				_indics.Shuffle();
			}
			else if (_order == Order.Decrease)
			{
				_indics.Reverse();
			}
		}
	}
}
