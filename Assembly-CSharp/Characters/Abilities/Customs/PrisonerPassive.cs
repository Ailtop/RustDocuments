using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities.CharacterStat;
using Characters.Gear.Weapons.Gauges;
using FX;
using Level;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class PrisonerPassive : Ability, IAbilityInstance
	{
		[Serializable]
		private class Scroll
		{
			[SerializeField]
			private EffectInfo _effect;

			[SerializeField]
			[Range(0f, 100f)]
			private int _weight;

			[SerializeField]
			private bool _brutality;

			[SerializeField]
			private bool _tactics;

			[SerializeField]
			private bool _survival;

			public EffectInfo effect => _effect;

			public int weight => _weight;

			public bool brutality => _brutality;

			public bool tactics => _tactics;

			public bool survival => _survival;

			public Scroll(bool brutality, bool tactics, bool survival)
			{
				_brutality = brutality;
				_tactics = tactics;
				_survival = survival;
			}
		}

		[Header("Walk Easteregg")]
		[SerializeField]
		private CharacterAnimation _characterAnimation;

		[SerializeField]
		private AnimationClip _walk;

		[SerializeField]
		private AnimationClip _walk2;

		[SerializeField]
		private Stat.Values _easterEggStat;

		[SerializeField]
		private float _easterEggDuration;

		private float _remainEasterEggDuration;

		private bool _easterAttached;

		[Space]
		[SerializeField]
		[Range(1f, 100f)]
		private int _possibility;

		[SerializeField]
		private double _totalDamage;

		private double _damageDealt;

		[SerializeField]
		private MotionTypeBoolArray _motionTypes;

		[SerializeField]
		private AttackTypeBoolArray _attackTypes;

		[SerializeField]
		private DamageAttributeBoolArray _attributes;

		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private DroppedCell _cellPrefab;

		[Header("Buff Stats(클릭해서 이동 가능)")]
		[SerializeField]
		private StackableStatBonusComponent _brutalityStat;

		[SerializeField]
		private StackableStatBonusComponent _tacticsStat;

		[SerializeField]
		private StackableStatBonusComponent _survivalStat;

		[Header("Effects and Weights")]
		[SerializeField]
		private SoundInfo _scrollSound;

		[Space]
		[SerializeField]
		private Scroll _brutality;

		[SerializeField]
		private Scroll _tactics;

		[SerializeField]
		private Scroll _survival;

		[Space]
		[SerializeField]
		private Scroll _minotaurus;

		[SerializeField]
		private Scroll _assassin;

		[SerializeField]
		private Scroll _guardian;

		[Space]
		[SerializeField]
		private Scroll _epic;

		private Scroll[] _scrolls;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool attached => true;

		public Sprite icon => _defaultIcon;

		public float iconFillAmount => 0f;

		public int iconStacks { get; protected set; }

		public bool expired => false;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		private void AttachEasterEgg()
		{
			if (!_easterAttached)
			{
				_easterAttached = true;
				_remainEasterEggDuration = _easterEggDuration;
				owner.stat.AttachValues(_easterEggStat);
				_characterAnimation.SetWalk(_walk2);
			}
		}

		private void DetachEasterEgg()
		{
			if (_easterAttached)
			{
				_easterAttached = false;
				owner.stat.DetachValues(_easterEggStat);
				_characterAnimation.SetWalk(_walk);
			}
		}

		public void UpdateTime(float deltaTime)
		{
			if (_easterAttached)
			{
				_remainEasterEggDuration -= deltaTime;
				if (_remainEasterEggDuration < 0f)
				{
					DetachEasterEgg();
				}
			}
		}

		public void Refresh()
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			_scrolls = new Scroll[7] { _brutality, _tactics, _survival, _minotaurus, _assassin, _guardian, _epic };
		}

		public void Attach()
		{
			_damageDealt = _totalDamage;
			Character character = owner;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			_gauge.onChanged += OnGaugeChanged;
		}

		public void Detach()
		{
			Character character = owner;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			_gauge.onChanged -= OnGaugeChanged;
		}

		private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			if (!_motionTypes[gaveDamage.motionType] || !_attackTypes[gaveDamage.attackType] || !_attributes[gaveDamage.attribute] || target.character == null || target.character.type == Character.Type.Dummy || target.character.type == Character.Type.Trap)
			{
				return;
			}
			double damageDealt2 = _damageDealt;
			Damage damage = gaveDamage;
			_damageDealt = damageDealt2 + damage.amount;
			while (_damageDealt > _totalDamage)
			{
				_damageDealt -= _totalDamage;
				if (MMMaths.PercentChance(_possibility))
				{
					_cellPrefab.Spawn(gaveDamage.hitPoint, _gauge);
				}
			}
		}

		private Scroll GetScrollToObtain()
		{
			int max = _scrolls.Select((Scroll scroll) => scroll.weight).Sum();
			int num = UnityEngine.Random.Range(0, max) + 1;
			for (int i = 0; i < _scrolls.Length; i++)
			{
				num -= _scrolls[i].weight;
				if (num <= 0)
				{
					return _scrolls[i];
				}
			}
			Debug.LogError("Scroll index is exceeded!");
			return _scrolls.Random();
		}

		private void OnGaugeChanged(float oldValue, float newValue)
		{
			if (!(newValue < _gauge.maxValue))
			{
				AttachEasterEgg();
				_gauge.Clear();
				PersistentSingleton<SoundManager>.Instance.PlaySound(_scrollSound, owner.transform.position);
				Scroll scrollToObtain = GetScrollToObtain();
				scrollToObtain.effect.Spawn(owner.transform.position, owner);
				if (scrollToObtain.brutality)
				{
					owner.ability.Add(_brutalityStat.ability);
				}
				if (scrollToObtain.tactics)
				{
					owner.ability.Add(_tacticsStat.ability);
				}
				if (scrollToObtain.survival)
				{
					owner.ability.Add(_survivalStat.ability);
				}
			}
		}
	}
}
