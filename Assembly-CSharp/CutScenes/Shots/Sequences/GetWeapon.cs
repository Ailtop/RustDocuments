using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class GetWeapon : Sequence
	{
		[SerializeField]
		private Weapon _weapon;

		private Character _player;

		private void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
		}

		public override IEnumerator CRun()
		{
			WeaponInventory inventory = _player.GetComponent<WeaponInventory>();
			Skul skul = inventory.polymorphOrCurrent.GetComponent<Skul>();
			skul.getSkul.TryStart();
			yield return null;
			StartCoroutine(CGet(skul, inventory));
		}

		private IEnumerator CGet(Skul skul, WeaponInventory inventory)
		{
			while (skul.getSkul.running)
			{
				yield return null;
			}
			inventory.ForceEquip(_weapon.Instantiate());
		}
	}
}
