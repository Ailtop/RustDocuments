using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Characters;
using Characters.Gear;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Characters.Player;
using Data;
using InControl;
using Level;
using Scenes;
using UnityEngine;

public class Resource : ScriptableObject
{
	public class Request<T> where T : UnityEngine.Object
	{
		private readonly ResourceRequest _request;

		public T asset { get; private set; }

		public bool isDone => _request.isDone;

		public Request(string path)
		{
			_request = Resources.LoadAsync<T>(path);
			_request.completed += delegate
			{
				asset = (T)_request.asset;
			};
		}
	}

	[Serializable]
	public class MapReference
	{
		public Map.Type type;

		public SpecialMap.Type specialMapType;

		public string path;

		public bool empty => string.IsNullOrWhiteSpace(path);

		public Request<Map> LoadAsync()
		{
			return new Request<Map>(path);
		}

		public Map Load()
		{
			return Resources.Load<Map>(path);
		}

		public static MapReference FromPath(string path)
		{
			return new MapReference
			{
				path = path
			};
		}
	}

	public abstract class GearReference
	{
		public string name;

		public string path;

		public bool obtainable;

		public bool needUnlock;

		public Sprite unlockIcon;

		public Rarity rarity;

		public Sprite icon;

		public Sprite thumbnail;

		public string displayNameKey;

		public abstract Gear.Type type { get; }

		public bool unlocked
		{
			get
			{
				if (!needUnlock)
				{
					return true;
				}
				return GameData.Gear.IsUnlocked(type.ToString(), name);
			}
		}

		public void Unlock()
		{
			if (!GameData.Gear.IsUnlocked(type.ToString(), name))
			{
				GameData.Gear.SetUnlocked(type.ToString(), name, true);
				Scene<GameBase>.instance.uiManager.unlockNotice.Show(unlockIcon, Lingua.GetLocalizedString(displayNameKey));
			}
		}

		public Request<Gear> LoadAsync()
		{
			return new Request<Gear>(path);
		}
	}

	[Serializable]
	public class WeaponReference : GearReference
	{
		public override Gear.Type type => Gear.Type.Weapon;

		public new Request<Weapon> LoadAsync()
		{
			return new Request<Weapon>(path);
		}
	}

	[Serializable]
	public class QuintessenceInfo : GearReference
	{
		public override Gear.Type type => Gear.Type.Quintessence;

		public new Request<Quintessence> LoadAsync()
		{
			return new Request<Quintessence>(path);
		}
	}

	[Serializable]
	public class ItemInfo : GearReference
	{
		public override Gear.Type type => Gear.Type.Item;

		public new Request<Item> LoadAsync()
		{
			return new Request<Item>(path);
		}
	}

	public const string assets = "Assets";

	public const string resources = "Resources";

	private const string audio = "Audio/";

	public const string sfx = "Audio/Sfx/";

	public const string music = "Audio/Music/";

	public const string level = "Assets/Resources/Level/";

	public const string levelCastle = "Assets/Resources/Level/Castle/";

	public const string levelTest = "Assets/Resources/Level/Test/";

	public const string levelChapter1 = "Assets/Resources/Level/Chapter1/";

	public const string levelChapter2 = "Assets/Resources/Level/Chapter2/";

	public const string levelChapter3 = "Assets/Resources/Level/Chapter3/";

	public const string levelChapter4 = "Assets/Resources/Level/Chapter4/";

	public const string levelChapter5 = "Assets/Resources/Level/Chapter5/";

	public const string strings = "Strings";

	public const string shaders = "Shaders/";

	public const string enemy = "Assets/Enemies/";

	public const string enemyElite = "Assets/Enemies/Elite/";

	public const string enemyBoss = "Assets/Enemies/Boss/";

	public const string enemyChapter1 = "Assets/Enemies/Chapter1/";

	public const string enemyChapter2 = "Assets/Enemies/Chapter2/";

	public const string enemyChapter3 = "Assets/Enemies/Chapter3/";

