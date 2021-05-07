using Scenes;

namespace CutScenes.Shots.Events
{
	public sealed class RenderEndingCut : Event
	{
		public override void Run()
		{
			Scene<GameBase>.instance.cameraController.RenderEndingScene();
		}
	}
}
