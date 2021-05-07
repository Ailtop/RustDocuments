using System.Collections;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public class GameFadeOut : CRunnable
	{
		[SerializeField]
		private float _speed;

		public override IEnumerator CRun()
		{
			GameBase instance = Scene<GameBase>.instance;
			instance.gameFadeInOut.Activate();
			yield return instance.gameFadeInOut.CFadeOut(_speed);
		}
	}
}
