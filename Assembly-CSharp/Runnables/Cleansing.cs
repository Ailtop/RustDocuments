using Characters;
using Characters.Abilities;
using Characters.Abilities.Enemies;
using UnityEngine;

namespace Runnables
{
	public class Cleansing : Runnable
	{
		[SerializeField]
		private Target _target;

		public override void Run()
		{
			Character character = _target.character;
			IAbilityInstance instance = character.ability.GetInstance<CurseOfLight>();
			if (instance != null)
			{
				character.ability.Remove(instance.ability);
			}
		}
	}
}
