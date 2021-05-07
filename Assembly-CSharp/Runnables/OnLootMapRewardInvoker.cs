using Level;
using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	public class OnLootMapRewardInvoker : MonoBehaviour
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		private void Start()
		{
			Map.Instance.mapReward.onLoot += Run;
		}

		private void Run()
		{
			if (_trigger.isSatisfied())
			{
				_execute.Run();
			}
		}
	}
}
