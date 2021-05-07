using System.Collections;
using Characters;
using Characters.Gear.Weapons;
using Data;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class Fox : InteractiveObject
	{
		private enum Phase
		{
			Initial,
			Gave,
			ExtraGave
		}

		private const NpcType _type = NpcType.Fox;

		[SerializeField]
		private int _extraHeadDarkQuartzCost;

		[SerializeField]
		private RarityPossibilities _headPossibilities;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private Transform _dropPosition;

		private Phase _phase;

		private NpcConversation _npcConversation;

		private Resource.Request<Weapon> _weaponToDrop;

		private Resource.Request<Weapon> _extraWeaponToDrop;

		public string displayName => Lingua.GetLocalizedString($"npc/{NpcType.Fox}/name");

		public string greeting => Lingua.GetLocalizedStringArray($"npc/{NpcType.Fox}/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Fox}/chat").Random();

		public string[] giveHeadScripts => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Fox}/GiveHead").Random();

		public string giveExtraHead => string.Format(Lingua.GetLocalizedStringArray($"npc/{NpcType.Fox}/GiveExtraHead").Random(), _extraHeadDarkQuartzCost);

		public string giveExtraHeadLabel => Lingua.GetLocalizedString($"npc/{NpcType.Fox}/GiveExtraHead/label");

		public string[] giveExtraHeadNoMoney => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Fox}/GiveExtraHead/NoMoney").Random();

		protected override void Awake()
		{
			base.Awake();
			if (!GameData.Progress.GetRescued(NpcType.Fox))
			{
				base.gameObject.SetActive(false);
			}
		}

		private void Start()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_weaponToDrop = Singleton<Service>.Instance.gearManager.GetWeaponToTake(_headPossibilities.Evaluate()).LoadAsync();
		}

		private void OnDisable()
		{
			if (!Service.quitting)
			{
				LetterBox.instance.visible = false;
			}
		}

		public override void InteractWith(Character character)
		{
			_npcConversation.name = displayName;
			_npcConversation.portrait = _portrait;
			switch (_phase)
			{
			case Phase.Initial:
				_phase = Phase.Gave;
				StartCoroutine(CGiveHead(character));
				break;
			case Phase.Gave:
				StartCoroutine(CSelectContent());
				break;
			case Phase.ExtraGave:
				Chat();
				break;
			}
		}

		private IEnumerator CSelectContent()
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.body = greeting;
			_npcConversation.skippable = false;
			_npcConversation.Type();
			_npcConversation.OpenContentSelector(giveExtraHeadLabel, GetExtraHead, Chat, Close);
		}

		private void Chat()
		{
			_npcConversation.skippable = true;
			StartCoroutine(_003CChat_003Eg__CRun_007C29_0());
		}

		private void Close()
		{
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
		}

		private void GetExtraHead()
		{
			StartCoroutine(_003CGetExtraHead_003Eg__CRun_007C31_0());
		}

		private IEnumerator CGiveHead(Character character)
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.skippable = true;
			yield return _npcConversation.CConversation(giveHeadScripts);
			LetterBox.instance.Disappear();
			while (!_weaponToDrop.isDone)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.DropWeapon(_weaponToDrop.asset, _dropPosition.position);
			_extraWeaponToDrop = Singleton<Service>.Instance.gearManager.GetWeaponToTake(_headPossibilities.Evaluate()).LoadAsync();
		}
	}
}
