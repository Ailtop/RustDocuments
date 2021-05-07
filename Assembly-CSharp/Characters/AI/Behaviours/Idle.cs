using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Idle : Behaviour
	{
		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _duration;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			float duration = Random.Range(_duration.x, _duration.y);
			float elapsed = 0f;
			if (duration > 0f)
			{
				while (base.result == Result.Doing)
				{
					yield return null;
					elapsed += controller.character.chronometer.master.deltaTime;
					if (elapsed > duration)
					{
						break;
					}
				}
			}
			base.result = Result.Success;
		}
	}
}
