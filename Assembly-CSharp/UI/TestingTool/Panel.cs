using System;
using System.Collections.ObjectModel;
using Characters;
using Characters.Controllers;
using Data;
using Level;
using Scenes;
using Services;
using Singletons;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class Panel : Dialogue
	{
		public class GearOptionData : OptionData
		{
			public readonly Resource.GearReference gearInfo;

			public GearOptionData(Resource.GearReference gearInfo, Sprite image)
				: this(gearInfo.name, image)
			{
				this.gearInfo = gearInfo;
			}
		}

		public enum Type
		{
			Main,
			MapList,
			GearList,
			Log
		}

		public static readonly ReadOnlyCollection<ulong> steamIDWhitelist = new ReadOnlyCollection<ulong>(new ulong[44]
		{
			76561199059728330uL, 76561197996006680uL, 76561198092253165uL, 76561198334264534uL, 76561198250024700uL, 76561198860098191uL, 76561198319657514uL, 76561199027260004uL, 76561198051885564uL, 76561198155119982uL,
			76561198069422371uL, 76561198801683767uL, 76561198057413833uL, 76561198088581028uL, 76561198413465784uL, 76561198097448980uL, 76561199072337405uL, 76561198067693373uL, 76561197996097052uL, 76561198256555413uL,
			76561198360404950uL, 76561198432064966uL, 76561198970788085uL, 76561198148466847uL, 76561198323030273uL, 76561199032674878uL, 76561197988893851uL, 76561199077667317uL, 76561199077534192uL, 76561199107829576uL,
			76561199131395716uL, 76561199077382271uL, 76561199118040463uL, 76561199117749825uL, 76561199118218379uL, 76561199117571392uL, 76561199127002825uL, 76561199127173988uL, 76561199127176461uL, 76561198439696608uL,
			76561199077382271uL, 76561197969210993uL, 76561198263114677uL, 76561199102356836uL
		});

		[SerializeField]
		private TMP_Text _mapName;

		[SerializeField]
		private TMP_Text _version;

		[Space]
		[SerializeField]
		private GameObject _main;

		[SerializeField]
		private GameObject _mapList;

		[SerializeField]
		private GameObject _gearList;

		[SerializeField]
		private Log _log;

		[SerializeField]
		private UnityEngine.UI.Button _awake;

		private EnumArray<Type, GameObject> _panels;

		[Space]
		[SerializeField]
		private UnityEngine.UI.Button _openMapList;

		[SerializeField]
		private UnityEngine.UI.Button _chapter1;

		[SerializeField]
		private UnityEngine.UI.Button _chapter2;

		[SerializeField]
		private UnityEngine.UI.Button _chapter3;

		[SerializeField]
		private UnityEngine.UI.Button _chapter4;

		[SerializeField]
		private UnityEngine.UI.Button _chapter5;

		[SerializeField]
		private UnityEngine.UI.Button _nextStage;

		[SerializeField]
		private UnityEngine.UI.Button _nextMap;

		[Space]
		[SerializeField]
		private UnityEngine.UI.Button _openGearList;

		[SerializeField]
		private UnityEngine.UI.Button _hideUI;

		[Space]
		[SerializeField]
		private UnityEngine.UI.Button _getGold;

		[SerializeField]
		private UnityEngine.UI.Button _getDarkquartz;

		[SerializeField]
		private UnityEngine.UI.Button _getBone;

		[SerializeField]
		private UnityEngine.UI.Button _resetItem;

		[SerializeField]
		private UnityEngine.UI.Button _resetProgress;

		[Space]
		[SerializeField]
		private UnityEngine.UI.Button _right3;

		[SerializeField]
		private UnityEngine.UI.Button _damageBuff;

		[SerializeField]
		private UnityEngine.UI.Button _noCooldown;

		[SerializeField]
		private UnityEngine.UI.Button _hp10k;

		[SerializeField]
		private UnityEngine.UI.Button _rerollSkill;

		[Space]
		[SerializeField]
		private TMP_Text _localNow;

		[SerializeField]
		private TMP_Text _utcNow;

		private bool _damageBuffAttached;

		private bool _noCooldownAttached;

		private bool _hp10kAttached;

		private Stat.Values _damageBuffStat = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.AttackDamage, 100.0));

		private Stat.Values _cooldownBuffStat = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.CooldownSpeed, 100.0));

		private Stat.Values _hp10kStat = new Stat.Values(new Stat.Value(Stat.Category.Constant, Stat.Kind.Health, 9900.0));

		public bool canUse => steamIDWhitelist.Contains(SteamUser.GetSteamID().m_SteamID);

		public override bool closeWithPauseKey => true;

		public void Open(Type type)
		{
			foreach (GameObject panel in _panels)
			{
				panel.SetActive(false);
			}
			_panels[type].SetActive(true);
		}

		public void OpenMain()
		{
			Open(Type.Main);
		}

		public void OpenMapList()
		{
			Open(Type.MapList);
		}

		public void OpenGearList()
		{
			Open(Type.GearList);
		}

		public void OpenLog()
		{
			Open(Type.Log);
		}

		private void Awake()
		{
			_panels = new EnumArray<Type, GameObject>(_main, _mapList, _gearList, _log.gameObject);
			_log.StartLog();
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			Resource instance = Resource.instance;
			_version.text = "version : " + Application.version;
			_awake.onClick.AddListener(delegate
			{
				levelManager.player.playerComponents.inventory.weapon.UpgradeCurrentWeapon();
			});
			_chapter1.onClick.AddListener(delegate
			{
				levelManager.Load(Chapter.Type.Chapter1);
			});
			_chapter2.onClick.AddListener(delegate
			{
				levelManager.Load(Chapter.Type.Chapter2);
			});
			_chapter3.onClick.AddListener(delegate
			{
				levelManager.Load(Chapter.Type.Chapter3);
			});
			_chapter4.onClick.AddListener(delegate
			{
				levelManager.Load(Chapter.Type.Chapter4);
			});
			_chapter5.onClick.AddListener(delegate
			{
				levelManager.Load(Chapter.Type.Chapter5);
			});
			_nextStage.onClick.AddListener(delegate
			{
				levelManager.LoadNextStage();
			});
			_nextMap.onClick.AddListener(delegate
			{
				levelManager.LoadNextMap();
			});
			_hideUI.onClick.AddListener(delegate
			{
				Scene<GameBase>.instance.uiManager.headupDisplay.visible = !Scene<GameBase>.instance.uiManager.headupDisplay.visible;
			});
			_getGold.onClick.AddListener(delegate
			{
				GameData.Currency.gold.Earn(10000);
			});
			_getDarkquartz.onClick.AddListener(delegate
			{
				GameData.Currency.darkQuartz.Earn(1000);
			});
			_getBone.onClick.AddListener(delegate
			{
				GameData.Currency.bone.Earn(100);
			});
			_resetItem.onClick.AddListener(delegate
			{
				levelManager.player.playerComponents.inventory.item.RemoveAll();
			});
			_resetProgress.onClick.AddListener(delegate
			{
				GameData.Generic.ResetAll();
				GameData.Currency.ResetAll();
				GameData.Progress.ResetAll();
				GameData.Gear.ResetAll();
			});
			_right3.onClick.AddListener(delegate
			{
				for (int i = 0; i < 3; i++)
				{
					_damageBuff.onClick.Invoke();
					_noCooldown.onClick.Invoke();
					_hp10k.onClick.Invoke();
				}
			});
			_damageBuff.onClick.AddListener(delegate
			{
				_damageBuffAttached = !_damageBuffAttached;
				if (_damageBuffAttached)
				{
					levelManager.player.stat.AttachValues(_damageBuffStat);
				}
				else
				{
					levelManager.player.stat.DetachValues(_damageBuffStat);
				}
			});
			_noCooldown.onClick.AddListener(delegate
			{
				_noCooldownAttached = !_noCooldownAttached;
				if (_noCooldownAttached)
				{
					levelManager.player.stat.AttachValues(_cooldownBuffStat);
				}
				else
				{
					levelManager.player.stat.DetachValues(_cooldownBuffStat);
				}
			});
			_hp10k.onClick.AddListener(delegate
			{
				_hp10kAttached = !_hp10kAttached;
				if (_hp10kAttached)
				{
					levelManager.player.stat.AttachValues(_hp10kStat);
					levelManager.player.health.ResetToMaximumHealth();
				}
				else
				{
					levelManager.player.stat.DetachValues(_hp10kStat);
				}
			});
			_rerollSkill.onClick.AddListener(delegate
			{
				levelManager.player.playerComponents.inventory.weapon.current.RerollSkills();
			});
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			_mapName.text = string.Format("Map : {0}/{1}/{2}", levelManager.currentChapter.type, levelManager.currentChapter.currentStage.name, Map.Instance.name.Replace(" (Clone)", ""));
			_localNow.text = $"Local now : {DateTime.Now}";
			_utcNow.text = $"Utc now : {DateTime.UtcNow}";
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PlayerInput.blocked.Detach(this);
			Chronometer.global.DetachTimeScale(this);
		}
	}
}
