using System.Collections;
using Characters.Gear.Weapons;
using Characters.Player;
using Data;
using Scenes;

namespace Tutorials
{
	public class FinalTutorial : Tutorial
	{
		private bool active
		{
			get
			{
				if (!GameData.Generic.playedTutorialDuringEA)
				{
					return GameData.Generic.tutorial.isPlaying();
				}
				return false;
			}
		}

		private void Awake()
		{
			if (!active)
			{
				base.gameObject.SetActive(false);
			}
		}

		public override void Activate()
		{
			if (active)
			{
				base.Activate();
			}
		}

		protected override IEnumerator Process()
		{
			yield return Chronometer.global.WaitForSeconds(2f);
			for (int i = 3; i < 12; i++)
			{
			}
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			WeaponInventory weapon = _player.playerComponents.inventory.weapon;
			Skul skul = weapon.polymorphOrCurrent.GetComponent<Skul>();
			skul.getScroll.TryStart();
			while (skul.getScroll.running)
			{
				yield return null;
			}
			for (int j = 12; j < 15; j++)
			{
				Deactivate();
			}
			GameData.Generic.tutorial.End();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}
	}
}
