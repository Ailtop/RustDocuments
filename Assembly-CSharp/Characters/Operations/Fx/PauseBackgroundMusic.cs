using Singletons;

namespace Characters.Operations.Fx
{
	public sealed class PauseBackgroundMusic : Operation
	{
		private bool _resumed;

		public override void Run()
		{
			_resumed = false;
			PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
		}

		public override void Stop()
		{
			if (!_resumed)
			{
				_resumed = true;
				PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(PersistentSingleton<SoundManager>.Instance.backgroundClip);
			}
		}

		private void OnDisable()
		{
			Stop();
		}
	}
}
