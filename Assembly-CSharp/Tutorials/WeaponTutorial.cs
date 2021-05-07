using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using Scenes;
using UnityEngine;

namespace Tutorials
{
	public class WeaponTutorial : Tutorial
	{
		[SerializeField]
		private Weapon _skeletonBossWeapon;

		[SerializeField]
		private Transform _conversationPoint;

		[SerializeField]
		private GameObject _head;

		protected override IEnumerator Process()
		{
			yield return MoveTo(_conversationPoint.position);
			_player.lookingDirection = Character.LookingDirection.Right;
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			WeaponInventory inventory = _player.playerComponents.inventory.weapon;
			Skul skul = inventory.polymorphOrCurrent.GetComponent<Skul>();
			_head.SetActive(false);
			skul.getSkul.TryStart();
			while (skul.getSkul.running)
			{
				yield return null;
			}
			inventory.ForceEquip(_skeletonBossWeapon.Instantiate());
			Deactivate();
		}
	}
}
