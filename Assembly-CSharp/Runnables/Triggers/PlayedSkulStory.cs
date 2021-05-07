using Data;
using SkulStories;
using UnityEngine;

namespace Runnables.Triggers
{
	public class PlayedSkulStory : Trigger
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
				return !GameData.Progress.skulstory.GetData(_key);
			}
			return GameData.Progress.skulstory.GetData(_key);
		}
	}
}
