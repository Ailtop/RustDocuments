using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using FX;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Tutorials
{
	public class AttackTutorial : Tutorial
	{
		[SerializeField]
		private Weapon _skul;

		[SerializeField]
		private Animator _skulAnimator;

		[SerializeField]
		private Transform _conversationPoint;

		[SerializeField]
		private SoundInfo _giveBoneSoundInfo;

		protected override IEnumerator Process()
		{
			_player.CancelAction();
			yield return MoveTo(_conversationPoint.position);
			_player.lookingDirection = Character.LookingDirection.Right;
			for (int i = 0; i < 5; i++)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_giveBoneSoundInfo, base.transform.position);
			}
			_skulAnimator.Play("GiveWeapon");
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			WeaponInventory inventory = _player.playerComponents.inventory.weapon;
			TutorialSkul tutorialSkul = inventory.polymorphOrCurrent.GetComponent<TutorialSkul>();
			tutorialSkul.getBone.TryStart();
			yield return Chronometer.global.WaitForSeconds(0.5f);
			_skulAnimator.Play("Dead");
			while (tutorialSkul.getBone.running)
			{
				yield return null;
			}
			_skulAnimator.Play("DeadStop");
			inventory.LoseAll();
			Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon.ForceEquipAt(_skul.Instantiate(), 0);
			inventory.polymorphOrCurrent.RemoveSkill(1);
			Deactivate();
			yield return Chronometer.global.WaitForSeconds(1.7f);
		}
	}
}
