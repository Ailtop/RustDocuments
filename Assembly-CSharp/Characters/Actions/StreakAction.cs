using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class StreakAction : Action
	{
		[SerializeField]
		[HideInInspector]
		[Subcomponent(true, typeof(Motion))]
		private Motion _startMotion;

		[SerializeField]
		[HideInInspector]
		[MinMaxSlider(0f, 1f)]
		private Vector2 _mainInput = new Vector2(0f, 1f);

		[SerializeField]
		[HideInInspector]
		[MinMaxSlider(0f, 1f)]
		private Vector2 _mainCancel = new Vector2(0.9f, 1f);

		[SerializeField]
		[HideInInspector]
		private bool _blockLook = true;

		[SerializeField]
		[HideInInspector]
		[Subcomponent(typeof(Motion))]
		private Motion _motion;

		[SerializeField]
		[HideInInspector]
		private bool _cancelToEnd;

		[SerializeField]
		[HideInInspector]
		[Subcomponent(true, typeof(Motion))]
		private Motion _endMotion;

		[SerializeField]
		[HideInInspector]
		[Subcomponent(true, typeof(Motion))]
		private Motion _fullStreakEndMotion;

		private bool _startReserved;

		private bool _endReserved;

		private Character.LookingDirection _lookingDirection;

		public override Motion[] motions => new Motion[3] { _startMotion, _motion, _endMotion };

		public Motion motion => _motion;

		public override bool canUse
		{
			get
			{
				if (!base.cooldown.canUse)
				{
					return false;
				}
				if (_owner.stunedOrFreezed)
				{
					return false;
				}
				if (!PassAllConstraints((_startMotion != null) ? _startMotion : _motion))
				{
					return false;
				}
				Motion runningMotion = base.owner.runningMotion;
				if (runningMotion != null)
				{
					if (runningMotion == _endMotion)
					{
						return false;
					}
					if (runningMotion == _fullStreakEndMotion)
					{
						return false;
					}
				}
				return true;
			}
		}

		private void CacheLookingDirection()
		{
			_lookingDirection = base.owner.lookingDirection;
		}

		private void RestoreLookingDirection()
		{
			base.owner.lookingDirection = _lookingDirection;
		}

		private void Expire()
		{
			base.cooldown.streak.Expire();
			_onEnd?.Invoke();
		}

		protected override void Awake()
		{
			base.Awake();
			_motion.Initialize(this);
			base.cooldown.Serialize();
			if (_motion.blockLook && _blockLook)
			{
				_motion.onStart += RestoreLookingDirection;
			}
			else if (_startMotion != null)
			{
				_motion.onStart += delegate
				{
					if (base.owner.runningMotion == _startMotion)
					{
						RestoreLookingDirection();
					}
				};
			}
			_motion.onEnd += OnMotionEnd;
			if (_startMotion != null)
			{
				_startMotion.Initialize(this);
				_startMotion.onEnd += delegate
				{
					DoMotion(_motion);
				};
				base.cooldown.streak.timeout += _startMotion.length;
			}
			if (_endMotion != null)
			{
				_endMotion.Initialize(this);
				if (_endMotion.blockLook && _blockLook)
				{
					_endMotion.onStart += RestoreLookingDirection;
				}
				_endMotion.onEnd += Expire;
			}
			if (_fullStreakEndMotion != null)
			{
				_fullStreakEndMotion.Initialize(this);
				if (_fullStreakEndMotion.blockLook && _blockLook)
				{
					_fullStreakEndMotion.onStart += RestoreLookingDirection;
				}
				_fullStreakEndMotion.onEnd += Expire;
				base.cooldown.streak.timeout += _fullStreakEndMotion.length;
			}
			else if (_endMotion != null)
			{
				_fullStreakEndMotion = _endMotion;
				base.cooldown.streak.timeout += _endMotion.length;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			base.cooldown.streak.timeout = _motion.length * (float)base.cooldown.streak.count;
		}

		private void OnMotionEnd()
		{
			if (_fullStreakEndMotion != null && base.cooldown.streak.count > 0 && base.cooldown.streak.remains == 0)
			{
				DoMotion(_fullStreakEndMotion);
			}
			else if (_endReserved || _inputMethod != 0)
			{
				if (_endMotion != null)
				{
					DoMotion(_endMotion);
				}
				else
				{
					Expire();
				}
			}
			else if (_inputMethod == InputMethod.TryStartIsPressed && base.cooldown.streak.remains > 0 && ConsumeCooldownIfNeeded())
			{
				DoMotion(_motion);
			}
		}

		public override bool TryStart()
		{
			Motion runningMotion = _owner.runningMotion;
			if (runningMotion != null && (runningMotion == _endMotion || runningMotion == _fullStreakEndMotion))
			{
				return false;
			}
			if (HandleWasPressed())
			{
				return true;
			}
			if ((_startMotion != null && runningMotion == _startMotion) || runningMotion == _motion)
			{
				return false;
			}
			if (base.cooldown.streak.remains > 0)
			{
				return false;
			}
			if (!canUse)
			{
				return false;
			}
			if (!ConsumeCooldownIfNeeded())
			{
				return false;
			}
			CacheLookingDirection();
			if (_startMotion != null)
			{
				DoAction(_startMotion);
			}
			else
			{
				DoAction(_motion);
			}
			_startReserved = false;
			_endReserved = false;
			return true;
		}

		private bool HandleWasPressed()
		{
			if (_inputMethod != InputMethod.TryStartWasPressed)
			{
				return false;
			}
			if (_startReserved)
			{
				return false;
			}
			if (!base.cooldown.canUse)
			{
				return false;
			}
			if (base.cooldown.streak.remains == 0)
			{
				return false;
			}
			Motion runningMotion = _owner.runningMotion;
			if (runningMotion == null)
			{
				return false;
			}
			if (runningMotion != _motion)
			{
				return false;
			}
			if (_mainInput.x == _mainInput.y)
			{
				return false;
			}
			if (!MMMaths.Range(runningMotion.normalizedTime, _mainInput))
			{
				return false;
			}
			if (MMMaths.Range(runningMotion.normalizedTime, _mainCancel))
			{
				if (ConsumeCooldownIfNeeded())
				{
					DoMotion(_motion);
				}
			}
			else
			{
				_startReserved = true;
				StartCoroutine(CReservedAttack());
			}
			return true;
		}

		private IEnumerator CReservedAttack()
		{
			while (_owner.runningMotion == _motion && _startReserved)
			{
				yield return null;
				if (MMMaths.Range(_owner.runningMotion.normalizedTime, _mainCancel))
				{
					if (ConsumeCooldownIfNeeded())
					{
						DoMotion(_motion);
					}
					break;
				}
			}
			_startReserved = false;
		}

		public override bool TryEnd()
		{
			if (_inputMethod != 0)
			{
				return false;
			}
			_endReserved = true;
			if (_cancelToEnd)
			{
				if (_endMotion == null)
				{
					base.cooldown.streak.Expire();
					base.owner.CancelAction();
					_onEnd?.Invoke();
					return false;
				}
				DoMotion(_endMotion);
				return true;
			}
			return false;
		}
	}
}
