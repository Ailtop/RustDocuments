using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class CoolTime : Decorator
	{
		[SerializeField]
		private float _value;

		private bool _canRun = true;

		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (!_canRun)
			{
				base.result = Result.Fail;
				yield break;
			}
			StartCoroutine(CCooldown(controller.character.chronometer.master));
			yield return _behaviour.CRun(controller);
			base.result = Result.Success;
		}

		private IEnumerator CCooldown(Chronometer chronometer)
		{
			_canRun = false;
			yield return chronometer.WaitForSeconds(_value);
			_canRun = true;
		}
	}
}
