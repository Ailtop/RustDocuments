using System;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public abstract class Node : MonoBehaviour, INode
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[24]
			{
				typeof(AutoReset),
				typeof(CheckWithInSight),
				typeof(Cooldown),
				typeof(Conditional),
				typeof(Failer),
				typeof(Inverter),
				typeof(MoveToTarget),
				typeof(MoveTowards),
				typeof(MoveToLookingDirection),
				typeof(Random),
				typeof(RangeWander),
				typeof(RepeatForever),
				typeof(Repeat),
				typeof(RunAction),
				typeof(RunOperations),
				typeof(SetPositionTo),
				typeof(Selector),
				typeof(Sequence),
				typeof(Succeder),
				typeof(TimeLimit),
				typeof(TranslateTowards),
				typeof(UntilFail),
				typeof(UntilSuccess),
				typeof(WaitForDuration)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Node>
		{
		}

		protected NodeState _state = NodeState.Ready;

		public NodeState Tick(Context context)
		{
			if (_state == NodeState.Ready)
			{
				OnInitialize();
			}
			_state = UpdateDeltatime(context);
			if (_state == NodeState.Ready)
			{
				throw new InvalidOperationException("Warning infinite loop!!");
			}
			if (_state != NodeState.Running)
			{
				OnTerminate(_state);
			}
			return _state;
		}

		public void ResetState()
		{
			if (_state != NodeState.Ready)
			{
				DoReset(_state);
				_state = NodeState.Ready;
			}
		}

		protected abstract NodeState UpdateDeltatime(Context context);

		protected virtual void OnInitialize()
		{
		}

		protected virtual void OnStop()
		{
		}

		protected virtual void OnTerminate(NodeState state)
		{
		}

		protected virtual void DoReset(NodeState state)
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
