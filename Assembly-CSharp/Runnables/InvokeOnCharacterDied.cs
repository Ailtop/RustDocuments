using Characters;
using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	public class InvokeOnCharacterDied : MonoBehaviour
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		[SerializeField]
		private Character _character;

		private void Awake()
		{
			if (!(_character == null))
			{
				_character.health.onDied += OnDied;
			}
		}

		private void OnDied()
		{
			if (!(_character == null))
			{
				_character.health.onDied -= OnDied;
				if (_trigger.isSatisfied())
				{
					_execute.Run();
				}
			}
		}

		private void OnDestroy()
		{
			if (!(_character == null))
			{
				_character.health.onDied -= OnDied;
			}
		}
	}
}
