using Characters;
using Scenes;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class DemonLord : InteractiveObject
	{
		[SerializeField]
		private string _type;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private NpcLineText _lineText;

		private NpcConversation _npcConversation;

		public string displayName => Lingua.GetLocalizedString("npc/" + _type + "/name");

		public string greeting => Lingua.GetLocalizedStringArray("npc/" + _type + "/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays("npc/" + _type + "/chat").Random();

		public override void InteractWith(Character character)
		{
			_lineText.gameObject.SetActive(false);
			_npcConversation.name = displayName;
			_npcConversation.portrait = _portrait;
			_npcConversation.skippable = true;
			StartCoroutine(_003CInteractWith_003Eg__CRun_007C10_0());
		}

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		private void Chat()
		{
			StartCoroutine(_003CChat_003Eg__CRun_007C12_0());
		}

		private void Close()
		{
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
			_lineText.gameObject.SetActive(true);
		}
	}
}
