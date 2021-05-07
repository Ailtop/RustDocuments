using Characters.Projectiles;
using Level.Traps;
using UnityEngine;

namespace Characters
{
	public struct Attacker
	{
		public readonly Character character;

		public readonly Projectile projectile;

		public readonly Trap trap;

		public readonly CharacterStatus characterStatus;

		public readonly Transform transform;

		public static implicit operator Attacker(Character character)
		{
			return new Attacker(character);
		}

		public static implicit operator Attacker(Projectile projectile)
		{
			return new Attacker(projectile);
		}

		public static implicit operator Attacker(Trap trap)
		{
			return new Attacker(trap);
		}

		public static implicit operator Attacker(CharacterStatus characterStatus)
		{
			return new Attacker(characterStatus);
		}

		public Attacker(Character character)
		{
			this.character = character;
			projectile = null;
			trap = null;
			characterStatus = null;
			transform = character.transform;
		}

		public Attacker(Projectile projectile)
		{
			character = null;
			this.projectile = projectile;
			trap = null;
			characterStatus = null;
			transform = projectile.transform;
		}

		public Attacker(Trap trap)
		{
			character = null;
			projectile = null;
			this.trap = trap;
			characterStatus = null;
			transform = trap.transform;
		}

		public Attacker(CharacterStatus characterStatus)
		{
			character = null;
			projectile = null;
			trap = null;
			this.characterStatus = characterStatus;
			transform = characterStatus.transform;
		}
	}
}
