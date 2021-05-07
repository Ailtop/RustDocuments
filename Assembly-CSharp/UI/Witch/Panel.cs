using Characters;
using Scenes;
using Services;
using Singletons;
using UnityEngine;
using UserInput;

namespace UI.Witch
{
	public class Panel : Dialogue
	{
		[SerializeField]
		private Tree _skull;

		[SerializeField]
		private Tree _body;

		[SerializeField]
		private Tree _soul;

		[SerializeField]
		private Option _currentOption;

		private WitchBonus _witchBonus;

		private WitchBonus.Bonus _bonus;

		public override bool closeWithPauseKey => false;

		protected override void OnEnable()
		{
			base.OnEnable();
			Character player = Singleton<Service>.Instance.levelManager.player;
			_witchBonus = WitchBonus.instance;
			_skull.Initialize(this);
			_body.Initialize(this);
			_soul.Initialize(this);
			_skull.Set(_witchBonus.skull);
			_body.Set(_witchBonus.body);
			_soul.Set(_witchBonus.soul);
		}

		protected override void OnDisable()
		{
			if (!Service.quitting)
			{
				base.OnDisable();
				LetterBox.instance.Disappear();
			}
		}

		private void Update()
		{
			if (KeyMapper.Map.Cancel.WasPressed)
			{
				Scene<GameBase>.instance.uiManager.npcConversation.visible = false;
				base.gameObject.SetActive(false);
			}
		}

		public void Set(WitchBonus.Bonus bonus)
		{
			_bonus = bonus;
			_currentOption.Set(bonus);
		}

		public void UpdateCurrentOption()
		{
			_currentOption.UpdateTexts();
		}
	}
}
