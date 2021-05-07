using System.Collections;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public class CoolDown : Condition
	{
		[SerializeField]
		private float _coolTime;

		private bool _canUse = true;

		protected override bool Check(AIController controller)
		{
			if (!_canUse)
			{
				return false;
			}
			StartCoroutine(CCoolDown(controller.character.chronometer.master));
			return true;
		}

		private IEnumerator CCoolDown(Chronometer chronometer)
		{
			_canUse = false;
			yield return chronometer.WaitForSeconds(_coolTime);
			_canUse = true;
		}
	}
}
