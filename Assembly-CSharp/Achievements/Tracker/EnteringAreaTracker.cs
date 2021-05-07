using Characters;
using UnityEngine;

namespace Achievements.Tracker
{
	public class EnteringAreaTracker : MonoBehaviour
	{
		[SerializeField]
		private Collider2D _area;

		[SerializeField]
		private Achievement.Type _achievement;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (!(collision.GetComponent<Character>() == null))
			{
				Achievement.SetAchievement(_achievement);
			}
		}
	}
}
