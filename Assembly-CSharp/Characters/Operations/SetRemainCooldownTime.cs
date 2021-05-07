using Characters.Actions;
using UnityEngine;

namespace Characters.Operations
{
	public class SetRemainCooldownTime : CharacterOperation
	{
		[SerializeField]
		private Action _action;

		[SerializeField]
		private float _time;

		public override void Run(Character owner)
		{
			_action.cooldown.time.remainTime = _time;
		}
	}
}
