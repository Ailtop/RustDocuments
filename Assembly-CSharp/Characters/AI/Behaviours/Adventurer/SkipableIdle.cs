using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer
{
	public class SkipableIdle : Behaviour
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _skipChance;

		[SerializeField]
		[Range(0f, 1f)]
		private float _potionChance;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _duration;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (MMMaths.Chance(_skipChance))
			{
				base.result = Result.Done;
				yield break;
			}
			if (MMMaths.Chance(_potionChance))
			{
				base.result = Result.Fail;
				yield break;
			}
			float duration = Random.Range(_duration.x, _duration.y);
			float elapsed = 0f;
			while (base.result == Result.Doing)
			{
				yield return null;
				elapsed += controller.character.chronometer.master.deltaTime;
				if (elapsed > duration)
				{
					break;
				}
			}
			base.result = Result.Success;
		}
	}
}
