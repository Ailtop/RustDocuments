using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class ChainAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _motions;

		private Character.LookingDirection _lookingDirection;

		public override Motion[] motions => _motions.components;

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
			base.Awake();
			bool flag = false;
			for (int i = 0; i < _motions.components.Length; i++)
			{
				Motion motion = _motions.components[i];
				if (motion.blockLook)
				{
					if (flag)
					{
						motion.onStart += delegate
						{
							base.owner.lookingDirection = _lookingDirection;
						};
					}
					motion.onStart += delegate
					{
						_lookingDirection = base.owner.lookingDirection;
					};
				}
				flag = motion.blockLook;
				if (i + 1 < motions.Length)
				{
					int cached = i + 1;
					motions[i].onEnd += delegate
					{
						DoMotion(motions[cached]);
					};
				}
			}
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
			DoAction(motions[0]);
			return true;
		}
	}
}
