using System.Collections;
using System.Collections.Generic;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public sealed class ShowDialog : Sequence
	{
		[SerializeField]
		private TextMessageInfo _messageInfo;

		[SerializeField]
		private Sprite _portrait;

		private int _startIndex;

		private int _endIndex;

		private NpcConversation _npcConversation;

		private List<string> _texts;

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_startIndex = _messageInfo.messages[0].startIndex;
			_endIndex = _messageInfo.messages[0].endIndex;
			_npcConversation.portrait = _portrait;
			_npcConversation.skippable = true;
			_texts = new List<string>();
		}

		public override IEnumerator CRun()
		{
			TextMessageInfo messageInfo = _messageInfo;
			string nameKey = messageInfo.nameKey;
			string messageKey = messageInfo.messageKey;
			_npcConversation.name = Lingua.GetLocalizedString(nameKey);
			while (_startIndex <= _endIndex)
			{
				_texts.Add(Lingua.GetLocalizedString(messageKey + _startIndex));
				_startIndex++;
			}
			yield return _npcConversation.CConversation(_texts.ToArray());
			_startIndex = _messageInfo.messages[0].startIndex;
		}
	}
}
