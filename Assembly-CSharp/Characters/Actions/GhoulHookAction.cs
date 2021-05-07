using Characters.Gear.Weapons;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class GhoulHookAction : Action
	{
		[SerializeField]
		private GhoulHook _hook;

		[Header("Motion")]
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		private Motion _fire;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		private Motion _pull;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		private Motion _fly;

		[SerializeField]
		private Action _consume;

		private Character.LookingDirection _lookingDirection;

		public override Motion[] motions => new Motion[3] { _fire, _pull, _fly };

		public Motion motion => _fire;

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_fire);
				}
				return false;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			_hook.onTerrainHit += OnHookTerrainHit;
			_hook.onExpired += OnHookExpired;
			_hook.onPullEnd += OnHookPullEnd;
			_hook.onFlyEnd += OnHookFlyEnd;
			if (!_fire.blockLook)
			{
				return;
			}
			_fire.onStart += delegate
			{
				_lookingDirection = base.owner.lookingDirection;
			};
			_pull.onStart += delegate
			{
				base.owner.lookingDirection = _lookingDirection;
			};
			if (_consume != null)
			{
				_hook.onPullEnd += delegate
				{
					_consume.TryStart();
				};
				_consume.onStart += delegate
				{
					base.owner.lookingDirection = _lookingDirection;
				};
			}
			_fly.onStart += delegate
			{
				base.owner.lookingDirection = _lookingDirection;
			};
		}

		private void OnHookExpired()
		{
			DoMotion(_pull);
		}

		private void OnHookTerrainHit()
		{
			DoMotion(_fly);
		}

		private void OnHookPullEnd()
		{
			base.owner.CancelAction();
		}

		private void OnHookFlyEnd()
		{
			base.owner.CancelAction();
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			_fire.Initialize(this);
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_fire);
			return true;
		}
	}
}
