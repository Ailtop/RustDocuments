using System.Collections;
using Characters.Abilities;
using Characters.Movements;
using FX;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Soar : Keyword
	{
		protected class StatBonus : IAbility, IAbilityInstance
		{
			private readonly EffectInfo _gettingStackEffect;

			private readonly EffectInfo _losingEffect;

			private readonly EffectInfo _loopEffect;

			private ReusableChronoSpriteEffect _loopEffectInstance;

			private readonly Sprite _icon;

			public double damagePerStack;

			private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.PhysicalAttackDamage, 0.0), new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MagicAttackDamage, 0.0));

			private Character _owner;

			private int _stacks;

			private bool _wasGrounded;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon
			{
				get
				{
					if (_stacks <= 0)
					{
						return null;
					}
					return _icon;
				}
			}

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => (int)(damagePerStack * (double)_stacks * 100.0);

			public bool expired => false;

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public StatBonus(Character owner, Sprite icon, EffectInfo gettingStackEffect, EffectInfo losingEffect, EffectInfo loopEffect)
			{
				_owner = owner;
				_icon = icon;
				_gettingStackEffect = gettingStackEffect;
				_losingEffect = losingEffect;
				_loopEffect = loopEffect;
			}

			public void Initialize()
			{
				_wasGrounded = true;
			}

			private void SpawnLoopEffect()
			{
				if (_loopEffectInstance == null)
				{
					_loopEffectInstance = _loopEffect.Spawn(_owner.transform.position, _owner);
				}
			}

			private void DespawnLoopEffect()
			{
				if (_loopEffectInstance != null)
				{
					_loopEffectInstance.reusable.Despawn();
					_loopEffectInstance = null;
				}
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					DespawnLoopEffect();
					_losingEffect.Spawn(_owner.transform.position, _owner);
					_stacks = 0;
					UpdateStat();
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				_owner.stat.AttachValues(_stat);
				_owner.movement.onJump += OnJump;
				_owner.movement.onGrounded += OnGrounded;
			}

			void IAbilityInstance.Detach()
			{
				_owner.stat.DetachValues(_stat);
				_owner.movement.onJump -= OnJump;
				DespawnLoopEffect();
			}

			private void OnGrounded()
			{
				if (!(remainTime < duration))
				{
					_wasGrounded = true;
					remainTime = duration;
				}
			}

			private void OnJump(Movement.JumpType jumpType, float jumpHeight)
			{
				if (_wasGrounded)
				{
					_stacks = 1;
					_wasGrounded = false;
				}
				else
				{
					_stacks++;
				}
				SpawnLoopEffect();
				_gettingStackEffect.Spawn(_owner.transform.position, _owner);
				remainTime = float.PositiveInfinity;
				UpdateStat();
			}

			public void UpdateStat()
			{
				_stat.values[0].value = damagePerStack * (double)_stacks;
				_stat.values[1].value = damagePerStack * (double)_stacks;
				_owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private float _duration = 1f;

		[SerializeField]
		private double[] _damagePerStackByLevel;

		[Header("Effect")]
		[SerializeField]
		private EffectInfo _gettingStackEffect;

		[SerializeField]
		private EffectInfo _losingEffect;

		[SerializeField]
		private EffectInfo _loopEffect;

		private StatBonus _statBonus;

		public override Key key => Key.Soar;

		protected override IList valuesByLevel => _damagePerStackByLevel;

		protected override void Initialize()
		{
			_statBonus = new StatBonus(base.character, _icon, _gettingStackEffect, _losingEffect, _loopEffect);
			_statBonus.duration = _duration;
			_statBonus.Initialize();
		}

		protected override void UpdateBonus()
		{
			_statBonus.damagePerStack = _damagePerStackByLevel[base.level] * 0.01;
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_statBonus);
		}

		protected override void OnDetach()
		{
			base.character.ability.Remove(_statBonus);
		}
	}
}
