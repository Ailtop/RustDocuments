using System.Collections;
using System.Collections.Generic;
using Scenes;
using UI;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public class ShowRandomDialog : Sequence
	{
		[SerializeField]
		private TextMessageInfo[] _messageInfo;

		[SerializeField]
		private Sprite _portrait;

		private NpcConversation _npcConversation;

		private int _startIndex;

		private int _endIndex;

		private int _randomIndex;

		private List<string> _texts;

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_npcConversation.portrait = _portrait;
			_npcConversation.skippable = true;
			_texts = new List<string>();
		}

		public override IEnumerator CRun()
		{
			_randomIndex = Random.Range(0, _messageInfo.Length);
			_startIndex = _messageInfo[_randomIndex].messages[0].startIndex;
			_endIndex = _messageInfo[_randomIndex].messages[0].endIndex;
			TextMessageInfo obj = _messageInfo[_randomIndex];
			string nameKey = obj.nameKey;
			string messageKey = obj.messageKey;
			_npcConversation.name = Lingua.GetLocalizedString(nameKey);
			while (_startIndex <= _endIndex)
			{
				_texts.Add(Lingua.GetLocalizedString(messageKey + _startIndex));
				_startIndex++;
			}
			yield return _npcConversation.CConversation(_texts.ToArray());
			_texts.Clear();
			_startIndex = _messageInfo[_randomIndex].messages[0].startIndex;
		}
	}
}
