using System.Collections;
using UnityEngine;

namespace Achievements.Tracker
{
	public class StayingTimeTracker : MonoBehaviour
	{
		[SerializeField]
		private float _time;

		[SerializeField]
		private Achievement.Type _achievement;

		private IEnumerator Start()
		{
			yield return Chronometer.global.WaitForSeconds(_time);
			Achievement.SetAchievement(_achievement);
		}
	}
}
