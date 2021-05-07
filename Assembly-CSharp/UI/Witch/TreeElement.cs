using Characters;
using FX;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Witch
{
	public class TreeElement : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		private Panel _panel;

		[SerializeField]
		private Button _button;

		[SerializeField]
		private TMP_Text _level;

		[SerializeField]
		private GameObject _ready;

		[SerializeField]
		private GameObject _mastered;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private Image _deactivateMask;

		private WitchBonus.Bonus _bonus;

		[SerializeField]
		private SoundInfo _getAbility;

		[SerializeField]
		private SoundInfo _select;

		public bool interactable
		{
			get
			{
				return _button.interactable;
			}
			set
			{
				_button.interactable = value;
			}
		}

		private void Awake()
		{
			UnityAction call = delegate
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_getAbility, base.transform.position);
				_bonus.LevelUp();
				_level.text = _bonus.level.ToString();
				_panel.UpdateCurrentOption();
			};
			_button.onClick.AddListener(call);
		}

		public void Initialize(Panel panel)
		{
			_panel = panel;
		}

		public void Set(WitchBonus.Bonus bonus)
		{
			_bonus = bonus;
			_name.text = _bonus.displayName;
			_level.text = _bonus.level.ToString();
		}

		public void OnSelect(BaseEventData eventData)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_select, base.transform.position);
			_panel.Set(_bonus);
		}

		private void Update()
		{
			_deactivateMask.enabled = !_bonus.ready;
			if (_ready != null && !_ready.activeSelf && _bonus.ready)
			{
				_ready.SetActive(true);
			}
			if (_mastered != null && !_mastered.activeSelf && _bonus.level == _bonus.maxLevel)
			{
				_mastered.SetActive(true);
			}
		}
	}
}
