using System.Collections;
using Characters.AI;
using Characters.AI.Mercenarys;
using Scenes;
using UI;
using UnityEngine;

namespace Tutorials
{
	public class BattleTutorial : Tutorial
	{
		[SerializeField]
		private AIController _ogre;

		[SerializeField]
		private Transform _trackPoint;

		[SerializeField]
		private BossNameDisplay _bossNameDisplay;

		[SerializeField]
		private Mercenary _witch;

		private void Awake()
		{
			_ogre.character.invulnerable.Attach(this);
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
		}

		public override void Activate()
		{
			base.Activate();
			Scene<GameBase>.instance.cameraController.StartTrack(_trackPoint);
		}

		public override void Deactivate()
		{
			base.Deactivate();
			Scene<GameBase>.instance.cameraController.StartTrack(_player.transform);
		}

		protected override IEnumerator Process()
		{
			UIManager uiManager = Scene<GameBase>.instance.uiManager;
			uiManager.npcConversation.Done();
			uiManager.headupDisplay.bossHealthBar.CloseAll();
			uiManager.headupDisplay.bossHealthBar.Open(BossHealthbarController.Type.Tutorial, _ogre.character);
			_bossNameDisplay.ShowAppearanceText();
			yield return Chronometer.global.WaitForSeconds(1.7f);
			_bossNameDisplay.HideAppearanceText();
			Deactivate();
			_witch.follow = false;
			_ogre.character.invulnerable.Detach(this);
			_ogre.target = _player;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_ogre.character.invulnerable.Detach(this);
		}
	}
}
