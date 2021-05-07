using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class ScreenFlash : CharacterOperation
	{
		[SerializeField]
		private FX.ScreenFlash.Info _info;

		private FX.ScreenFlash _instance;

		public override void Run(Character owner)
		{
			_instance = Singleton<ScreenFlashSpawner>.Instance.Spawn(_info);
		}

		public override void Stop()
		{
			if (!Service.quitting && _instance != null)
			{
				_instance.FadeOut();
			}
		}
	}
}
