using System;

namespace Characters
{
	public interface ITrigger
	{
		float cooldownTime { get; }

		float remainCooldownTime { get; }

		event Action onTriggered;

		void Attach(Character character);

		void Detach();

		void UpdateTime(float deltaTime);
	}
}
