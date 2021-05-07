using EndingCredit;
using UnityEngine;

namespace UI.Pause
{
	public class CreditExit : PauseEvent
	{
		[SerializeField]
		private CreditRoll _endingCredit;

		private bool _activated;

		public override void Invoke()
		{
			if (!_activated)
			{
				_activated = true;
				StartCoroutine(_endingCredit.CLoadScene());
			}
		}
	}
}
