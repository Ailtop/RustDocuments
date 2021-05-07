using Data;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class SaveCutSceneData : Event
	{
		[SerializeField]
		private Key _key;

		public override void Run()
		{
			GameData.Progress.cutscene.SetData(_key, true);
		}
	}
}
