using Level;
using Scenes;
using Services;
using Singletons;

namespace Runnables
{
	public class ShowStageInfo : Runnable
	{
		public override void Run()
		{
			Chapter currentChapter = Singleton<Service>.Instance.levelManager.currentChapter;
			Scene<GameBase>.instance.uiManager.stageName.Show(currentChapter.chapterName, currentChapter.stageTag, currentChapter.stageName);
		}
	}
}
