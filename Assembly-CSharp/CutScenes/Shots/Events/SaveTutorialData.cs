using Data;

namespace CutScenes.Shots.Events
{
	public class SaveTutorialData : Event
	{
		public override void Run()
		{
			GameData.Generic.tutorial.End();
		}
	}
}
