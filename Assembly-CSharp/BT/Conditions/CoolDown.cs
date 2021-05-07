using System.Collections;
using UnityEngine;

namespace BT.Conditions
{
	public class CoolDown : Condition
	{
		[SerializeField]
		private float _time;

		private bool _success;

		private void OnEnable()
		{
			_success = true;
		}

		protected override bool Check(Context context)
		{
			if (_success)
			{
				StartCoroutine(CCoolDown());
				return true;
			}
			return false;
		}

		private IEnumerator CCoolDown()
		{
			_success = false;
			yield return Chronometer.global.WaitForSeconds(_time);
			_success = true;
		}
	}
}
