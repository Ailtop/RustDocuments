using UnityEditor;
using UnityEngine;

namespace Characters.Actions
{
	public class RandomAction : Action
	{
		[SerializeField]
		[Subcomponent(typeof(Motion))]
		protected Motion.Subcomponents _motions;

		private int _indexToUse;

		public override Motion[] motions => _motions.components;

		public override bool canUse
		{
			get
			{
				if (base.cooldown.canUse && !_owner.stunedOrFreezed)
				{
					return PassAllConstraints(_motions.components[_indexToUse]);
				}
				return false;
			}
		}

		public override void Initialize(Character owner)
		{
			base.Initialize(owner);
			RandomizeIndex();
			for (int i = 0; i < motions.Length; i++)
			{
				motions[i].Initialize(this);
			}
		}

		private void RandomizeIndex()
		{
			_indexToUse = _motions.components.RandomIndex();
		}

		public override bool TryStart()
		{
			if (!base.cooldown.canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			DoAction(_motions.components[_indexToUse]);
			RandomizeIndex();
			return true;
		}
	}
}
