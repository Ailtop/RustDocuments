using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class PowerbombAction : Action
	{
		[SerializeField]
		[Tooltip("이 옵션을 활성화하면 땅에서 사용할 경우 즉시 landing motion이 발동됩니다.")]
		private bool _doLandingMotionIfGrounded;

		[SerializeField]
		[Tooltip("이 시간이 지난 후부터 땅에 있는 지 검사하여 땅에 있을 경우 강제로 landing motion을 실행시킵니다. 넉백이나 프레임 드랍 등으로 인해 landing motion이 실행되지 않는 경우를 방지하기 위해서이며, motion에 의해 캐릭터가 땅에서 떨어져 공중으로 뜨기 위한 시간 정도로 짧게 주는 것이 좋습니다. 보통 기본값인 0.1초로 충분합니다.")]
		private float _motionTimeout = 0.1f;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _motion;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _landingMotion;

		private Character.LookingDirection _lookingDirection;

		public Motion motion => _motion;

		public Motion landingMotion => _landingMotion;

		public override Motion[] motions => new Motion[2] { _motion, _landingMotion };

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_motion);
				}
				return false;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			_motion.onStart += delegate
			{
				_lookingDirection = base.owner.lookingDirection;
				base.owner.movement.onGrounded += OnGrounded;
			};
			_motion.onEnd += delegate
			{
				base.owner.movement.onGrounded -= OnGrounded;
			};
			_motion.onCancel += delegate
			{
				base.owner.movement.onGrounded -= OnGrounded;
			};
			_landingMotion.onStart += delegate
			{
				base.owner.lookingDirection = _lookingDirection;
			};
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			_motion.Initialize(this);
			_landingMotion.Initialize(this);
		}

		private void OnDisable()
		{
			base.owner.movement.onGrounded -= OnGrounded;
		}

		private void OnGrounded()
		{
			StopAllCoroutines();
			DoMotion(_landingMotion);
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			if (base.owner.movement.isGrounded && _doLandingMotionIfGrounded)
			{
				_lookingDirection = base.owner.lookingDirection;
				DoAction(_landingMotion);
			}
			else
			{
				DoAction(_motion);
				StopAllCoroutines();
				StartCoroutine(CExtraGroundCheck());
			}
			return true;
		}

		private IEnumerator CExtraGroundCheck()
		{
			float speedMultiplier = GetSpeedMultiplier(_motion);
			yield return Chronometer.global.WaitForSeconds(_motionTimeout / speedMultiplier);
			while (_motion.running)
			{
				if (base.owner.movement.isGrounded)
				{
					DoMotion(_landingMotion);
					break;
				}
				yield return null;
			}
		}
	}
}
