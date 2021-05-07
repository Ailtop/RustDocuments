using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Characters.Actions
{
	public class EnhanceableComboAction : Action
	{
		[Serializable]
		internal class ActionInfo
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<ActionInfo>
			{
			}

			[SerializeField]
			private Vector2 _input;

			[SerializeField]
			private Vector2 _cancel;

			[SerializeField]
			private Motion _motion;

			internal Vector2 input => _input;

			internal Vector2 cancel => _cancel;

			internal Motion motion => _motion;
		}

		[Header("Actions")]
		[SerializeField]
		private ActionInfo.Reorderable _actionInfo;

		[Header("Enhanced Actions")]
		[SerializeField]
		private ActionInfo.Reorderable _enhancedActionInfo;

		[SerializeField]
		private int _cycleOffset;

		protected bool _cancelReserved;

		protected int _current;

		[NonSerialized]
		public bool enhanced;

		private ActionInfo current => _currentActionInfo[_current];

		public override Motion[] motions => _actionInfo.values.Select((ActionInfo m) => m.motion).ToArray();

		private ActionInfo[] _currentActionInfo
		{
			get
			{
				if (!enhanced)
				{
					return _actionInfo.values;
				}
				return _enhancedActionInfo.values;
			}
		}

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(current.motion);
				}
				return false;
			}
		}

		private IEnumerator CReservedAttack()
		{
			while (_cancelReserved)
			{
				Motion motion = _actionInfo.values[_current].motion;
				Motion motion2 = _enhancedActionInfo.values[_current].motion;
				ActionInfo actionInfo;
				if (_owner.runningMotion == motion)
				{
					actionInfo = _actionInfo.values[_current];
				}
				else
				{
					if (!(_owner.runningMotion == motion2))
					{
						break;
					}
					actionInfo = _enhancedActionInfo.values[_current];
				}
				if (MMMaths.Range(actionInfo.motion.time, actionInfo.cancel) && (_cancelReserved || MMMaths.Range(actionInfo.motion.time, actionInfo.input)))
				{
					_cancelReserved = false;
					int num = _current + 1;
					if (num >= _currentActionInfo.Length)
					{
						num = _cycleOffset;
					}
					_current = num;
					DoAction(_currentActionInfo[num].motion);
				}
				yield return null;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			for (int i = 0; i < _actionInfo.values.Length; i++)
			{
				_actionInfo.values[i].motion.Initialize(this);
			}
		}

		public override bool TryStart()
		{
			if (!canUse)
			{
				return false;
			}
			if (_owner.runningMotion != null && _owner.runningMotion.action == this)
			{
				if (_cancelReserved)
				{
					return false;
				}
				Motion motion = _actionInfo.values[_current].motion;
				Motion motion2 = _enhancedActionInfo.values[_current].motion;
				ActionInfo actionInfo;
				if (_owner.runningMotion == motion)
				{
					actionInfo = _actionInfo.values[_current];
				}
				else
				{
					if (!(_owner.runningMotion == motion2))
					{
						return false;
					}
					actionInfo = _enhancedActionInfo.values[_current];
				}
				if (actionInfo.input.x == actionInfo.input.y)
				{
					return false;
				}
				if (!MMMaths.Range(actionInfo.motion.time, actionInfo.input))
				{
					return false;
				}
				_cancelReserved = true;
				StartCoroutine(CReservedAttack());
				return true;
			}
			_current = 0;
			_cancelReserved = false;
			if (!ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(current.motion);
			return true;
		}
	}
}
