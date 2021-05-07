using System.Collections;
using Characters;
using Characters.Gear.Items;
using Data;
using Scenes;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class Ogre : InteractiveObject
	{
		private enum Phase
		{
			Initial,
			Gave,
			ExtraGave
		}

		private const NpcType _type = NpcType.Ogre;

		[SerializeField]
		private int _extraItemDarkQuartzCost;

		[SerializeField]
		private RarityPossibilities _itemPossibilities;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private Transform _dropPosition;

		private Phase _phase;

		private NpcConversation _npcConversation;

		private Resource.Request<Item> _itemToDrop;

		private Resource.Request<Item> _extraItemToDrop;

		public string displayName => Lingua.GetLocalizedString($"npc/{NpcType.Ogre}/name");

		public string greeting => Lingua.GetLocalizedStringArray($"npc/{NpcType.Ogre}/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Ogre}/chat").Random();

		public string[] giveItemScripts => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Ogre}/GiveItem").Random();

		public string giveExtraItem => string.Format(Lingua.GetLocalizedStringArray($"npc/{NpcType.Ogre}/GiveExtraItem").Random(), _extraItemDarkQuartzCost);

		public string giveExtraItemLabel => Lingua.GetLocalizedString($"npc/{NpcType.Ogre}/GiveExtraItem/label");

		public string[] giveExtraItemNoMoney => Lingua.GetLocalizedStringArrays($"npc/{NpcType.Ogre}/GiveExtraItem/NoMoney").Random();

		protected override void Awake()
		{
			base.Awake();
			if (!GameData.Progress.GetRescued(NpcType.Ogre))
			{
				base.gameObject.SetActive(false);
			}
		}

		private void Start()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
			_itemToDrop = Singleton<Service>.Instance.gearManager.GetItemToTake(_itemPossibilities.Evaluate()).LoadAsync();
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
				StartCoroutine(CGiveItem(character));
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
			_npcConversation.OpenContentSelector(giveExtraItemLabel, GetExtraItem, Chat, Close);
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

		private void GetExtraItem()
		{
			StartCoroutine(_003CGetExtraItem_003Eg__CRun_007C31_0());
		}

		private IEnumerator CGiveItem(Character character)
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.skippable = true;
			yield return _npcConversation.CConversation(giveItemScripts);
			LetterBox.instance.Disappear();
			while (!_itemToDrop.isDone)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.DropItem(_itemToDrop.asset, _dropPosition.position);
			_extraItemToDrop = Singleton<Service>.Instance.gearManager.GetItemToTake(_itemPossibilities.Evaluate()).LoadAsync();
		}
	}
}
