using System;

namespace BT
{
	public class Selector : Composite
	{
		private int _currentIndex;

		protected virtual Node GetChild(int i)
		{
			if (i >= _child.components.Length || i < 0)
			{
				throw new ArgumentException($"{i} : invalid child index");
			}
			return _child.components[i].node;
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			do
			{
				NodeState nodeState = GetChild(_currentIndex).Tick(context);
				if (nodeState != 0)
				{
					return nodeState;
				}
			}
			while (++_currentIndex < _child.components.Length);
			return NodeState.Fail;
		}

		protected override void DoReset(NodeState state)
		{
			_currentIndex = 0;
			base.DoReset(state);
		}
	}
}
