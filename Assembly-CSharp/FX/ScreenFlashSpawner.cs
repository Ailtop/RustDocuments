using Singletons;

namespace FX
{
	public class ScreenFlashSpawner : Singleton<ScreenFlashSpawner>
	{
		private static class Assets
		{
			internal static readonly PoolObject effect = Resource.instance.screenFlashEffect;
		}

		public ScreenFlash Spawn(ScreenFlash.Info info)
		{
			PoolObject poolObject = Assets.effect.Spawn();
			poolObject.transform.parent = base.transform;
			ScreenFlash component = poolObject.GetComponent<ScreenFlash>();
			component.Play(info);
			return component;
		}
	}
}
