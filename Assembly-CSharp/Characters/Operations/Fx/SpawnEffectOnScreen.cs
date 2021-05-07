using FX;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class SpawnEffectOnScreen : Operation
	{
		[SerializeField]
		private Vector3 _positionOffset;

		[SerializeField]
		private EffectInfo _info;

		public override void Run()
		{
			Singleton<ScreenEffectSpawner>.Instance.Spawn(_info, _positionOffset);
		}

		public override void Stop()
		{
			_info.DespawnChildren();
		}
	}
}
