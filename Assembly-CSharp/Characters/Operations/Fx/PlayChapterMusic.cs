using Services;
using Singletons;

namespace Characters.Operations.Fx
{
	public sealed class PlayChapterMusic : Operation
	{
		public override void Run()
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(Singleton<Service>.Instance.levelManager.currentChapter.currentStage.music);
		}
	}
}
