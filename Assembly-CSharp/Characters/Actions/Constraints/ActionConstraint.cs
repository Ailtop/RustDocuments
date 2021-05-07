using System;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class ActionConstraint : Constraint
	{
		[Serializable]
		public class Exception
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<Exception>
			{
			}

			[SerializeField]
			private Motion _motion;

			[SerializeField]
			[MinMaxSlider(0f, 1f)]
			private Vector2 _range;

			public Motion motion => _motion;

			public Vector2 range => _range;
		}

		[SerializeField]
		private ActionTypeBoolArray _canCancel;

		[SerializeField]
		private Exception.Reorderable _exceptions;

		public override bool Pass()
		{
			Motion runningMotion = _action.owner.runningMotion;
			if (runningMotion == null)
			{
				return true;
			}
			for (int i = 0; i < _exceptions.values.Length; i++)
			{
				Exception ex = _exceptions.values[i];
				if (ex.motion == runningMotion)
				{
					if (ex.range.x == ex.range.y)
					{
						return false;
					}
					return MMMaths.Range(runningMotion.normalizedTime, ex.range);
				}
			}
			return _canCancel.GetOrDefault(runningMotion.action.type);
		}
	}
}
