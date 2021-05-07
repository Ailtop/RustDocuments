using System;
using Data;
using Level;
using UnityEngine;

namespace Characters.Gear
{
	public abstract class Gear : MonoBehaviour
	{
		public enum Type
		{
			Weapon,
			Item,
			Quintessence
		}

		public enum State
		{
			Dropped,
			Equipped
		}

		[Flags]
		public enum Tag
		{
			Carleon = 0x1,
			Skeleton = 0x2,
			Spirit = 0x4,
			Temp = 0x8
		}

		protected Action _onDiscard;

		[Space]
		[Tooltip("입수가능")]
		public bool obtainable = true;

		[Tooltip("파괴가능")]
		public bool destructible = true;

		[Tooltip("해금해야 드랍되는지, obtainable이 false이면 어쨌든 입수 불가능이므로 주의")]
		public bool needUnlock;

		[SerializeField]
		private Sprite _unlockIcon;

		[Space]
		[SerializeField]
		private Rarity _rarity;

		[SerializeField]
		[EnumFlag]
		private Tag _gearTag;

		[Space]
		[SerializeField]
		protected Stat.Values _stat;

		[SerializeField]
		private DroppedGear _dropped;

		[SerializeField]
		private GameObject _equipped;

		[Space]
		[SerializeField]
		private string[] _setItemKeys;

		[SerializeField]
		private Sprite _setItemImage;

		[SerializeField]
		private RuntimeAnimatorController _setItemAnimator;

		private State _state;

		public Sprite unlockIcon
		{
			get
			{
				if (!(_unlockIcon == null))
				{
					return _unlockIcon;
				}
				return Resource.instance.GetItemBuffIcon(base.name);
			}
		}

		public abstract Type type { get; }

		public Rarity rarity => _rarity;

		public Tag gearTag => _gearTag;

		public Stat.Values stat => _stat;

		public DroppedGear dropped => _dropped;

		public GameObject equipped => _equipped;

		public bool lootable { get; set; } = true;


		public string[] setItemKeys => _setItemKeys;

		public Sprite setItemImage => _setItemImage;

		public RuntimeAnimatorController setItemAnimator => _setItemAnimator;

		protected abstract string _prefix { get; }

		protected string _keyBase => _prefix + "/" + base.name;

		public string displayNameKey => _keyBase + "/name";

		public string displayName => Lingua.GetLocalizedString(_keyBase + "/name");

		public string description => Lingua.GetLocalizedString(_keyBase + "/desc");

		public string flavor => Lingua.GetLocalizedString(_keyBase + "/flavor");

		public string typeDisplayName => Lingua.GetLocalizedString("label/" + _prefix + "/name");

		public bool hasFlavor => !string.IsNullOrWhiteSpace(flavor);

		public Sprite icon => dropped.spriteRenderer.sprite;

		public virtual Sprite thumbnail => Resource.instance.GetGearThumbnail(base.name) ?? icon;

		public virtual GameData.Currency.Type currencyTypeByDiscard => GameData.Currency.Type.Gold;

		public virtual int currencyByDiscard => 0;

		public State state
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state != value)
				{
					_state = value;
					switch (_state)
					{
					case State.Dropped:
						OnDropped();
						break;
					case State.Equipped:
						OnEquipped();
						break;
					}
				}
			}
		}

		public Character owner { get; protected set; }

		public event Action onDropped;

		public event Action onEquipped;

		public event Action onDiscard
		{
			add
			{
				_onDiscard = (Action)Delegate.Combine(_onDiscard, value);
			}
			remove
			{
				_onDiscard = (Action)Delegate.Remove(_onDiscard, value);
			}
		}

		protected virtual void Awake()
		{
			if (_dropped != null)
			{
				_dropped.onLoot += OnLoot;
			}
			OnDropped();
		}

		public virtual void Initialize()
		{
			_dropped?.Initialize(this);
		}

		protected abstract void OnLoot(Character character);

		protected virtual void OnDropped()
		{
			base.transform.parent = Map.Instance.transform;
			base.transform.localScale = Vector3.one;
			if (_equipped != null)
			{
				_equipped.SetActive(false);
			}
			if (_dropped != null)
			{
				_dropped.gameObject.SetActive(true);
			}
			this.onDropped?.Invoke();
		}

		protected virtual void OnEquipped()
		{
			if (_dropped != null)
			{
				_dropped.gameObject.SetActive(false);
			}
			if (_equipped != null)
			{
				_equipped.SetActive(true);
			}
			this.onEquipped?.Invoke();
		}
	}
}
