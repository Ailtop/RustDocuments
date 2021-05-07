using Characters.Actions.Cooldowns;
using Characters.Gear.Weapons;
using Characters.Operations;
using Characters.Player;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class CooldownConstraint : Constraint
	{
		[SerializeField]
		[Cooldown.Subcomponent]
		private Cooldown _cooldown;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operationsWhenReady;

		private Weapon _weapon;

		private WeaponInventory _inventory;

		public bool canUse => _cooldown.canUse;

		public override void Initilaize(Action action)
		{
			base.Initilaize(action);
			_cooldown.Initialize(action.owner);
			_inventory = action.owner.GetComponent<WeaponInventory>();
			if (_inventory == null)
			{
				_cooldown.onReady += RunOperationsWhenReady;
				return;
			}
			_weapon = GetComponentInParent<Weapon>();
			_cooldown.onReady += RunOperationsWhenReadyWithCheckWeapon;
		}

		private void RunOperationsWhenReady()
		{
			StartCoroutine(_operationsWhenReady.CRun(_action.owner));
		}

		private void RunOperationsWhenReadyWithCheckWeapon()
		{
			if (_inventory.polymorphOrCurrent == _weapon)
			{
				StartCoroutine(_operationsWhenReady.CRun(_action.owner));
			}
		}

		private void OnDisable()
		{
			_cooldown.onReady -= RunOperationsWhenReady;
			_cooldown.onReady -= RunOperationsWhenReadyWithCheckWeapon;
		}

		public override bool Pass()
		{
			return _cooldown.canUse;
		}

		public override void Consume()
		{
			_cooldown.Consume();
		}
	}
}
