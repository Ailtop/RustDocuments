using Characters;
using Data;
using FX;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class Witch : InteractiveObject
	{
		private const NpcType _type = NpcType.Witch;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private Sprite _portraitCat;

		[SerializeField]
		private GameObject _body;

		private NpcConversation _npcConversation;

		[SerializeField]
		private SoundInfo _open;

		[SerializeField]
		private SoundInfo _close;

		public string displayName => Lingua.GetLocalizedString($"npc/{NpcType.Witch}/name");

		public string greeting => Lingua.GetLocalizedStringArray($"npc/{NpcType.Witch}/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Witch}/chat").Random();

		public string masteriesScript => Lingua.GetLocalizedStringArray($"npc/{NpcType.Witch}/Masteries").Random();

		public string masteriesLabel => Lingua.GetLocalizedString($"npc/{NpcType.Witch}/Masteries/label");

		protected override void Awake()
		{
			base.Awake();
			_animator.Play("Idle_Human_Castle");
		}

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		private void OnDisable()
		{
			if (!Service.quitting)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_close, base.transform.position);
				LetterBox.instance.visible = false;
			}
		}

		public override void InteractWith(Character character)
		{
			StartCoroutine(_003CInteractWith_003Eg__CRun_007C21_0());
		}

		private void OpenContent()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_open, base.transform.position);
			_npcConversation.OpenCurrencyBalancePanel(GameData.Currency.Type.DarkQuartz);
			_npcConversation.witchContent.SetActive(true);
			_npcConversation.body = masteriesScript;
			_npcConversation.skippable = false;
			_npcConversation.Type();
		}

		private void Chat()
		{
			_npcConversation.skippable = true;
			StartCoroutine(_003CChat_003Eg__CRun_007C23_0());
		}

		private void Close()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_close, base.transform.position);
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
		}
	}
}
