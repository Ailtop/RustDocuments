using CutScenes;
using Data;
using UnityEngine;

namespace Runnables.Triggers
{
	public class PlayedCutScene : Trigger
	{
		private enum Compare
		{
			Played,
			NotPlayed
		}

		[SerializeField]
		private Key _key;

		[SerializeField]
		private Compare _played;

		protected override bool Check()
		{
			if (_played != 0)
			{
				return !GameData.Progress.cutscene.GetData(_key);
			}
			return GameData.Progress.cutscene.GetData(_key);
		}
	}
}
