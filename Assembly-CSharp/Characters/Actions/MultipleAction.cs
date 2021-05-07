using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class MultipleAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _motions;

		public override Motion[] motions => _motions.components;

		public override bool canUse
		{
			get
			{
				if (!base.cooldown.canUse || _owner.stunedOrFreezed)
				{
					return false;
				}
				for (int i = 0; i < _motions.components.Length; i++)
				{
					if (PassAllConstraints(_motions.components[i]))
					{
						return true;
					}
				}
				return true;
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
			if (!base.cooldown.canUse || _owner.stunedOrFreezed)
			{
				return false;
			}
			for (int i = 0; i < _motions.components.Length; i++)
			{
				if (PassAllConstraints(_motions.components[i]) && ConsumeCooldownIfNeeded())
				{
					DoAction(_motions.components[i]);
					return true;
				}
			}
			return false;
		}
	}
}
