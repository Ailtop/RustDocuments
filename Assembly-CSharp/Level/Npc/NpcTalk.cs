using Characters;
using Scenes;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Level.Npc
{
	public class NpcTalk : InteractiveObject
	{
		[SerializeField]
		private string _displayNameKey;

		[SerializeField]
		private string _greetingKey;

		[SerializeField]
		private string _chatKey;

		[SerializeField]
		private UnityEvent _onChat;

		private NpcConversation _npcConversation;

		private string displayName => Lingua.GetLocalizedString(_displayNameKey);

		private string greeting => Lingua.GetLocalizedStringArray(_greetingKey).Random();

		private string[] chat => Lingua.GetLocalizedStringArrays(_chatKey).Random();

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		public override void InteractWith(Character character)
		{
			LetterBox.instance.Appear();
			_npcConversation.name = displayName;
			_npcConversation.portrait = null;
			_npcConversation.body = greeting;
			_npcConversation.skippable = false;
			_npcConversation.Type();
			_npcConversation.OpenChatSelector(Chat, Close);
		}

		private void Chat()
		{
			_npcConversation.skippable = true;
			StartCoroutine(_003CChat_003Eg__CRun_007C13_0());
		}

		private void Close()
		{
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
		}
	}
}
