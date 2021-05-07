using Services;
using Singletons;
using UnityEngine;

namespace Runnables
{
	public class SetPlayerVisible : Runnable
	{
		[SerializeField]
		private bool _visible;

		public override void Run()
		{
			Singleton<Service>.Instance.levelManager.player.playerComponents.visibility.SetVisible(_visible);
		}
	}
}
