using System.Collections;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class TalkRandomly : Sequence
	{
		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private string _nameKey;

		[SerializeField]
		private string _textArrayKey;

		[SerializeField]
		private bool _skippable = true;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = _skippable;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			string[] texts = Lingua.GetLocalizedStringArrays(_textArrayKey).Random();
			yield return npcConversation.CConversation(texts);
		}
	}
}
