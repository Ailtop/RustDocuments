using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class ChargeComboAction : Action
	{
		[Serializable]
		internal class ActionInfo
		{
			[Serializable]
			internal class Reorderable : ReorderableArray<ActionInfo>
			{
			}

			[MinMaxSlider(0f, 1f)]
			[Tooltip("fnish 모션을 캔슬하기 위한 입력 구간, 사용자의 입력이 input 범위 내이지만 cancel 보다 빠를 경우 선입력으로 예약됨")]
			public Vector2 input;

			[MinMaxSlider(0f, 1f)]
			[Tooltip("finish 모션이 캔슬될 수 있는 구간")]
			public Vector2 cancel;

			[MinMaxSlider(0f, 1f)]
			[Tooltip("earlyFinish 모션을 캔슬하기 위한 입력 구간, 사용자의 입력이 input 범위 내이지만 earlyCancel 보다 빠를 경우 선입력으로 예약됨")]
			public Vector2 earlyInput;

			[MinMaxSlider(0f, 1f)]
			[Tooltip("earlyFinish 모션이 캔슬될 수 있는 구간")]
			public Vector2 earlyCancel;

			[Space]
			[Subcomponent(typeof(Motion))]
			public Motion anticipation;

			[Subcomponent(true, typeof(Motion))]
			public Motion prepare;

			[Subcomponent(typeof(Motion))]
			public Motion charging;

			[Subcomponent(true, typeof(Motion))]
			public Motion charged;

			[Subcomponent(true, typeof(Motion))]
			public Motion earlyFinish;

			[Subcomponent(typeof(Motion))]
			public Motion finish;

			private bool _earlyFinishReserved;

			private ChargeComboAction _action;

			private Motion[] _motions;

			public Motion[] motions
			{
				get
				{
					Motion[] array = _motions;
					if (array == null)
					{
						Motion[] obj = new Motion[6] { anticipation, prepare, charging, charged, earlyFinish, finish };
						Motion[] array2 = obj;
						_motions = obj;
						array = array2;
					}
					return array;
				}
			}

			public void InitializeMotions(ChargeComboAction action)
			{
				_003C_003Ec__DisplayClass16_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass16_0();
				CS_0024_003C_003E8__locals0.action = action;
				CS_0024_003C_003E8__locals0._003C_003E4__this = this;
				_action = CS_0024_003C_003E8__locals0.action;
				List<Motion> list = new List<Motion>(6);
				list.Add(anticipation);
				if (prepare != null)
				{
					list.Add(prepare);
				}
				list.Add(charging);
				if (charged != null)
				{
					list.Add(charged);
				}
				list.Add(finish);
				for (int i = 0; i < list.Count - 1; i++)
				{
					_003C_003Ec__DisplayClass16_0 _003C_003Ec__DisplayClass16_ = CS_0024_003C_003E8__locals0;
					Motion nextMotion = list[i + 1];
					list[i].onEnd += delegate
					{
						_003C_003Ec__DisplayClass16_.action.DoMotion(nextMotion);
					};
				}
				if (earlyFinish != null)
				{
					list.Add(earlyFinish);
				}
				_motions = list.ToArray();
				Motion[] array = _motions;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].Initialize(CS_0024_003C_003E8__locals0.action);
				}
				if (anticipation.blockLook)
				{
					anticipation.onStart += delegate
					{
						CS_0024_003C_003E8__locals0.action._lookingDirection = CS_0024_003C_003E8__locals0.action._owner.lookingDirection;
					};
					if (prepare != null)
					{
						prepare.onStart += CS_0024_003C_003E8__locals0._003CInitializeMotions_003Eg__RepositLookingDirection_007C1;
					}
					charging.onStart += CS_0024_003C_003E8__locals0._003CInitializeMotions_003Eg__RepositLookingDirection_007C1;
					if (charged != null)
					{
						charged.onStart += CS_0024_003C_003E8__locals0._003CInitializeMotions_003Eg__RepositLookingDirection_007C1;
					}
					if (earlyFinish != null)
					{
						earlyFinish.onStart += CS_0024_003C_003E8__locals0._003CInitializeMotions_003Eg__RepositLookingDirection_007C1;
					}
					finish.onStart += CS_0024_003C_003E8__locals0._003CInitializeMotions_003Eg__RepositLookingDirection_007C1;
				}
				charging.onStart += delegate
				{
					if (!CS_0024_003C_003E8__locals0._003C_003E4__this._earlyFinishReserved)
					{
						CS_0024_003C_003E8__locals0.action.InvokeStartCharging();
					}
				};
				charging.onCancel += CS_0024_003C_003E8__locals0.action.InvokeCancelCharging;
				if (charged == null)
				{
					charging.onEnd += CS_0024_003C_003E8__locals0.action.InvokeEndCharging;
					return;
				}
				charged.onEnd += CS_0024_003C_003E8__locals0.action.InvokeEndCharging;
				charged.onCancel += CS_0024_003C_003E8__locals0.action.InvokeCancelCharging;
			}

			private void EarlyFinish()
			{
				_earlyFinishReserved = false;
				_action.DoMotion(earlyFinish);
				anticipation.onEnd -= EarlyFinish;
				if (prepare != null)
				{
					prepare.onEnd -= EarlyFinish;
				}
			}

			public void ReserveEarlyFinish()
			{
				_earlyFinishReserved = true;
				anticipation.onEnd -= EarlyFinish;
				anticipation.onEnd += EarlyFinish;
				if (prepare != null)
				{
					prepare.onEnd -= EarlyFinish;
					prepare.onEnd += EarlyFinish;
				}
			}
		}

		[SerializeField]
		private ActionInfo.Reorderable _actionInfo;

		[SerializeField]
		private int _cycleOffset;

		protected bool _cancelReserved;

		protected bool _endReserved;

		protected int _current;

		private Character.LookingDirection _lookingDirection;

		private Motion[] _motions;

		internal ActionInfo current => _actionInfo.values[_current];

		public override Motion[] motions
		{
			get
			{
				if (_motions == null)
				{
					_motions = _actionInfo.values.SelectMany((ActionInfo m) => m.motions).ToArray();
				}
				return _motions;
			}
		}

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(current.anticipation);
				}
				return false;
			}
		}

		private IEnumerator CReservedAttack()
		{
			while (_cancelReserved)
			{
				Vector2 range;
				Vector2 range2;
				if (_owner.runningMotion == current.earlyFinish)
				{
					range = current.earlyInput;
					range2 = current.earlyCancel;
				}
				else
				{
					if (!(_owner.runningMotion == current.finish))
					{
						break;
					}
					range = current.input;
					range2 = current.cancel;
				}
				if (MMMaths.Range(_owner.runningMotion.normalizedTime, range2) && (_cancelReserved || MMMaths.Range(_owner.runningMotion.normalizedTime, range)))
				{
					_cancelReserved = false;
					int num = _current + 1;
					if (num >= _actionInfo.values.Length)
					{
						num = _cycleOffset;
					}
					ActionInfo actionInfo = _actionInfo.values[num];
					if (_endReserved)
					{
						_endReserved = false;
						actionInfo.ReserveEarlyFinish();
					}
					_current = num;
					DoAction(actionInfo.anticipation);
				}
				yield return null;
			}
		}

		protected override void Awake()
		{
			for (int i = 0; i < _actionInfo.values.Length; i++)
			{
				_actionInfo.values[i].InitializeMotions(this);
			}
		}

		private void InvokeStartCharging()
		{
			_owner.onStartCharging?.Invoke(this);
		}

		private void InvokeEndCharging()
		{
			_owner.onStopCharging?.Invoke(this);
		}

		private void InvokeCancelCharging()
		{
			_owner.onCancelCharging?.Invoke(this);
		}

		public override bool TryStart()
		{
			if (!base.gameObject.activeSelf || !canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			Motion runningMotion = _owner.runningMotion;
			if (runningMotion != null && runningMotion.action == this)
			{
				if (_cancelReserved)
				{
					return false;
				}
				Vector2 range;
				if (_owner.runningMotion == current.earlyFinish)
				{
					range = current.earlyInput;
				}
				else
				{
					if (!(_owner.runningMotion == current.finish))
					{
						return false;
					}
					range = current.input;
				}
				if (range.x == range.y)
				{
					return false;
				}
				if (!MMMaths.Range(runningMotion.normalizedTime, range))
				{
					return false;
				}
				_cancelReserved = true;
				StartCoroutine(CReservedAttack());
				return true;
			}
			_current = 0;
			_cancelReserved = false;
			_endReserved = false;
			DoAction(current.anticipation);
			return true;
		}

		public override bool TryEnd()
		{
			Motion runningMotion = _owner.runningMotion;
			if (runningMotion == null || runningMotion.action != this)
			{
				return false;
			}
			if (runningMotion == current.earlyFinish || runningMotion == current.finish)
			{
				if (_cancelReserved)
				{
					_endReserved = true;
				}
				return false;
			}
			if (current.charged != null && runningMotion == current.charged)
			{
				DoMotion(current.finish);
				return true;
			}
			if (current.earlyFinish != null && runningMotion != current.earlyFinish && runningMotion != current.finish)
			{
				if (base.owner.motion == current.anticipation || base.owner.motion == current.prepare)
				{
					current.ReserveEarlyFinish();
					return false;
				}
				DoMotion(current.earlyFinish);
				return true;
			}
			base.owner.CancelAction();
			return false;
		}
	}
}
