using UnityEngine;

namespace Characters.Abilities
{
	public interface IAbilityInstance
	{
		Character owner { get; }

		IAbility ability { get; }

		float remainTime { get; set; }

		bool attached { get; }

		Sprite icon { get; }

		float iconFillAmount { get; }

		bool iconFillInversed { get; }

		bool iconFillFlipped { get; }

		int iconStacks { get; }

		bool expired { get; }

		void UpdateTime(float deltaTime);

		void Refresh();

		void Attach();

		void Detach();
	}
}