	public const string enemyChapter4 = "Assets/Enemies/Chapter4/";

	public const string parts = "Parts/";

	public const string followers = "Followers/";

	public const string monsters = "Monsters/";

	public const string weaponDirectory = "Gear/Weapons/";

	public const string itemDirectory = "Gear/Items/";

	public const string quintessenceDirectory = "Gear/Quintessences/";

	private static Resource _instance;

	public ParticleEffectInfo hitParticle;

	public ParticleEffectInfo reassembleParticle;

	[SerializeField]
	private Character _player;

	[SerializeField]
	private PlayerDieHeadParts _playerDieHeadParts;

	[Space]
	[SerializeField]
	private ParticleEffectInfo _freezeLargeParticle;

	[SerializeField]
	private ParticleEffectInfo _freezeMediumParticle;

	[SerializeField]
	private ParticleEffectInfo _freezeMediumParticle2;

	[SerializeField]
	private ParticleEffectInfo _freezeSmallParticle;

	[Space]
	[SerializeField]
	private RuntimeAnimatorController _freezeLarge;

	[SerializeField]
	private RuntimeAnimatorController _freezeMedium1;

	[SerializeField]
	private RuntimeAnimatorController _freezeMedium2;

	[SerializeField]
	private RuntimeAnimatorController _freezeSmall;

	[Space]
	[SerializeField]
	private Potion _smallPotion;

	[SerializeField]
	private Potion _mediumPotion;

	[SerializeField]
	private Potion _largePotion;

	[Space]
	[SerializeField]
	private Sprite _flexibleSpineIcon;

	[SerializeField]
	private Sprite _soulAccelerationIcon;

	[SerializeField]
	private Sprite _reassembleIcon;

	[SerializeField]
	private PoolObject _emptyEffect;

	[SerializeField]
	private PoolObject _vignetteEffect;

	[SerializeField]
	private PoolObject _screenFlashEffect;

	[SerializeField]
	private Sprite _curseOfLightIcon;

	[Space]
	[SerializeField]
	private RuntimeAnimatorController _curseOfLightAttachEffect;

	[SerializeField]
	private RuntimeAnimatorController _enemyInSightEffect;

	[SerializeField]
	private RuntimeAnimatorController _enemyAppearanceEffect;

	[SerializeField]
	private RuntimeAnimatorController _poisonEffect;

	[SerializeField]
	private RuntimeAnimatorController _slowEffect;

	[SerializeField]
	private RuntimeAnimatorController _bindingEffect;

	[SerializeField]
	private RuntimeAnimatorController _bleedEffect;

	[SerializeField]
	private RuntimeAnimatorController _stunEffect;

	[SerializeField]
	private RuntimeAnimatorController _swapEffect;

	[Space]
	[SerializeField]
	private PoolObject _goldParticle;

	[SerializeField]
	private PoolObject _darkQuartzParticle;

	[SerializeField]
	private PoolObject _boneParticle;

	[SerializeField]
	private PoolObject _droppedSkulHead;

	[SerializeField]
	private Sprite _pixelSprite;

	[SerializeField]
	private Sprite _emptySprite;

	[SerializeField]
	private SpriteRenderer _footShadow;

	[SerializeField]
	[HideInInspector]
	private MapReference _blackMarketMapReference;

	[SerializeField]
	private Sprite[] _gearThumbnails;

	private Dictionary<string, Sprite> _gearThumbnailDictionary;

	[SerializeField]
	private Sprite[] _weaponHudMainIcons;

	private Dictionary<string, Sprite> _weaponHudMainIconDictionary;

	[SerializeField]
	private Sprite[] _weaponHudSubIcons;

	private Dictionary<string, Sprite> _weaponHudSubIconDictionary;

	[SerializeField]
	private Sprite[] _quintessenceSilhouettes;

	private Dictionary<string, Sprite> _quintessenceSilhouetteDictionary;

