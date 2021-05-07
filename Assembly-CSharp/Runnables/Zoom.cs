using Scenes;
using UnityEngine;

namespace Runnables
{
	public sealed class Zoom : Runnable
	{
		[SerializeField]
		[Range(0f, 10f)]
		private float _percent = 1f;

		[SerializeField]
		private float _speed = 1f;

		public override void Run()
		{
			Scene<GameBase>.instance.cameraController.Zoom(_percent, _speed);
		}
	}
}
