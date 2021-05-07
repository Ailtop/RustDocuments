using System;
using System.Collections;
using System.Linq;
using Characters;
using Characters.Actions;
using Characters.Gear.Weapons;
using Characters.Player;
using PhysicsUtils;
using UnityEngine;

namespace Level.Traps
{
	public class SpikeTrap : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private float _interval = 2f;

		[SerializeField]
		private Characters.Actions.Action _attackAction;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private readonly NonAllocOverlapper _overlapper = new NonAllocOverlapper(1);

		[Space]
		[SerializeField]
		private Weapon[] _weaponsToExclude;

		private string[] _weaponNamesToExclude;

		private void Awake()
		{
			_attackAction.Initialize(_character);
			_weaponNamesToExclude = _weaponsToExclude.Select((Weapon weapon) => weapon.name).ToArray();
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			while (true)
			{
				yield return Chronometer.global.WaitForSeconds(0.1f);
				if (FindPlayer())
				{
					_attackAction.TryStart();
					yield return _attackAction.CWaitForEndOfRunning();
					yield return Chronometer.global.WaitForSeconds(_interval);
				}
			}
		}

		private bool FindPlayer()
		{
			_range.enabled = true;
			_overlapper.contactFilter.SetLayerMask(512);
			_overlapper.OverlapCollider(_range);
			_range.enabled = false;
			Target component = _overlapper.GetComponent<Target>();
			if (component == null || component.character == null || !component.character.movement.isGrounded)
			{
				return false;
			}
			PlayerComponents playerComponents = component.character.playerComponents;
			if (playerComponents != null && _weaponNamesToExclude.Any((string name) => name.Equals(playerComponents.inventory.weapon.polymorphOrCurrent.name, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}
			return true;
		}
	}
}
