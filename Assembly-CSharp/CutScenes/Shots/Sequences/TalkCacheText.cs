using System.Collections;
using Runnables;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class TalkCacheText : Sequence
	{
		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private TextKeyCache _nameCache;

		[SerializeField]
		private TextKeyCache _chatCache;

		[SerializeField]
		private bool _skippable = true;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = _portrait;
			npcConversation.skippable = _skippable;
			npcConversation.name = Lingua.GetLocalizedString(_nameCache.key);
			yield return npcConversation.CConversation(Lingua.GetLocalizedString(_chatCache.key));
		}
	}
}