	[SerializeField]
	private Sprite[] _quintessenceHudIcons;

	private Dictionary<string, Sprite> _quintessenceHudIconDictionary;

	[SerializeField]
	private Sprite[] _skillIcons;

	private Dictionary<string, Sprite> _skillIconDictionary;

	[SerializeField]
	private Sprite[] _itemBuffIcons;

	private Dictionary<string, Sprite> _itemBuffIconDictionary;

	[Space]
	[SerializeField]
	private Sprite[] _keyboardButtons;

	private Dictionary<string, Sprite> _keyboardButtonDictionary;

	[SerializeField]
	private Sprite[] _keyboardButtonsOutline;

	private Dictionary<string, Sprite> _keyboardButtonOutlineDictionary;

	[SerializeField]
	private Sprite[] _mouseButtons;

	private Dictionary<string, Sprite> _mouseButtonDictionary;

	[SerializeField]
	private Sprite[] _mouseButtonsOutline;

	private Dictionary<string, Sprite> _mouseButtonOutlineDictionary;

	[SerializeField]
	private Sprite[] _controllerButtons;

	private Dictionary<string, Sprite> _controllerButtonDictionary;

	[SerializeField]
	private Sprite[] _controllerButtonsOutline;

	private Dictionary<string, Sprite> _controllerButtonOutlineDictionary;

	[Space]
	[SerializeField]
	private Sprite[] _keywordIcons;

	public WeaponReference[] weapons;

	public ItemInfo[] items;

	public QuintessenceInfo[] quintessences;

	public Material[] materials;

	public Chapter[] chapters;

