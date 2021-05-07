using Runnables;
using Scenes;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class CameraMoveToCharacter : Event
	{
		[SerializeField]
		private Target _target;

		public override void Run()
		{
			Scene<GameBase>.instance.cameraController.StartTrack(_target.character.transform);
		}
	}
}
