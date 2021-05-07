using System.Collections;
using EndingCredit;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public class ShowEndingCredit : CRunnable
	{
		[SerializeField]
		private float _delay;

		private CreditRoll _creditRoll;

		public override IEnumerator CRun()
		{
			_creditRoll = Scene<GameBase>.instance.uiManager.endingCredit;
			_creditRoll.Show();
			yield return Chronometer.global.WaitForSeconds(_delay);
			StartCoroutine(_creditRoll.CRun());
		}

		private void OnDisable()
		{
			if (_creditRoll != null)
			{
				_creditRoll.Hide();
			}
		}
	}
}
