using Characters;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class CameraMoveTo : Event
	{
		[SerializeField]
		private Transform _trackPoint;

		public override void Run()
		{
			Scene<GameBase>.instance.cameraController.StartTrack(_trackPoint);
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Character player = Singleton<Service>.Instance.levelManager.player;
				if (!(player == null))
				{
					Scene<GameBase>.instance.cameraController.StartTrack(player.transform);
				}
			}
		}
	}
}
