using System.Collections;
using Characters.Abilities;
using Characters.Operations.Fx;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Mirage : Keyword
	{
		protected class Ability : IAbility, IAbilityInstance
		{
			private readonly Character _owner;

			private readonly Mirage _mirage;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon { get; set; }

			public float iconFillAmount => remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired => false;

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, Mirage mirage)
			{
				_owner = owner;
				_mirage = mirage;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
				if (!(remainTime < 0f))
				{
					remainTime -= deltaTime;
					if (remainTime < 0f)
					{
						_mirage.StartSpawningMotionTrail();
					}
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				_owner.health.onTakeDamage.Add(int.MinValue, OnTakeDamage);
			}

			void IAbilityInstance.Detach()
			{
				_owner.health.onTakeDamage.Remove(OnTakeDamage);
			}

			private bool OnTakeDamage(ref Damage damage)
			{
				if (damage.attackType == Damage.AttackType.None || damage.attackType == Damage.AttackType.Additional)
				{
					return false;
				}
				if (damage.amount < 1.0)
				{
					return false;
				}
				if (remainTime < 0f)
				{
					_mirage.SpawnMotionTrail();
					_mirage.StopSpawningMotionTrail();
					remainTime = duration;
					return true;
				}
				return false;
			}
		}

		[SerializeField]
		private Sprite _icon;

		[Space]
		[SerializeField]
		private MotionTrail _motionTrailOperation;

		[SerializeField]
		private float _motionTrailInterval;

		[Space]
		[SerializeField]
		private float[] _cooldownTimeByLevel = new float[6] { 0f, 50f, 40f, 30f, 20f, 10f };

		private Ability _ability;

		public override Key key => Key.Mirage;

		protected override IList valuesByLevel => _cooldownTimeByLevel;

		protected override void Initialize()
		{
			_ability = new Ability(base.character, this);
			_ability.Initialize();
			_ability.icon = _icon;
		}

		protected override void UpdateBonus()
		{
			_ability.duration = _cooldownTimeByLevel[base.level];
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_ability);
		}

		protected override void OnDetach()
		{
			base.character.ability.Remove(_ability);
			StopSpawningMotionTrail();
		}

		private void SpawnMotionTrail()
		{
			_motionTrailOperation.Run(base.character);
		}

		private void StartSpawningMotionTrail()
		{
			StartCoroutine("CSpawnTrail");
		}

		private void StopSpawningMotionTrail()
		{
			StopCoroutine("CSpawnTrail");
		}

		private IEnumerator CSpawnTrail()
		{
			while (true)
			{
				SpawnMotionTrail();
				yield return Chronometer.global.WaitForSeconds(_motionTrailInterval);
			}
		}
	}
}
