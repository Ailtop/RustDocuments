using System;

namespace BT
{
	public class Sequence : Composite
	{
		private int _currentIndex;

		protected virtual Node GetChild(int i)
		{
			if (i >= _child.components.Length || i < 0)
			{
				throw new ArgumentException("invalid child index");
			}
			return _child.components[i].node;
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			do
			{
				NodeState nodeState = GetChild(_currentIndex).Tick(context);
				if (nodeState != NodeState.Success)
				{
					return nodeState;
				}
			}
			while (++_currentIndex < _child.components.Length);
			return NodeState.Success;
		}

		protected override void DoReset(NodeState state)
		{
			_currentIndex = 0;
			base.DoReset(state);
		}
	}
}
