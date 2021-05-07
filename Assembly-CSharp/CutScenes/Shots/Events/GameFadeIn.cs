using Scenes;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class GameFadeIn : Event
	{
		[SerializeField]
		private float _speed;

		public override void Run()
		{
			GameBase instance = Scene<GameBase>.instance;
			StartCoroutine(instance.gameFadeInOut.CFadeIn(_speed));
		}
	}
}
