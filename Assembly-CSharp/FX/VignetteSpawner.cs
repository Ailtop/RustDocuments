using Singletons;
using UnityEngine;

namespace FX
{
	public class VignetteSpawner : Singleton<VignetteSpawner>
	{
		private static class Assets
		{
			internal static readonly PoolObject effect = Resource.instance.vignetteEffect;
		}

		public void Spawn(Color startColor, Color endColor, Curve curve)
		{
			PoolObject poolObject = Assets.effect.Spawn();
			poolObject.transform.SetParent(base.transform, false);
			poolObject.GetComponent<Vignette>().Initialize(startColor, endColor, curve);
		}
	}
}
