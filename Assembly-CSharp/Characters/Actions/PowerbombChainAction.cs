using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class PowerbombChainAction : Action
	{
		[SerializeField]
		[Tooltip("이 옵션을 활성화하면 땅에서 사용할 경우 즉시 landing motion이 발동됩니다.")]
		private bool _doLandingMotionIfGrounded;

		[SerializeField]
		[Tooltip("이 시간이 지난 후부터 땅에 있는 지 검사하여 땅에 있을 경우 강제로 landing motion을 실행시킵니다. 넉백이나 프레임 드랍 등으로 인해 landing motion이 실행되지 않는 경우를 방지하기 위해서이며, motion에 의해 캐릭터가 땅에서 떨어져 공중으로 뜨기 위한 시간 정도로 짧게 주는 것이 좋습니다. 보통 기본값인 0.1초로 충분합니다.")]
		private float _motionTimeout = 0.1f;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _motions;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _landingMotions;

		private Character.LookingDirection _lookingDirection;

		public override Motion[] motions => _motions.components.Concat(_landingMotions.components).ToArray();

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(motions[0]);
				}
				return false;
			}
		}

		protected override void Awake()
		{
			_003C_003Ec__DisplayClass9_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass9_0();
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			base.Awake();
			CS_0024_003C_003E8__locals0.blockLookBefore = false;
			CS_0024_003C_003E8__locals0._003CAwake_003Eg__JoinMotion_007C0(_motions);
			CS_0024_003C_003E8__locals0._003CAwake_003Eg__JoinMotion_007C0(_landingMotions);
			if (_motions.components.Length != 0)
			{
				Motion obj = _motions.components[_motions.components.Length - 1];
				obj.onStart += delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.owner.movement.onGrounded += CS_0024_003C_003E8__locals0._003C_003E4__this.OnGrounded;
				};
				obj.onEnd += delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.owner.movement.onGrounded -= CS_0024_003C_003E8__locals0._003C_003E4__this.OnGrounded;
				};
				obj.onCancel += delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.owner.movement.onGrounded -= CS_0024_003C_003E8__locals0._003C_003E4__this.OnGrounded;
				};
			}
		}

		private void OnDisable()
		{
			base.owner.movement.onGrounded -= OnGrounded;
		}

		private void OnGrounded()
		{
			StopAllCoroutines();
			DoMotion(_landingMotions.components[0]);
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			for (int i = 0; i < motions.Length; i++)
			{
				motions[i].Initialize(this);
			}
		}

		public override bool TryStart()
		{
			if (!base.gameObject.activeSelf || !canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			if (base.owner.movement.isGrounded && _doLandingMotionIfGrounded)
			{
				_lookingDirection = base.owner.lookingDirection;
				DoAction(_landingMotions.components[0]);
			}
			else
			{
				DoAction(_motions.components[0]);
				StopAllCoroutines();
				StartCoroutine(CExtraGroundCheck());
			}
			return true;
		}

		private IEnumerator CExtraGroundCheck()
		{
			float speedMultiplier = GetSpeedMultiplier(_motions.components[0]);
			yield return Chronometer.global.WaitForSeconds(_motionTimeout / speedMultiplier);
			while (_motions.components.Any((Motion m) => m.running))
			{
				if (base.owner.movement.isGrounded)
				{
					DoMotion(_landingMotions.components[0]);
					break;
				}
				yield return null;
			}
		}
	}
}
