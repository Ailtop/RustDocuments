using System;
using System.Collections;
using Characters;
using Data;
using Housing;
using Scenes;
using Services;
using UI;
using UnityEngine;

namespace Level.Npc
{
	public class DeathKnight : InteractiveObject
	{
		[Serializable]
		public class SpecialBuildText
		{
			[SerializeField]
			public string textKey;

			[SerializeField]
			public BuildLevel target;

			public string Text(int cost)
			{
				return string.Format(Lingua.GetLocalizedString($"npc/{NpcType.DeathKnight}/build/Special/{textKey}"), cost);
			}
		}

		private const NpcType _type = NpcType.DeathKnight;

		private bool _afterBuild;

		private NpcConversation _npcConversation;

		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private HousingBuilder _housingBuilder;

		[SerializeField]
		private SpecialBuildText[] _specialBuildTexts;

		public string displayName => Lingua.GetLocalizedString($"npc/{NpcType.DeathKnight}/name");

		public string greeting => Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/greeting").Random();

		public string[] chat => Lingua.GetLocalizedStringArrays($"npc/{NpcType.DeathKnight}/chat").Random();

		public string buildLabel => Lingua.GetLocalizedString($"npc/{NpcType.DeathKnight}/build/Label");

		public string buildSuccess => Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/build/Success").Random();

		public string buildNoMoney => Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/build/NoMoney").Random();

		public string[] buildNoLevel => Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/build/NoLevel");

		public string buildAgain => Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/build/Again").Random();

		public string BuildText(int cost)
		{
			return string.Format(Lingua.GetLocalizedStringArray($"npc/{NpcType.DeathKnight}/build").Random(), cost);
		}

		private string GetBuildText(BuildLevel buildLevel)
		{
			for (int i = 0; i < _specialBuildTexts.Length; i++)
			{
				if (_specialBuildTexts[i].target == buildLevel)
				{
					return _specialBuildTexts[i].Text(buildLevel.cost);
				}
			}
			return BuildText(buildLevel.cost);
		}

		protected override void Awake()
		{
			base.Awake();
			if (!GameData.Progress.GetRescued(NpcType.DeathKnight))
			{
				base.gameObject.SetActive(false);
			}
		}

		private void Start()
		{
			_npcConversation = Scene<GameBase>.instance.uiManager.npcConversation;
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
			StartCoroutine(CBuild());
		}

		private IEnumerator CBuild()
		{
			yield return LetterBox.instance.CAppear();
			_npcConversation.body = (_afterBuild ? buildAgain : greeting);
			_npcConversation.skippable = false;
			_npcConversation.Type();
			_npcConversation.OpenContentSelector(buildLabel, Build, Chat, Close);
		}

		private void Chat()
		{
			_npcConversation.skippable = true;
			StartCoroutine(_003CChat_003Eg__CRun_007C30_0());
		}

		private void Close()
		{
			_npcConversation.visible = false;
			LetterBox.instance.Disappear();
		}

		private void Build()
		{
			_003C_003Ec__DisplayClass32_0 _003C_003Ec__DisplayClass32_ = new _003C_003Ec__DisplayClass32_0();
			_003C_003Ec__DisplayClass32_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass32_.nextBuildLevel = _housingBuilder.GetLevelAfterPoint(GameData.Progress.housingPoint);
			StartCoroutine(_003C_003Ec__DisplayClass32_._003CBuild_003Eg__CRun_007C0());
		}
	}
}
