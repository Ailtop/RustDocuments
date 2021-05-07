using Characters.Gear.Weapons;
using Services;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	public class Berserker2PassiveComponent : AbilityComponent<Berserker2Passive>
	{
		[SerializeField]
		private Weapon _polymorphWeapon;

		private void Awake()
		{
			_polymorphWeapon = Object.Instantiate(_polymorphWeapon);
			_polymorphWeapon.gameObject.SetActive(false);
		}

		public override void Initialize()
		{
			base.Initialize();
			_ability._polymorphWeapon = _polymorphWeapon;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Object.Destroy(_polymorphWeapon.gameObject);
			}
		}
	}
}
