using Characters;
using Runnables.Triggers;
using Singletons;
using UnityEngine;

namespace Runnables
{
	public class InteractionInvoker : InteractiveObject
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		[SerializeField]
		private bool _once = true;

		private bool _used;

		public override void InteractWith(Character character)
		{
			if ((!_once || !_used) && _trigger.isSatisfied())
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
				_used = true;
				_execute.Run();
				if (_once)
				{
					Deactivate();
				}
			}
		}
	}
}
