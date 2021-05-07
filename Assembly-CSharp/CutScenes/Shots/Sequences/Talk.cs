using System.Collections;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class Talk : Sequence
	{
		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private string _nameKey;

		[SerializeField]
		private string _textKey;

		[SerializeField]
		private bool _skippable = true;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.portrait = null;
			npcConversation.skippable = _skippable;
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			yield return npcConversation.CConversation(Lingua.GetLocalizedStringArray(_textKey));
		}
	}
}
