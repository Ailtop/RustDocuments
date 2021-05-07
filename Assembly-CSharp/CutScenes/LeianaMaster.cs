using System.Collections;
using Characters.Actions;
using Characters.Controllers;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes
{
	public class LeianaMaster : MonoBehaviour
	{
		private enum Animation
		{
			Arrival,
			Talk1,
			Laugh,
			Pose_Loop,
			Pose_Freeze,
			Talk2,
			Idle,
			Dead,
			DeadFreeze,
			BackIdle,
			Outro
		}

		private enum IntroText
		{
			Talk1_01,
			Laugh_01,
			Talk1_02,
			Talk1_03,
			Talk2_01,
			Talk2_02,
			Idle_01,
			Idle_02
		}

		private enum OutroText
		{
			Outro_01,
			Outro_02,
			Back_01,
			Back_02
		}

		private static readonly string _nameKey = "CutScene/name/GrandMaster";

		private static readonly string _textKey = "CutScene/Ch2BossIntro/GrandMaster/0";

		private static readonly string _outroTextkey = "CutScene/Ch2BossOutro/GrandMaster/0";

		[SerializeField]
		private Action[] _actions;

		private string[] _texts;

		private string[] _outroTexts;

		private void Start()
		{
			_texts = Lingua.GetLocalizedStringArray(_textKey);
			_outroTexts = Lingua.GetLocalizedStringArray(_outroTextkey);
		}

		private void OnDestroy()
		{
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.headupDisplay.bossHealthBar.CloseAll();
			Scene<GameBase>.instance.uiManager.headupDisplay.visible = true;
		}

		public IEnumerator CSmallTalk()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_actions[0].TryStart();
			while (_actions[0].running)
			{
				yield return null;
			}
			_actions[1].TryStart();
			yield return npcConversation.CConversation(_texts[0]);
			_actions[2].TryStart();
			yield return npcConversation.CConversation(_texts[1]);
			_actions[1].TryStart();
			yield return npcConversation.CConversation(_texts[2]);
			yield return npcConversation.CConversation(_texts[3]);
			npcConversation.Done();
			_actions[3].TryStart();
			while (_actions[3].running)
			{
				yield return null;
			}
			_actions[5].TryStart();
			yield return npcConversation.CConversation(_texts[4]);
			yield return npcConversation.CConversation(_texts[5]);
			_actions[4].TryStart();
			npcConversation.Done();
		}

		public IEnumerator CTalkToStartCombat()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_actions[6].TryStart();
			yield return npcConversation.CConversation(_texts[6]);
			yield return npcConversation.CConversation(_texts[7]);
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
		}

		public IEnumerator COutroTalk()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = true;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			_actions[7].TryStart();
			yield return npcConversation.CConversation(_outroTexts[0]);
			yield return npcConversation.CConversation(_outroTexts[1]);
			_actions[8].TryStart();
			yield return npcConversation.CConversation(_outroTexts[2]);
			yield return npcConversation.CConversation(_outroTexts[3]);
			_actions[9].TryStart();
			yield return Chronometer.global.WaitForSeconds(1f);
			Scene<GameBase>.instance.uiManager.npcConversation.Done();
		}
	}
}
