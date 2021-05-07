using Level;
using Scenes;
using UnityEngine;

namespace Runnables
{
	public sealed class ChangeBackground : Runnable
	{
		[SerializeField]
		private ParallaxBackground _background;

		public override void Run()
		{
			Map instance = Map.Instance;
			Scene<GameBase>.instance.ChangeBackgroundWithFade(_background, instance.playerOrigin.y - instance.backgroundOrigin.y);
		}
	}
}
