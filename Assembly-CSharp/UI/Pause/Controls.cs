using Data;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI.Pause
{
	public class Controls : Dialogue
	{
		private enum InputDeviceType
		{
			KeyboardAndMouse,
			GameController
		}

		private const string _key = "label/pause/settings";

		[SerializeField]
		private Panel _panel;

		[SerializeField]
		private PressNewKey _pressNewKey;

		[SerializeField]
		private KeyBinder _up;

		[SerializeField]
		private KeyBinder _down;

		[SerializeField]
		private KeyBinder _left;

		[SerializeField]
		private KeyBinder _right;

		[SerializeField]
		private KeyBinder _attack;

		[SerializeField]
		private KeyBinder _jump;

		[SerializeField]
		private KeyBinder _dash;

		[SerializeField]
		private Toggle _arrowDash;

		[SerializeField]
		private KeyBinder _swap;

		[SerializeField]
		private KeyBinder _skill1;

		[SerializeField]
		private KeyBinder _skill2;

		[SerializeField]
		private KeyBinder _quintessence;

		[SerializeField]
		private KeyBinder _inventory;

		[SerializeField]
		private KeyBinder _interaction;

		[SerializeField]
		private Button _reset;

		[SerializeField]
		private Button _return;

		public override bool closeWithPauseKey => false;

		private void Awake()
		{
			_reset.onClick.AddListener(delegate
			{
				KeyMapper.Map.ResetToDefault();
			});
			_return.onClick.AddListener(delegate
			{
				_panel.state = Panel.State.Menu;
			});
			_up.Initialize(KeyMapper.Map.Up, _pressNewKey);
			_down.Initialize(KeyMapper.Map.Down, _pressNewKey);
			_left.Initialize(KeyMapper.Map.Left, _pressNewKey);
			_right.Initialize(KeyMapper.Map.Right, _pressNewKey);
			_attack.Initialize(KeyMapper.Map.Attack, _pressNewKey);
			_jump.Initialize(KeyMapper.Map.Jump, _pressNewKey);
			_dash.Initialize(KeyMapper.Map.Dash, _pressNewKey);
			ArrowDashText();
			_arrowDash.value = (GameData.Settings.arrowDashEnabled ? 1 : 0);
			_arrowDash.onValueChanged += delegate(int v)
			{
				GameData.Settings.arrowDashEnabled = v == 1;
			};
			_swap.Initialize(KeyMapper.Map.Swap, _pressNewKey);
			_skill1.Initialize(KeyMapper.Map.Skill1, _pressNewKey);
			_skill2.Initialize(KeyMapper.Map.Skill2, _pressNewKey);
			_quintessence.Initialize(KeyMapper.Map.Quintessence, _pressNewKey);
			_inventory.Initialize(KeyMapper.Map.Inventory, _pressNewKey);
			_interaction.Initialize(KeyMapper.Map.Interaction, _pressNewKey);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_up.UpdateKeyImageAndBindingSource();
			_down.UpdateKeyImageAndBindingSource();
			_left.UpdateKeyImageAndBindingSource();
			_right.UpdateKeyImageAndBindingSource();
			_attack.UpdateKeyImageAndBindingSource();
			_jump.UpdateKeyImageAndBindingSource();
			_dash.UpdateKeyImageAndBindingSource();
			ArrowDashText();
			_swap.UpdateKeyImageAndBindingSource();
			_skill1.UpdateKeyImageAndBindingSource();
			_skill2.UpdateKeyImageAndBindingSource();
			_quintessence.UpdateKeyImageAndBindingSource();
			_inventory.UpdateKeyImageAndBindingSource();
			_interaction.UpdateKeyImageAndBindingSource();
		}

		private void ArrowDashText()
		{
			_arrowDash.SetTexts(Lingua.GetLocalizedStrings("label/pause/settings/off", "label/pause/settings/on"));
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			GameData.Settings.keyBindings = KeyMapper.Map.Save();
		}
	}
}
