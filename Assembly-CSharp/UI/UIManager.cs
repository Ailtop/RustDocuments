using Characters.Controllers;
using EndingCredit;
using FX;
using InControl;
using Level;
using Services;
using Singletons;
using SkulStories;
using UI.Boss;
using UI.Ending;
using UI.GearPopup;
using UI.Hud;
using UI.Inventory;
using UI.Pause;
using UI.SkulStories;
using UI.TestingTool;
using UnityEngine;
using UnityEngine.EventSystems;
using UserInput;

namespace UI
{
	public class UIManager : MonoBehaviour
	{
		public enum HideOption
		{
			ShowAll,
			HideHUD,
			HideAll
		}

		[SerializeField]
		private RectTransform _rectTransform;

		[Space]
		[SerializeField]
		private RectTransform _content;

		[Space]
		[SerializeField]
		private HeadupDisplay _headupDisplay;

		[SerializeField]
		private LetterBox _letterBox;

		[SerializeField]
		private NpcContent _npcContent;

		[SerializeField]
		private NpcConversation _npcConversation;

		[SerializeField]
		private Narration _narration;

		[SerializeField]
		private NarrationScene _narrationScene;

		[SerializeField]
		private BossUIContainer _bossUI;

		[SerializeField]
		private AdventurerHealthBarUIController _adventurerHealthBarUIController;

		[SerializeField]
		private StageName _stageName;

		[SerializeField]
		private UnlockNotice _unlockNotice;

		[SerializeField]
		private GameResult _gameResult;

		[SerializeField]
		private EndingGameResult _endingGameResult;

		[SerializeField]
		private UI.Inventory.Panel _inventory;

		[SerializeField]
		private UI.Ending.Panel _ending;

		[SerializeField]
		private UI.TestingTool.Panel _testingTool;

		[SerializeField]
		private Confirm _confirm;

		[SerializeField]
		private UI.SkulStories.Confirm _skulstoryConfirm;

		[SerializeField]
		private GearPopupCanvas _gearPopupCanvas;

		[SerializeField]
		private ItemSelect _itemSelect;

		[SerializeField]
		private InControlInputModule _inputModule;

		[SerializeField]
		private StackVignette _curseOfLightVignette;

		[SerializeField]
		private CreditRoll _endingCredit;

		[SerializeField]
		private UI.Pause.Panel _pause;

		[SerializeField]
		private PauseEventSystem _pauseEventSystem;

		[Header("Hide")]
		[SerializeField]
		private GameObject _hidedByHideHud;

		[SerializeField]
		private GameObject _hidedByHideAll;

		[SerializeField]
		private Scaler _scaler;

		private GameObject _lastSelectedGameObject;

		public RectTransform rectTransform => _rectTransform;

		public RectTransform content => _content;

		public Scaler scaler => _scaler;

		public HeadupDisplay headupDisplay => _headupDisplay;

		public LetterBox letterBox => _letterBox;

		public NpcContent npcContent => _npcContent;

		public NpcConversation npcConversation => _npcConversation;

		public Narration narration => _narration;

		public NarrationScene narrationScene => _narrationScene;

		public BossUIContainer bossUI => _bossUI;

		public StageName stageName => _stageName;

		public UnlockNotice unlockNotice => _unlockNotice;

		public GameResult gameResult => _gameResult;

		public PauseEventSystem pauseEventSystem => _pauseEventSystem;

		public UI.Ending.Panel ending => _ending;

		public UI.TestingTool.Panel testingTool => _testingTool;

		public UI.Inventory.Panel inventory => _inventory;

		public AdventurerHealthBarUIController adventurerHealthBarUIController => _adventurerHealthBarUIController;

		public Confirm confirm => _confirm;

		public GearPopupCanvas gearPopupCanvas => _gearPopupCanvas;

		public ItemSelect itemSelect => _itemSelect;

		public StackVignette curseOfLightVignette => _curseOfLightVignette;

		public CreditRoll endingCredit => _endingCredit;

		public EndingGameResult endingGameResult => _endingGameResult;

		public bool allowMouseInput
		{
			get
			{
				return _inputModule.allowMouseInput;
			}
			set
			{
				_inputModule.allowMouseInput = (_inputModule.focusOnMouseHover = value);
			}
		}

		public HideOption hideOption { get; private set; }

		private void Awake()
		{
			InputManager.OnDeviceDetached += OnDeviceDetached;
		}

		private void OnDeviceDetached(InputDevice obj)
		{
			if (Dialogue.GetCurrent() == null)
			{
				_pause.Return();
			}
		}

		private void OnDestroy()
		{
			InputManager.OnDeviceDetached -= OnDeviceDetached;
		}

		private void Update()
		{
			EventSystem current = EventSystem.current;
			if (current.currentSelectedGameObject == null)
			{
				current.SetSelectedGameObject(_lastSelectedGameObject);
			}
			else
			{
				_lastSelectedGameObject = current.currentSelectedGameObject;
			}
			Dialogue current2 = Dialogue.GetCurrent();
			if (current2 == null)
			{
				if (KeyMapper.Map.Inventory.WasPressed && !PlayerInput.blocked.value && Singleton<Service>.Instance.levelManager.currentChapter.type != Chapter.Type.Tutorial)
				{
					if (_inventory.gameObject.activeSelf)
					{
						_inventory.Close();
					}
					else if (!PlayerInput.blocked.value)
					{
						_inventory.Open();
					}
				}
				if (KeyMapper.Map.Pause.WasPressed)
				{
					_pauseEventSystem.Run();
				}
			}
			else if (current2.closeWithPauseKey && KeyMapper.Map.Pause.WasPressed)
			{
				current2.Close();
			}
			if (testingTool.canUse && KeyMapper.Map.OpenTestingTool.WasPressed)
			{
				testingTool.Toggle();
			}
		}

		public void SetHideOption(HideOption hideOption)
		{
			this.hideOption = hideOption;
			switch (hideOption)
			{
			case HideOption.ShowAll:
				_hidedByHideHud.SetActive(true);
				_hidedByHideAll.SetActive(true);
				break;
			case HideOption.HideHUD:
				_hidedByHideHud.SetActive(false);
				_hidedByHideAll.SetActive(true);
				break;
			case HideOption.HideAll:
				_hidedByHideHud.SetActive(false);
				_hidedByHideAll.SetActive(false);
				break;
			}
		}
	}
}
