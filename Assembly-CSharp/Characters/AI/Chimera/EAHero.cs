using System.Collections;
using Characters.Actions;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class EAHero : MonoBehaviour
	{
		private enum Text
		{
			LandingFreeze_01,
			Landing_01,
			Standing_01,
			Standing_02,
			Standing_03,
			Standing_04,
			Fly_01
		}

		private static readonly string _nameKey = "CutScene/name/FirstHero";

		private static readonly string _textKey = "CutScene/EAEnding/FirstHero/0";

		[SerializeField]
		private Character _character;

		[SerializeField]
		private float _teleportDelay;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Transform _teleportDestination;

		[SerializeField]
		private Action _landingFreeze;

		[SerializeField]
		private Action _landing;

		[SerializeField]
		private Action _stand;

		[SerializeField]
		private Action _fly;

		[SerializeField]
		private Action _teleportStart;

		[SerializeField]
		private Action _teleportEnd;

		[SerializeField]
		private Action _attack;

		private string[] _texts;

		private void OnEnable()
		{
			_texts = Lingua.GetLocalizedStringArray(_textKey);
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Scene<GameBase>.instance.uiManager.npcConversation.Done();
				LetterBox.instance.Disappear();
				Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
				Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
				if (!(Scene<GameBase>.instance.cameraController == null) && !(Singleton<Service>.Instance.levelManager.player == null))
				{
					Scene<GameBase>.instance.cameraController.StartTrack(Singleton<Service>.Instance.levelManager.player.transform);
				}
			}
		}

		public IEnumerator Landing()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_landingFreeze.TryStart();
			yield return npcConversation.CConversation(_texts[0]);
			_landing.TryStart();
			yield return npcConversation.CConversation(_texts[1]);
		}

		public IEnumerator Standing()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_stand.TryStart();
			yield return npcConversation.CConversation(_texts[2]);
			yield return npcConversation.CConversation(_texts[3]);
			yield return npcConversation.CConversation(_texts[4]);
			yield return npcConversation.CConversation(_texts[5]);
		}

		public IEnumerator Teleport()
		{
			SetTeleportDestinationToPlayer();
			_teleportStart.TryStart();
			_spriteRenderer.enabled = false;
			while (_teleportStart.running)
			{
				yield return null;
			}
			yield return Chronometer.global.WaitForSeconds(_teleportDelay);
			_character.ForceToLookAt(Character.LookingDirection.Right);
			_teleportEnd.TryStart();
			_spriteRenderer.enabled = true;
			while (_teleportEnd.running)
			{
				yield return null;
			}
		}

		public IEnumerator FlyIdle()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_fly.TryStart();
			yield return npcConversation.CConversation(_texts[6]);
			npcConversation.Done();
		}

		public IEnumerator Attack()
		{
			_attack.TryStart();
			yield return null;
		}

		public void SetTeleportDestinationToPlayer()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			_teleportDestination.position = player.transform.position;
		}
	}
}