	public static Resource instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<Resource>("Resource");
				_instance.Initialize();
			}
			return _instance;
		}
	}

	public Character player => _player;

	public PlayerDieHeadParts playerDieHeadParts => _playerDieHeadParts;

	public ParticleEffectInfo freezeLargeParticle => _freezeLargeParticle;

	public ParticleEffectInfo freezeMediumParticle => _freezeMediumParticle;

	public ParticleEffectInfo freezeMediumParticle2 => _freezeMediumParticle2;

	public ParticleEffectInfo freezeSmallParticle => _freezeSmallParticle;

	public Potion smallPotion => _smallPotion;

	public Potion mediumPotion => _mediumPotion;

	public Potion largePotion => _largePotion;

	public EnumArray<Potion.Size, Potion> potions { get; private set; }

	public Sprite flexibleSpineIcon => _flexibleSpineIcon;

	public Sprite soulAccelerationIcon => _soulAccelerationIcon;

	public Sprite reassembleIcon => _reassembleIcon;

	public PoolObject emptyEffect => _emptyEffect;

	public PoolObject vignetteEffect => _vignetteEffect;

	public PoolObject screenFlashEffect => _screenFlashEffect;

	public Sprite curseOfLightIcon => _curseOfLightIcon;

	public RuntimeAnimatorController curseOfLightAttachEffect => _curseOfLightAttachEffect;

	public RuntimeAnimatorController enemyInSightEffect => _enemyInSightEffect;

	public RuntimeAnimatorController enemyAppearanceEffect => _enemyAppearanceEffect;

	public RuntimeAnimatorController poisonEffect => _poisonEffect;

	public RuntimeAnimatorController slowEffect => _slowEffect;

	public RuntimeAnimatorController bindingEffect => _bindingEffect;

	public RuntimeAnimatorController bleedEffect => _bleedEffect;

	public RuntimeAnimatorController stunEffect => _stunEffect;

	public RuntimeAnimatorController swapEffect => _swapEffect;

	public PoolObject goldParticle => _goldParticle;

	public PoolObject darkQuartzParticle => _darkQuartzParticle;

	public PoolObject boneParticle => _boneParticle;

	public PoolObject droppedSkulHead => _droppedSkulHead;

	public Sprite pixelSprite => _pixelSprite;

	public Sprite emptySprite => _emptySprite;

	public SpriteRenderer footShadow => _footShadow;

	public MapReference blackMarketMapReference => _blackMarketMapReference;

	public Dictionary<string, Sprite> keywordIconDictionary { get; private set; }

	public Dictionary<string, WeaponReference> weaponDictionary { get; private set; }

	public Dictionary<string, ItemInfo> itemDictionary { get; private set; }

	public Dictionary<string, QuintessenceInfo> quintessenceDictionary { get; private set; }

	public Dictionary<string, Material> materialDictionary { get; private set; }

	public static string AssetPathToResourcesPath(string path)
	{
		return path.ToLowerInvariant().Replace(".prefab", string.Empty).Replace('\\', '/')
			.Replace("Assets/".ToLowerInvariant(), string.Empty)
			.Replace("Resources/".ToLowerInvariant(), string.Empty);
	}

	public RuntimeAnimatorController GetFreezeAnimator(Vector2 pixelSize)
	{
		_003C_003Ec__DisplayClass62_0 _003C_003Ec__DisplayClass62_ = default(_003C_003Ec__DisplayClass62_0);
		_003C_003Ec__DisplayClass62_.pixelSize = pixelSize;
		int num = 1;
		while (true)
		{
			if (_003CGetFreezeAnimator_003Eg__Fit_007C62_0(40 * num, 50 * num, ref _003C_003Ec__DisplayClass62_))
			{
				return _freezeSmall;
			}
			if (_003CGetFreezeAnimator_003Eg__Fit_007C62_0(64 * num, 66 * num, ref _003C_003Ec__DisplayClass62_))
			{
				return _freezeMedium1;
			}
			if (_003CGetFreezeAnimator_003Eg__Fit_007C62_0(79 * num, 71 * num, ref _003C_003Ec__DisplayClass62_))
			{
				return _freezeMedium2;
			}
			if (_003CGetFreezeAnimator_003Eg__Fit_007C62_0(125 * num, 120 * num, ref _003C_003Ec__DisplayClass62_))
			{
				break;
			}
			num++;
		}
		return _freezeLarge;
	}

	public PoolObject GetCurrencyParticle(GameData.Currency.Type type)
	{
		switch (type)
		{
		case GameData.Currency.Type.Gold:
			return _goldParticle;
		case GameData.Currency.Type.DarkQuartz:
			return _darkQuartzParticle;
		case GameData.Currency.Type.Bone:
			return _boneParticle;
		default:
			return null;
		}
	}

	public Sprite GetGearThumbnail(string name)
	{
		Sprite value;
		_gearThumbnailDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetWeaponHudMainIcon(string name)
	{
		Sprite value;
		_weaponHudMainIconDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetWeaponHudSubIcon(string name)
	{
		Sprite value;
		_weaponHudSubIconDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetQuintessenceSilhouette(string name)
	{
		Sprite value;
		_quintessenceSilhouetteDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetQuintessenceHudIcon(string name)
	{
		Sprite value;
		_quintessenceHudIconDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetSkillIcon(string name)
	{
		Sprite value;
		_skillIconDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetItemBuffIcon(string name)
	{
		Sprite value;
		_itemBuffIconDictionary.TryGetValue(name, out value);
		return value;
	}

	public Sprite GetKeyIconOrDefault(BindingSource bindingSource, bool outline = false)
	{
		Sprite sprite;
		if (TryGetKeyIcon(bindingSource, out sprite, outline))
		{
			return sprite;
		}
		return (outline ? _controllerButtonOutlineDictionary : _controllerButtonDictionary)["unknown"];
	}

	public bool TryGetKeyIcon(BindingSource bindingSource, out Sprite sprite, bool outline = false)
	{
		if ((object)bindingSource != null)
		{
			KeyBindingSource keyBindingSource;
			if ((object)(keyBindingSource = bindingSource as KeyBindingSource) != null)
			{
				KeyBindingSource keyBindingSource2 = keyBindingSource;
				return (outline ? _keyboardButtonOutlineDictionary : _keyboardButtonDictionary).TryGetValue(keyBindingSource2.Control.GetInclude(0).ToString().Trim(), out sprite);
			}
			MouseBindingSource mouseBindingSource;
			if ((object)(mouseBindingSource = bindingSource as MouseBindingSource) != null)
			{
				MouseBindingSource mouseBindingSource2 = mouseBindingSource;
				return (outline ? _mouseButtonOutlineDictionary : _mouseButtonDictionary).TryGetValue(mouseBindingSource2.Control.ToString(), out sprite);
			}
			DeviceBindingSource deviceBindingSource;
			if ((object)(deviceBindingSource = bindingSource as DeviceBindingSource) != null)
			{
				DeviceBindingSource deviceBindingSource2 = deviceBindingSource;
				string text = deviceBindingSource2.Control.ToString();
				if (deviceBindingSource2.Control == InputControlType.Start || deviceBindingSource2.Control == InputControlType.Options)
				{
					text = InputControlType.Menu.ToString();
				}
				if (deviceBindingSource2.Control == InputControlType.Back || deviceBindingSource2.Control == InputControlType.View || deviceBindingSource2.Control == InputControlType.Share || deviceBindingSource2.Control == InputControlType.Pause || deviceBindingSource2.Control == InputControlType.Command)
				{
					text = InputControlType.Select.ToString();
				}
				Dictionary<string, Sprite> dictionary = (outline ? _controllerButtonOutlineDictionary : _controllerButtonDictionary);
				if ((deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStation2 || deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStation3 || deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStation4 || deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStation5 || deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStationMove || deviceBindingSource2.DeviceStyle == InputDeviceStyle.PlayStationVita) && dictionary.TryGetValue("PS_" + text, out sprite))
				{
					return sprite;
				}
				return dictionary.TryGetValue(text, out sprite);
			}
		}
		sprite = null;
		return false;
	}

	public static T[] LoadAll<T>(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : UnityEngine.Object
	{
		return new T[0];
	}

	public static KeyValuePair<string, T>[] LoadAllWithPath<T>(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : UnityEngine.Object
	{
		return new KeyValuePair<string, T>[0];
	}

	public void Load()
	{
	}

	private void Initialize()
	{
		potions = new EnumArray<Potion.Size, Potion>(_smallPotion, _mediumPotion, _largePotion);
		_gearThumbnailDictionary = _gearThumbnails.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_weaponHudMainIconDictionary = _weaponHudMainIcons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_weaponHudSubIconDictionary = _weaponHudSubIcons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_quintessenceSilhouetteDictionary = _quintessenceSilhouettes.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_quintessenceHudIconDictionary = _quintessenceHudIcons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_skillIconDictionary = _skillIcons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_itemBuffIconDictionary = _itemBuffIcons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_keyboardButtonDictionary = _keyboardButtons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_keyboardButtonOutlineDictionary = _keyboardButtonsOutline.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_mouseButtonDictionary = _mouseButtons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_mouseButtonOutlineDictionary = _mouseButtonsOutline.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_controllerButtonDictionary = _controllerButtons.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		_controllerButtonOutlineDictionary = _controllerButtonsOutline.ToDictionary((Sprite sprite) => sprite.name, StringComparer.OrdinalIgnoreCase);
		weaponDictionary = weapons.ToDictionary((WeaponReference weapon) => weapon.name, StringComparer.OrdinalIgnoreCase);
		itemDictionary = items.ToDictionary((ItemInfo item) => item.name, StringComparer.OrdinalIgnoreCase);
		quintessenceDictionary = quintessences.ToDictionary((QuintessenceInfo quintessence) => quintessence.name, StringComparer.OrdinalIgnoreCase);
		materialDictionary = materials.ToDictionary((Material material) => material.name, StringComparer.OrdinalIgnoreCase);
		keywordIconDictionary = _keywordIcons.ToDictionary((Sprite sprite) => sprite.name);
	}
}
