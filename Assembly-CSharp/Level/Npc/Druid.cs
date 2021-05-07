using Characters;
using Data;
using Scenes;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class Druid : InteractiveObject
	{
		private const NpcType _type = NpcType.Druid;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private NpcLineText _lineText;

		private NpcConversation _npcConversation;

		public string displayName => Lingua.GetLocalizedString($"npc/{NpcType.Druid}/name");

		public string greeting => Lingua.GetLocalizedStringArray($"npc/{NpcType.Druid}/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Druid}/chat").Random();

		public string changeProphecyLabel => Lingua.GetLocalizedString($"npc/{NpcType.Druid}/ChangeProphecy/label");

		public string[] changeProphecyNoMoney => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Druid}/ChangeProphecy/NoMoney").Random();

		protected override void Awake()
		{
			base.Awake();
			if (!GameData.Progress.GetRescued(NpcType.Druid))
			{
				base.gameObject.SetActive(false);
			}
		}

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		public override void InteractWith(Character character)
		{
			_lineText.gameObject.SetActive(false);
			_npcConversation.name = displayName;
			_npcConversation.portrait = _portrait;
			_npcConversation.skippable = true;
			StartCoroutine(_003CInteractWith_003Eg__CRun_007C16_0());
		}

		private void Chat()
		{
			StartCoroutine(_003CChat_003Eg__CRun_007C17_0());
		}

		private void Close()
		{
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
			_lineText.gameObject.SetActive(true);
		}
	}
}
