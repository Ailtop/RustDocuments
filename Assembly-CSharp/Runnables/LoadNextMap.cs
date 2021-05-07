using Services;
using Singletons;

namespace Runnables
{
	public sealed class LoadNextMap : Runnable
	{
		public override void Run()
		{
			Singleton<Service>.Instance.levelManager.LoadNextMap();
		}
	}
}
