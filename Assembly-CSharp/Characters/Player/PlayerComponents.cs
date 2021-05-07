using System;

namespace Characters.Player
{
	public class PlayerComponents : IDisposable
	{
		public readonly Character character;

		public readonly Inventory inventory;

		public readonly CombatDetector combatDetector;

		public readonly MinionLeader minionLeader;

		public readonly Visibility visibility;

		public PlayerComponents(Character character)
		{
			this.character = character;
			inventory = new Inventory(character);
			combatDetector = new CombatDetector(character);
			minionLeader = new MinionLeader(character);
			visibility = character.gameObject.AddComponent<Visibility>();
		}

		public void Dispose()
		{
			minionLeader.Dispose();
		}

		public void Initialize()
		{
			inventory.Initialize();
		}

		public void Update(float deltaTime)
		{
			combatDetector.Update(deltaTime);
		}
	}
}
