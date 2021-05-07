using Level;
using Scenes;
using UnityEngine;

namespace Characters.Operations
{
	public sealed class SetBackground : Operation
	{
		[SerializeField]
		private ParallaxBackground _background;

		public override void Run()
		{
			Map instance = Map.Instance;
			Scene<GameBase>.instance.SetBackground(_background, instance.playerOrigin.y - instance.backgroundOrigin.y);
		}
	}
}
