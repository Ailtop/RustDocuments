using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class Heal : CharacterHitOperation
	{
		private enum Type
		{
			Percent,
			Constnat
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private CustomFloat _amount;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character character)
		{
			if (!(character == null))
			{
				character.health.Heal(GetAmount(character.health));
			}
		}

		private double GetAmount(Health health)
		{
			switch (_type)
			{
			case Type.Percent:
				return (double)_amount.value * health.maximumHealth * 0.0099999997764825821;
			case Type.Constnat:
				return _amount.value;
			default:
				return 0.0;
			}
		}
	}
}
