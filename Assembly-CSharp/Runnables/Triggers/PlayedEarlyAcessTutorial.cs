using Data;
using UnityEngine;

namespace Runnables.Triggers
{
	public class PlayedEarlyAcessTutorial : Trigger
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
				return !GameData.Generic.playedTutorialDuringEA;
			}
			return GameData.Generic.playedTutorialDuringEA;
		}
	}
}
