using System.Collections;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public class GameFadeIn : CRunnable
	{
		[SerializeField]
		private float _speed;

		public override IEnumerator CRun()
		{
			GameBase instance = Scene<GameBase>.instance;
			yield return instance.gameFadeInOut.CFadeIn(_speed);
		}
	}
}
