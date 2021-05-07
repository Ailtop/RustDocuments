using System.Collections;
using Characters;
using Characters.AI;
using Characters.Player;
using CutScenes;
using Data;
using Runnables;
using Scenes;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class Arachne : InteractiveObject
	{
		private enum Phase
		{
			First,
			Awakened
		}

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private ReviveScarecrowOnDie _scarecrow;

		[SerializeField]
		private Runnable _awakeningForRare;

		[SerializeField]
		private Runnable _awakeningForUnique;

		[SerializeField]
		private Runnable _awakeningForLegendary;

		[SerializeField]
		private Runnable _tutorial;

		private const CutScenes.Key _key = CutScenes.Key.arachne;

		private Phase _phase;

		private NpcConversation _npcConversation;

		private WeaponInventory _weaponInventory;

		private string displayName => Lingua.GetLocalizedString("npc/arachne/name");

		private string greeting => Lingua.GetLocalizedString("npc/arachne/greeting");

		private string awakenLabel => Lingua.GetLocalizedString("npc/arachne/awaken/label");

		private string notExistsNextGrade => Lingua.GetLocalizedString("npc/arachne/awaken/notExistsNextGrade");

		private string awaken => Lingua.GetLocalizedString("npc/arachne/awaken");

		private string noMoney => Lingua.GetLocalizedString("npc/arachne/awaken/noMoney");

		private string[] skulAwaken => Lingua.GetLocalizedStringArray("npc/arachne/awaken/skul");

		private string[] tutorial => Lingua.GetLocalizedStringArray("npc/arachne/tutorial");

		private string[] chat => Lingua.GetLocalizedStringArrays("npc/arachne/chat").Random();

		private string askAwaken(int cost)
		{
			return string.Format(Lingua.GetLocalizedString("npc/arachne/awaken/ask"), cost);
		}

		private string askAwakenAgain(int cost)
		{
			return string.Format(Lingua.GetLocalizedString("npc/arachne/awaken/askAgain"), cost);
		}

		protected override void Awake()
		{
			base.Awake();
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
		}

		public override void InteractWith(Character character)
		{
			character.CancelAction();
			_weaponInventory = character.playerComponents.inventory.weapon;
			_lineText.gameObject.SetActive(false);
			_npcConversation.name = displayName;
			_npcConversation.skippable = true;
			_npcConversation.portrait = null;
			if (!GameData.Progress.cutscene.GetData(CutScenes.Key.arachne))
			{
				_tutorial.Run();
			}
			else
			{
				StartCoroutine(CGreeting());
			}
		}

		public void SpawnScarecrowWave()
		{
			if (!(_scarecrow == null) && !_scarecrow.gameObject.activeSelf)
			{
				_scarecrow.gameObject.SetActive(true);
			}
		}

		private void SimpleConversationAndClose(params string[] texts)
		{
			_003C_003Ec__DisplayClass34_0 _003C_003Ec__DisplayClass34_ = new _003C_003Ec__DisplayClass34_0();
			_003C_003Ec__DisplayClass34_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass34_.texts = texts;
			StartCoroutine(_003C_003Ec__DisplayClass34_._003CSimpleConversationAndClose_003Eg__CRun_007C0());
		}

		private IEnumerator CGreeting()
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.body = greeting;
			_npcConversation.skippable = false;
			_npcConversation.Type();
			_npcConversation.OpenContentSelector(awakenLabel, _003CCGreeting_003Eg__OnSelectAwaken_007C35_0, _003CCGreeting_003Eg__OnSelectChat_007C35_1, Close);
		}

		private IEnumerator CAskAwaken()
		{
			_003C_003Ec__DisplayClass36_0 _003C_003Ec__DisplayClass36_ = new _003C_003Ec__DisplayClass36_0();
			_003C_003Ec__DisplayClass36_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass36_.cost = Settings.instance.bonesToUpgrade.get_Item(_weaponInventory.current.rarity);
			_npcConversation.skippable = true;
			_npcConversation.body = ((_phase == Phase.First) ? askAwaken(_003C_003Ec__DisplayClass36_.cost) : askAwakenAgain(_003C_003Ec__DisplayClass36_.cost));
			_npcConversation.OpenCurrencyBalancePanel(GameData.Currency.Type.Bone);
			yield return _npcConversation.CType();
			_npcConversation.OpenConfirmSelector(_003C_003Ec__DisplayClass36_._003CCAskAwaken_003Eg__OnSelectYes_007C0, Close);
		}

		private IEnumerator CAwaken()
		{
			_npcConversation.skippable = true;
			_npcConversation.body = awaken;
			yield return _npcConversation.CType();
			yield return _npcConversation.CWaitInput();
			_npcConversation.visible = false;
			_phase = Phase.Awakened;
			Rarity rarity = _weaponInventory.current.nextLevelReference.rarity;
			switch ((int)rarity)
			{
			case 0:
			case 1:
				_awakeningForRare.Run();
				break;
			case 2:
				_awakeningForUnique.Run();
				break;
			case 3:
				_awakeningForLegendary.Run();
				break;
			}
		}

		public IEnumerator CUpgrade()
		{
			yield return _weaponInventory.CUpgradeCurrentWeapon();
		}

		private IEnumerator CNoMoney()
		{
			_npcConversation.skippable = true;
			_npcConversation.body = noMoney;
			yield return _npcConversation.CType();
			yield return _npcConversation.CWaitInput();
			Close();
		}

		private void Close()
		{
			_npcConversation.visible = false;
			_npcConversation.CloseCurrencyBalancePanel();
			LetterBox.instance.Disappear();
			_lineText.gameObject.SetActive(true);
		}
	}
}
