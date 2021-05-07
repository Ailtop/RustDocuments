using Characters.Actions;
using UnityEngine;

namespace Characters.Operations
{
	public class ReduceCooldownTime : CharacterOperation
	{
		[SerializeField]
		private float _amount;

		[SerializeField]
		private bool _skill;

		[SerializeField]
		private bool _dash;

		public override void Run(Character owner)
		{
			foreach (Action action in owner.actions)
			{
				if (action.cooldown.time != null)
				{
					bool num = _skill && action.type == Action.Type.Skill;
					bool flag = _dash && action.type == Action.Type.Dash;
					if (num || flag)
					{
						action.cooldown.time.remainTime -= _amount;
					}
				}
			}
		}
	}
}
