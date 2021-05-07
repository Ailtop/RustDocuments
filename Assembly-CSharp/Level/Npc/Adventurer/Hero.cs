using System.Collections;
using Characters;
using Characters.Actions;
using Characters.AI.Adventurer;
using Characters.Controllers;
using CutScenes;
using Data;
using FX;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.Adventurer
{
	public class Hero : MonoBehaviour
	{
		private static readonly string _nameKey = "CutScene/name/RookieHero";

		private static readonly string _textKey = "CutScene/AdventurerIntro/RookieHero/0";

		[SerializeField]
		private MusicInfo _musicInfo;

		[SerializeField]
		private AdventurerHero _hero;

		[SerializeField]
		private Action[] _actions;

		private string[] _texts;

		private void Start()
		{
			_texts = Lingua.GetLocalizedStringArray(_textKey);
			if (!GameData.Progress.cutscene.GetData(CutScenes.Key.rookieHero))
			{
				_hero.character.ForceToLookAt(Character.LookingDirection.Right);
			}
		}

		private void OnDestroy()
		{
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
			LetterBox.instance.Disappear();
			Scene<GameBase>.instance.cameraController.StartTrack(Singleton<Service>.Instance.levelManager.player.transform);
		}

		private IEnumerator CSmallTalk()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			yield return Chronometer.global.WaitForSeconds(3f);
			for (int i = 0; i < _texts.Length; i++)
			{
				_actions[i]?.TryStart();
				yield return npcConversation.CConversation(_texts[i]);
			}
			Deactivate();
		}

		public void Activate()
		{
			StartCoroutine(CActivate());
		}

		public IEnumerator CActivate()
		{
			PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
			_hero.character.invulnerable.Attach(this);
			PlayerInput.blocked.Attach(this);
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = false;
			yield return Scene<GameBase>.instance.uiManager.letterBox.CAppear();
			yield return CSmallTalk();
		}

		private void Deactivate()
		{
			_hero.character.invulnerable.Detach(this);
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			StartCoroutine(CDeactivate());
			_hero.CombatTutorial();
			GameData.Progress.cutscene.SetData(CutScenes.Key.rookieHero, true);
		}

		private IEnumerator CDeactivate()
		{
			PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_musicInfo);
			yield return Scene<GameBase>.instance.uiManager.letterBox.CDisappear();
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
		}
	}
}
