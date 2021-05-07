using Characters;
using Characters.Player;

namespace FX
{
	public class PlayerSpriteEffectStack : SpriteEffectStack, ISpriteEffectStack
	{
		private WeaponInventory _weaponInventory;

		protected override void Awake()
		{
			base.Awake();
			Character component = GetComponent<Character>();
			_chronometer = component.chronometer.animation;
			_weaponInventory = component.playerComponents.inventory.weapon;
		}

		protected override void LateUpdate()
		{
			_spriteRenderer = _weaponInventory.polymorphOrCurrent.characterAnimation.spriteRenderer;
			base.LateUpdate();
		}
	}
}
