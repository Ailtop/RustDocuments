using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public class GetBone : Sequence
	{
		[SerializeField]
		private Weapon _skul;

		private TutorialSkul _tutorialSkul;

		private WeaponInventory _inventory;

		private Character _player;

		private void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
		}

		public override IEnumerator CRun()
		{
			_inventory = _player.GetComponent<WeaponInventory>();
			_tutorialSkul = _inventory.polymorphOrCurrent.GetComponent<TutorialSkul>();
			_tutorialSkul.getBone.TryStart();
			yield return Chronometer.global.WaitForSeconds(0.5f);
			StartCoroutine(CGetWeapon());
		}

		private IEnumerator CGetWeapon()
		{
			while (_tutorialSkul.getBone.running)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.player.GetComponent<WeaponInventory>().ForceEquipAt(_skul.Instantiate(), 0);
			_inventory.polymorphOrCurrent.RemoveSkill(1);
		}
	}
}
