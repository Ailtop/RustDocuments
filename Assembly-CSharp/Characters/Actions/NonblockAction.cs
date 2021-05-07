using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class NonblockAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion _motion;

		public override Motion[] motions => new Motion[1] { _motion };

		public Motion motion => _motion;

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
			_motion.onEnd += delegate
			{
				_onEnd?.Invoke();
			};
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			_motion.Initialize(this);
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoActionNonBlock(motion);
			return true;
		}

		public override bool Process()
		{
			base.Process();
			return false;
		}
	}
}
