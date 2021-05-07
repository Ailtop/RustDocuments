using System.Collections;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class NextTalk : Sequence
	{
		[SerializeField]
		private TalkerInfo _talker;

		[SerializeField]
		private bool _skippable = true;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = _talker.portrait;
			npcConversation.skippable = _skippable;
			npcConversation.name = _talker.name;
			yield return npcConversation.CConversation(_talker.GetNextText());
		}
	}
}
