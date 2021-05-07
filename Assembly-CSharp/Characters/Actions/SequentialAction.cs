using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class SequentialAction : Action
	{
		[SerializeField]
		private bool _shuffle = true;

		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _motions;

		private int _motionIndex;

		public override Motion[] motions => _motions.components;

		public Motion currentMotion => _motions.components[_motionIndex];

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(currentMotion);
				}
				return false;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			if (_shuffle)
			{
				motions.PseudoShuffle();
			}
			for (int i = 0; i < motions.Length; i++)
			{
				motions[i].Initialize(this);
			}
		}

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_motions.components[_motionIndex]);
			_motionIndex = (_motionIndex + 1) % _motions.components.Length;
			return true;
		}
	}
}
