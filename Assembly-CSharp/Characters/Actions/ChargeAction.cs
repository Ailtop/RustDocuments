using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class ChargeAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _anticipation;

		[SerializeField]
		[Subcomponent(true, typeof(Motion))]
		protected Motion _prepare;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _charging;

		[SerializeField]
		[Subcomponent(true, typeof(Motion))]
		protected Motion _charged;

		[SerializeField]
		[Subcomponent(true, typeof(Motion))]
		protected Motion _earlyFinish;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _finish;

		private Character.LookingDirection _lookingDirection;

		protected Motion[] _motions;

		private bool _earlyFinishReserved;

		public override Motion[] motions => new Motion[6] { _anticipation, _prepare, _charging, _charged, _earlyFinish, _finish };

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_anticipation);
				}
				return false;
			}
		}

		public float chargedPercent => _charging.normalizedTime;

		public float chargingPercent
		{
			get
			{
				if (!_charging.running)
				{
					return 0f;
				}
				return _charging.normalizedTime;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeMotions();
		}

		private void InvokeStartCharging()
		{
			if (!_earlyFinishReserved)
			{
				_owner.onStartCharging?.Invoke(this);
			}
		}

		private void InvokeEndCharging()
		{
			_owner.onStopCharging?.Invoke(this);
		}

		private void InvokeCancelCharging()
		{
			_owner.onCancelCharging?.Invoke(this);
		}

		private void InitializeMotions()
		{
			List<Motion> list = new List<Motion>(6);
			list.Add(_anticipation);
			if (_prepare != null)
			{
				list.Add(_prepare);
			}
			list.Add(_charging);
			if (_charged != null)
			{
				list.Add(_charged);
			}
			list.Add(_finish);
			for (int i = 0; i < list.Count - 1; i++)
			{
				Motion nextMotion = list[i + 1];
				list[i].onEnd += delegate
				{
					DoMotion(nextMotion);
				};
			}
			if (_earlyFinish != null)
			{
				list.Add(_earlyFinish);
			}
			_motions = list.ToArray();
			Motion[] array = _motions;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Initialize(this);
			}
			if (_anticipation.blockLook)
			{
				_anticipation.onStart += delegate
				{
					_lookingDirection = _owner.lookingDirection;
				};
				if (_prepare != null)
				{
					_prepare.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C21_1;
				}
				_charging.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C21_1;
				if (_charged != null)
				{
					_charged.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C21_1;
				}
				if (_earlyFinish != null)
				{
					_earlyFinish.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C21_1;
				}
				_finish.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C21_1;
			}
			_charging.onStart += InvokeStartCharging;
			_charging.onCancel += InvokeCancelCharging;
			if (_charged == null)
			{
				_charging.onEnd += InvokeEndCharging;
				return;
			}
			_charged.onEnd += InvokeEndCharging;
			_charged.onCancel += InvokeCancelCharging;
		}

		public override bool TryStart()
		{
			if (!base.gameObject.activeSelf || !canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_anticipation);
			return true;
		}

		private void EarlyFinish()
		{
			_earlyFinishReserved = false;
			DoMotion(_earlyFinish);
			_anticipation.onEnd -= EarlyFinish;
			if (_prepare != null)
			{
				_prepare.onEnd -= EarlyFinish;
			}
		}

		public void ReserveEarlyFinish()
		{
			_earlyFinishReserved = true;
			_anticipation.onEnd -= EarlyFinish;
			_anticipation.onEnd += EarlyFinish;
			if (_prepare != null)
			{
				_prepare.onEnd -= EarlyFinish;
				_prepare.onEnd += EarlyFinish;
			}
		}

		public override bool TryEnd()
		{
			if (base.owner.motion == _finish || base.owner.motion == _earlyFinish)
			{
				return false;
			}
			if (_charged != null && base.owner.motion == _charged)
			{
				DoMotion(_finish);
				return true;
			}
			if (_earlyFinish != null && base.owner.motion != _earlyFinish && base.owner.motion != _finish)
			{
				if (base.owner.motion == _anticipation || base.owner.motion == _prepare)
				{
					ReserveEarlyFinish();
					return false;
				}
				DoMotion(_earlyFinish);
				return true;
			}
			base.owner.CancelAction();
			return false;
		}
	}
}
