using System.Collections;
using Scenes;
using UI;
using UnityEditor;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public class OpenChatSelector : Sequence
	{
		[SerializeField]
		private string _nameKey;

		[SerializeField]
		private string _textKey;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ShotInfo))]
		private ShotInfo.Subcomponents _onChat;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ShotInfo))]
		private ShotInfo.Subcomponents _onClose;

		public override IEnumerator CRun()
		{
			NpcConversation npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			npcConversation.OpenChatSelector(delegate
			{
				_onChat.Run(null, null);
			}, delegate
			{
				_onClose.Run(null, null);
			});
			npcConversation.name = Lingua.GetLocalizedString(_nameKey);
			npcConversation.body = Lingua.GetLocalizedString(_textKey);
			yield return npcConversation.CType();
		}
	}
}
