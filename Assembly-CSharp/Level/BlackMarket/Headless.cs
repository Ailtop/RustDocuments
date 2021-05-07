using System;
using System.Collections;
using System.Linq;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using Level.Npc;
using Services;
using Singletons;
using UnityEngine;

namespace Level.BlackMarket
{
	public class Headless : Npc
	{
		protected static readonly int _activateWithHeadHash = Animator.StringToHash("ActivateWithHead");

		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private SpriteRenderer _headSlot;

		[SerializeField]
		private Animator _headSlotAnimator;

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private GameObject _talk;

		private Weapon _displayedGear;

		public string submitLine => Lingua.GetLocalizedStringArray("npc/headless/submit/line").Random();

		private void OnDisable()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (player != null)
			{
				player.playerComponents.inventory.weapon.onChanged -= OnWeaponChanged;
			}
		}

		private void OnWeaponChanged(Weapon old, Weapon @new)
		{
			if (!(@new != _displayedGear))
			{
				@new.destructible = true;
				_lineText.Run(submitLine);
				if (old == null)
				{
					WeaponInventory weapon = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
					old = weapon.next;
					weapon.Unequip(old);
				}
				_headSlot.enabled = true;
				_headSlot.sprite = old.dropped.spriteRenderer.sprite;
				_animator.Play(_activateWithHeadHash, 0, 0f);
				_headSlotAnimator.Play(Npc._activateHash, 0, 0f);
				old.destructible = false;
				UnityEngine.Object.Destroy(old.gameObject);
			}
		}

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.activateHeadless)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		private IEnumerator CDropGear()
		{
			Rarity rarity = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.headlessHeadPossibilities.Evaluate();
			Resource.WeaponReference weaponToTake = Singleton<Service>.Instance.gearManager.GetWeaponToTake(rarity);
			WeaponInventory weapon2 = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
			while (weapon2.weapons.Where((Weapon weapon) => weapon != null).Count() <= 1 && weaponToTake.name.Equals("BombSkul", StringComparison.OrdinalIgnoreCase))
			{
				weaponToTake = Singleton<Service>.Instance.gearManager.GetWeaponToTake(rarity);
			}
			Resource.Request<Weapon> request = weaponToTake.LoadAsync();
			while (!request.isDone)
			{
				yield return null;
			}
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			_displayedGear = levelManager.DropWeapon(request.asset, _slot.position);
			_displayedGear.destructible = false;
			_displayedGear.dropped.dropMovement.Stop();
			_displayedGear.dropped.dropMovement.Float();
		}

		protected override void OnActivate()
		{
			_lineText.gameObject.SetActive(true);
			_talk.SetActive(true);
			_headSlot.enabled = false;
			StartCoroutine(CDropGear());
			Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon.onChanged += OnWeaponChanged;
		}

		protected override void OnDeactivate()
		{
			_lineText.gameObject.SetActive(false);
			_headSlot.enabled = false;
		}
	}
}
