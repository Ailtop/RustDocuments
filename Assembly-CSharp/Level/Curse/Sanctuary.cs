using Characters;
using Characters.Abilities;
using Characters.Abilities.Enemies;

namespace Level.Curse
{
	public class Sanctuary : InteractiveObject, ISanctuary
	{
		public override void InteractWith(Character character)
		{
			RemoveCurse(character);
		}

		public void RemoveCurse(Character character)
		{
			IAbilityInstance instance = character.ability.GetInstance<CurseOfLight>();
			if (instance != null)
			{
				character.ability.Remove(instance.ability);
			}
		}
	}
}
