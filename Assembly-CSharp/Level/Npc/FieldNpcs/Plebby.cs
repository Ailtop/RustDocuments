using System.Collections;
using Characters;
using Characters.Gear.Items;
using Data;
using FX;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class Plebby : FieldNpc
	{
		[SerializeField]
		private Transform _dropPosition;

		[SerializeField]
		private EffectInfo _dropEffect;

		[SerializeField]
		private SoundInfo _dropSound;

		private Resource.Request<Item> _itemToDrop;

		protected override NpcType _type => NpcType.Plebby;

		protected string _displayNameA => Lingua.GetLocalizedString($"npc/{_type}/A/name");

		protected string _displayNameB => Lingua.GetLocalizedString($"npc/{_type}/B/name");

		private int _goldCost => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.plebbyGoldCost;

		private RarityPossibilities _itemPossibilities => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.plebbyItemPossibilities;

		private void Start()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			_itemToDrop = Singleton<Service>.Instance.gearManager.GetItemToTake(_itemPossibilities.Evaluate()).LoadAsync();
		}

		protected override void Interact(Character character)
		{
			base.Interact(character);
			switch (_phase)
			{
			case Phase.Initial:
			case Phase.Greeted:
				StartCoroutine(CGreetingAndConfirm(character));
				break;
			case Phase.Gave:
				StartCoroutine(CChat());
				break;
			}
		}

		private IEnumerator CGreetingAndConfirm(Character character)
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.skippable = true;
			string arg = ((_phase == Phase.Initial) ? "greeting" : "regreeting");
			string[] greeting = Lingua.GetLocalizedStringArray($"npc/{_type}/{arg}");
			string[] speaker = Lingua.GetLocalizedStringArray($"npc/{_type}/{arg}/speaker");
			_phase = Phase.Greeted;
			int lastIndex = greeting.Length - 1;
			for (int i = 0; i < lastIndex; i++)
			{
				_npcConversation.name = speaker[i];
				yield return _npcConversation.CConversation(greeting[i]);
			}
			_npcConversation.name = speaker[lastIndex];
			_npcConversation.body = string.Format(greeting[lastIndex], _goldCost);
			_npcConversation.OpenCurrencyBalancePanel(GameData.Currency.Type.Gold);
			yield return _npcConversation.CType();
			yield return new WaitForSecondsRealtime(0.3f);
			_npcConversation.OpenConfirmSelector(OnConfirmed, base.Close);
		}

		private void OnConfirmed()
		{
			_npcConversation.CloseCurrencyBalancePanel();
			if (GameData.Currency.gold.Has(_goldCost))
			{
				_phase = Phase.Gave;
				StartCoroutine(CConfirmed());
			}
			else
			{
				StartCoroutine(CNoMoneyAndClose());
			}
		}

		private IEnumerator CConversation(string key)
		{
			string[] localizedStringArray = Lingua.GetLocalizedStringArray(key);
			string[] localizedStringArray2 = Lingua.GetLocalizedStringArray(key + "/speaker");
			yield return CConversation(localizedStringArray2, localizedStringArray);
		}

		private IEnumerator CConversation(string[] speakers, string[] scripts)
		{
			for (int i = 0; i < scripts.Length; i++)
			{
				_npcConversation.name = speakers[i];
				yield return _npcConversation.CConversation(scripts[i]);
			}
		}

		private IEnumerator CConfirmed()
		{
			_npcConversation.skippable = true;
			yield return CDropItem();
			GameData.Currency.gold.Consume(_goldCost);
			yield return CConversation($"npc/{_type}/confirmed");
			LetterBox.instance.Disappear();
		}

		private IEnumerator CDropItem()
		{
			while (!_itemToDrop.isDone)
			{
				yield return null;
			}
			Singleton<Service>.Instance.levelManager.DropItem(_itemToDrop.asset, _dropPosition.position);
			_dropEffect.Spawn(_dropPosition.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dropSound, base.transform.position);
		}

		private IEnumerator CNoMoneyAndClose()
		{
			_npcConversation.skippable = true;
			yield return CConversation($"npc/{_type}/noMoney");
			LetterBox.instance.Disappear();
		}

		private new IEnumerator CChat()
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.skippable = true;
			string[][] localizedStringArrays = Lingua.GetLocalizedStringArrays($"npc/{_type}/chat");
			string[][] localizedStringArrays2 = Lingua.GetLocalizedStringArrays($"npc/{_type}/chat/speaker");
			int num = localizedStringArrays.RandomIndex();
			yield return CConversation(localizedStringArrays2[num], localizedStringArrays[num]);
			LetterBox.instance.Disappear();
		}
	}
}
