using Data;
using UnityEngine;

namespace Runnables.Triggers
{
	public class PlayedTutorial : Trigger
	{
		private enum Compare
		{
			Played,
			NotPlayed
		}

		[SerializeField]
		private Compare _played;

		protected override bool Check()
		{
			if (_played != 0)
			{
				return !GameData.Generic.tutorial.isPlayed();
			}
			return GameData.Generic.tutorial.isPlayed();
		}
	}
}
