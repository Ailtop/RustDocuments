using System;
using Characters;
using Characters.Gear.Weapons;
using UnityEngine;

namespace Level
{
	public class DroppedWeapon : InteractiveObject
	{
		[NonSerialized]
		public Weapon weapon;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		[GetComponent]
		private DropMovement _dropMovement;

		protected override void Awake()
		{
			base.Awake();
			_dropMovement.onGround += Activate;
		}

		private void OnEnable()
		{
			Deactivate();
		}

		public override void InteractWith(Character character)
		{
			character.playerComponents.inventory.weapon.Equip(weapon);
			_effect.Spawn(base.transform.position);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
