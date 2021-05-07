using System;
using Characters;
using Characters.Abilities;
using FX;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level.BlackMarket
{
	public class ChefsFood : MonoBehaviour, IAbility, IAbilityInstance
	{
		[SerializeField]
		private ChefsFoodDisplay _display;

		[SerializeField]
		private Rarity _rarity;

		[SerializeField]
		[FormerlySerializedAs("_healAmount")]
		private int _healingPercent;

		[SerializeField]
		private int _durationMaps = 3;

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private SoundInfo _lootSound;

		[SerializeField]
		private EffectInfo _loopEffect = new EffectInfo
		{
			subordinated = true
		};

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher.Subcomponents _abilityAttacher;

		private ReusableChronoSpriteEffect _loopEffectInstance;

		private int _remainMaps;

		private const string _prefix = "food";

		protected string _keyBase => "food/" + base.name;

		public string displayName => Lingua.GetLocalizedString(_keyBase + "/name");

		public string description => Lingua.GetLocalizedString(_keyBase + "/desc");

		public Rarity rarity => _rarity;

		public int price { get; set; }

		public Character owner { get; private set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached { get; private set; }

		public Sprite icon => _icon;

		public float iconFillAmount => 0f;

		public int iconStacks => _remainMaps;

		public bool expired => false;

		public float duration { get; set; }

		public int iconPriority => 0;

		public bool iconFillInversed => false;

		public bool iconFillFlipped => false;

		public bool removeOnSwapWeapon => false;

		public event Action onSold;

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn -= OnMapChagned;
			}
		}

		public void Loot(Character character)
		{
			_display.gameObject.SetActive(false);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_lootSound, base.transform.position);
			owner = character;
			base.transform.parent = owner.transform;
			base.transform.localPosition = Vector3.zero;
			owner.health.PercentHeal((float)_healingPercent * 0.01f);
			_abilityAttacher.Initialize(owner);
			if (_durationMaps > 0)
			{
				owner.ability.Add(this);
			}
		}

		private void OnMapChagned(Map old, Map @new)
		{
			if (@new.waveContainer.enemyWaves.Length != 0)
			{
				_remainMaps--;
				if (_remainMaps == 0)
				{
					owner.ability.Remove(this);
					_abilityAttacher.StopAttach();
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		public void UpdateTime(float deltaTime)
		{
		}

		public void Refresh()
		{
			remainTime = duration;
		}

		public void Attach()
		{
			attached = true;
			_remainMaps = _durationMaps;
			_loopEffectInstance = ((_loopEffect == null) ? null : _loopEffect.Spawn(owner.transform.position, owner));
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn += OnMapChagned;
			_abilityAttacher.StartAttach();
		}

		public void Detach()
		{
			attached = false;
			if (_loopEffectInstance != null)
			{
				_loopEffectInstance.reusable.Despawn();
			}
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn -= OnMapChagned;
		}

		public void Initialize()
		{
			_display.Initialize(this);
			_display.price = price;
			_display.onLoot += Loot;
			_display.onLoot += delegate
			{
				this.onSold?.Invoke();
			};
		}
	}
}
