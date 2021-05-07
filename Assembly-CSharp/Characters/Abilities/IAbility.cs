namespace Characters.Abilities
{
	public interface IAbility
	{
		float duration { get; }

		int iconPriority { get; }

		bool removeOnSwapWeapon { get; }

		void Initialize();

		IAbilityInstance CreateInstance(Character owner);
	}
}
