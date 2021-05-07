using System;
using System.Collections;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class TalkRaw : Sequence
	{
		[Serializable]
		private class Info
		{
			public string name;

			public string[] messages;
		}

		[SerializeField]
		private Info _info;

		[SerializeField]
		private bool _skippable = true;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = _skippable;
			npcConversation.name = _info.name;
			yield return npcConversation.CConversation(_info.messages);
		}
	}
}
