using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class MultiChargeAction : Action
	{
		[Serializable]
		protected class ChargeMotions
		{
			[Serializable]
			public class Reorderable : ReorderableArray<ChargeMotions>
			{
			}

			[Space]
			[Subcomponent(typeof(Motion))]
			public Motion charging;

			[Subcomponent(true, typeof(Motion))]
			public Motion charged;

			[Subcomponent(typeof(Motion))]
			public Motion finish;
		}

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _anticipation;

		[SerializeField]
		[Subcomponent(true, typeof(Motion))]
		protected Motion _prepare;

		[SerializeField]
		[Subcomponent(true, typeof(Motion))]
		protected Motion _earlyFinish;

		[Space]
		[SerializeField]
		protected ChargeMotions.Reorderable _chargeMotions;

		private Character.LookingDirection _lookingDirection;

		protected Motion[] _motions;

		private bool _earlyFinishReserved;

		public override Motion[] motions => _motions;

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
			List<Motion> list = new List<Motion>(8);
			list.Add(_anticipation);
			if (_prepare != null)
			{
				list.Add(_prepare);
			}
			ChargeMotions[] values = _chargeMotions.values;
			foreach (ChargeMotions chargeMotions in values)
			{
				list.Add(chargeMotions.charging);
				if (chargeMotions.charged != null)
				{
					list.Add(chargeMotions.charged);
				}
			}
			list.Add(_chargeMotions.values.Last().finish);
			for (int j = 0; j < list.Count - 1; j++)
			{
				Motion nextMotion = list[j + 1];
				list[j].onEnd += delegate
				{
					DoMotion(nextMotion);
				};
			}
			if (_earlyFinish != null)
			{
				list.Add(_earlyFinish);
			}
			foreach (Motion item in list)
			{
				item.Initialize(this);
			}
			if (_anticipation.blockLook)
			{
				_anticipation.onStart += delegate
				{
					_lookingDirection = _owner.lookingDirection;
				};
				if (_prepare != null)
				{
					_prepare.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C16_1;
				}
				if (_earlyFinish != null)
				{
					_earlyFinish.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C16_1;
				}
				values = _chargeMotions.values;
				foreach (ChargeMotions chargeMotions2 in values)
				{
					chargeMotions2.charging.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C16_1;
					if (chargeMotions2.charged != null)
					{
						chargeMotions2.charged.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C16_1;
					}
					chargeMotions2.finish.onStart += _003CInitializeMotions_003Eg__RepositLookingDirection_007C16_1;
					list.Add(chargeMotions2.finish);
					chargeMotions2.finish.Initialize(this);
				}
			}
			for (int k = 0; k < _chargeMotions.values.Length; k++)
			{
				ChargeMotions chargeMotions3 = _chargeMotions.values[k];
				Motion motion = null;
				if (k + 1 < _chargeMotions.values.Length)
				{
					motion = _chargeMotions.values[k + 1].charging;
				}
				if (k == 0)
				{
					chargeMotions3.charging.onStart += InvokeStartCharging;
				}
				chargeMotions3.charging.onCancel += InvokeCancelCharging;
				if (chargeMotions3.charged == null)
				{
					if (motion == null)
					{
						chargeMotions3.charging.onEnd += InvokeEndCharging;
					}
				}
				else
				{
					chargeMotions3.charged.onEnd += InvokeEndCharging;
					chargeMotions3.charged.onCancel += InvokeCancelCharging;
				}
			}
			_motions = list.ToArray();
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
			Motion runningMotion = base.owner.runningMotion;
			if (runningMotion == null || runningMotion.action != this)
			{
				return false;
			}
			if (runningMotion == _earlyFinish)
			{
				return false;
			}
			if (_earlyFinish != null && (runningMotion == _anticipation || runningMotion == _prepare))
			{
				ReserveEarlyFinish();
				return false;
			}
			for (int i = 0; i < _chargeMotions.values.Length; i++)
			{
				ChargeMotions chargeMotions = _chargeMotions.values[i];
				if (runningMotion == chargeMotions.finish)
				{
					return false;
				}
				if (runningMotion == chargeMotions.charging)
				{
					if (i == 0)
					{
						if (_earlyFinish == null)
						{
							return false;
						}
						DoMotion(_earlyFinish);
					}
					else
					{
						DoMotion(_chargeMotions.values[i - 1].finish);
					}
					return true;
				}
				if (runningMotion == chargeMotions.charged)
				{
					DoMotion(chargeMotions.finish);
					return true;
				}
			}
			base.owner.CancelAction();
			return false;
		}
	}
}
