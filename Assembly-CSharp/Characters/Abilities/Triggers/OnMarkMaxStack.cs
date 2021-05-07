using System;
using Characters.Marks;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnMarkMaxStack : Trigger
	{
		[SerializeField]
		private Transform _moveToTargetPosition;

		[SerializeField]
		private MarkInfo _mark;

		[SerializeField]
		private bool _clearMarkOnTriggered;

		private Character _character;

		public override void Attach(Character character)
		{
			MarkInfo mark = _mark;
			mark.onStack = (MarkInfo.OnStackDelegate)Delegate.Combine(mark.onStack, new MarkInfo.OnStackDelegate(OnStack));
		}

		public override void Detach()
		{
			MarkInfo mark = _mark;
			mark.onStack = (MarkInfo.OnStackDelegate)Delegate.Remove(mark.onStack, new MarkInfo.OnStackDelegate(OnStack));
		}

		private void OnStack(Mark mark, float stack)
		{
			if (!(stack < (float)_mark.maxStack) && base.canBeTriggered)
			{
				if (_moveToTargetPosition != null)
				{
					_moveToTargetPosition.position = MMMaths.RandomPointWithinBounds(mark.owner.collider.bounds);
				}
				if (_clearMarkOnTriggered)
				{
					mark.ClearStack(_mark);
				}
				Invoke();
			}
		}
	}
}
