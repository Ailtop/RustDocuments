using Data;
using UnityEngine;

namespace SkulStories
{
	public sealed class SaveSkulStoryData : Event
	{
		[SerializeField]
		private Key _key;

		public override void Run()
		{
			GameData.Progress.skulstory.SetData(_key, true);
		}
	}
}
