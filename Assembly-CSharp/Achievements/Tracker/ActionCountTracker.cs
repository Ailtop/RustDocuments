using System.Collections;
using UnityEngine;

namespace Achievements.Tracker
{
	public class ActionCountTracker : MonoBehaviour
	{
		[SerializeField]
		private Achievement.Type _achievement;

		[SerializeField]
		private int _count;

		[SerializeField]
		private int _timeout;

		private int _currentCount;

		public void AddCount()
		{
			_currentCount++;
			if (_timeout > 0)
			{
				StopAllCoroutines();
				StartCoroutine(CTimeout());
			}
			if (_currentCount >= _count)
			{
				Achievement.SetAchievement(_achievement);
			}
		}

		private IEnumerator CTimeout()
		{
			yield return Chronometer.global.WaitForSeconds(_timeout);
			_currentCount = 0;
		}
	}
}
