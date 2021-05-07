using System.Collections;
using Characters;
using Data;
using FX;
using Services;
using Singletons;
using UI;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	public class DarkPriest : FieldNpc
	{
		[SerializeField]
		private EffectInfo _frontEffect;

		[SerializeField]
		private EffectInfo _behindEffect;

		[SerializeField]
		private SoundInfo _sound;

		[SerializeField]
		private float _effectShowingDuration;

		[SerializeField]
		private SkillChangingEffect _skillChangingEffect;

		private int _goldCostIndex;

		protected override NpcType _type => NpcType.DarkPriest;

		private int[] _goldCosts => Singleton<Service>.Instance.levelManager.currentChapter.currentStage.fieldNpcSettings.darkPriestGoldCosts;

		private int _goldCost => _goldCosts[_goldCostIndex];

		protected override void Interact(Character character)
		{
			base.Interact(character);
			StartCoroutine(CGreetingAndConfirm(character, _goldCost));
		}

		private IEnumerator CGreetingAndConfirm(Character character, object confirmArg = null)
		{
			yield return LetterBox.instance.CAppear();
			string[] scripts = ((_phase == Phase.Initial) ? base._greeting : base._regreeting);
			_phase = Phase.Greeted;
			_npcConversation.skippable = true;
			int lastIndex = scripts.Length - 1;
			for (int i = 0; i < lastIndex; i++)
			{
				yield return _npcConversation.CConversation(scripts[i]);
			}
			_npcConversation.skippable = true;
			_npcConversation.body = ((confirmArg == null) ? scripts[lastIndex] : string.Format(scripts[lastIndex], confirmArg));
			_npcConversation.OpenCurrencyBalancePanel(GameData.Currency.Type.Gold);
			yield return _npcConversation.CType();
			yield return new WaitForSecondsRealtime(0.3f);
			_npcConversation.OpenConfirmSelector(delegate
			{
				OnConfirmed(character);
			}, base.Close);
		}

		private void OnConfirmed(Character character)
		{
			_003C_003Ec__DisplayClass14_0 _003C_003Ec__DisplayClass14_ = new _003C_003Ec__DisplayClass14_0();
			_003C_003Ec__DisplayClass14_.character = character;
			_003C_003Ec__DisplayClass14_._003C_003E4__this = this;
			_npcConversation.CloseCurrencyBalancePanel();
			if (GameData.Currency.gold.Consume(_goldCost))
			{
				if (_goldCostIndex < _goldCosts.Length - 1)
				{
					_goldCostIndex++;
				}
				StartCoroutine(_003C_003Ec__DisplayClass14_._003COnConfirmed_003Eg__CRerollSkills_007C0());
			}
			else
			{
				StartCoroutine(_003C_003Ec__DisplayClass14_._003COnConfirmed_003Eg__CNoMoneyAndClose_007C1());
			}
		}
	}
}
