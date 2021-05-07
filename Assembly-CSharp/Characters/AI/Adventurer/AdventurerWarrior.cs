using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Warrior;
using Characters.AI.Behaviours.Attacks;
using Characters.Movements;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class AdventurerWarrior : AdventurerController
	{
		[Header("Stamping")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Stamping))]
		private Stamping _stamping;

		[Header("Whirlwind")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Whirlwind))]
		private Whirlwind _whirlwind;

		[Header("Earthquake")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Earthquake))]
		private Earthquake _earthquake;

		[Header("Guard")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Guard))]
		private Guard _guard;

		[Header("Rescue")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Rescue))]
		private Rescue _rescue;

		[Header("PowerWave")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(PowerWave))]
		private PowerWave _powerWave;

		[Header("Casting")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[Header("Tool")]
		[Space]
		[SerializeField]
		private Collider2D _earthQuakeTrigger;

		[Header("Pattern Management")]
		[SerializeField]
		[Subcomponent(typeof(WarriorPatternSelector))]
		private WarriorPatternSelector _warriorPatternSelector;

		public WarriorPatternSelector warriorPatternSelector => _warriorPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _stamping, _whirlwind, _earthquake, _guard, _rescue };
			character.health.onDie += delegate
			{
				character.movement.config.type = Movement.Config.Type.Walking;
			};
			_sequenceSelector = new WarriorSequenceSelector(this);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
			character.invulnerable.Attach(this);
			if (_commander == null)
			{
				_commander = GetComponentInParent<Commander>();
			}
		}

		protected override IEnumerator CProcess()
		{
			yield return base.CProcess();
			while (base.target == null)
			{
				yield return null;
			}
		}

		public override IEnumerator RunPattern(Pattern pattern)
		{
			if (!base.dead)
			{
				if (pattern is WarriorPattern.Stamping)
				{
					yield return CastStamping();
				}
				else if (pattern is WarriorPattern.Whirlwind)
				{
					yield return TrySpeacialMove();
				}
				else if (pattern is WarriorPattern.Earthquake)
				{
					yield return CastEarthquake();
				}
				else if (pattern is WarriorPattern.Guard)
				{
					yield return CastGaurd();
				}
				else if (pattern is WarriorPattern.Rescue)
				{
					yield return CastRescue();
				}
				else if (pattern is WarriorPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is WarriorPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is WarriorPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is WarriorPattern.Runaway)
				{
					yield return CRunaway();
				}
				else if (pattern is WarriorPattern.PowerWave)
				{
					yield return CastPowerWave();
				}
			}
		}

		private IEnumerator CastWhirlwind()
		{
			yield return _whirlwind.CRun(this);
		}

		private IEnumerator CastStamping()
		{
			yield return _stamping.CRun(this);
		}

		private IEnumerator CastGaurd()
		{
			yield return _guard.CRun(this);
		}

		private IEnumerator CastEarthquake()
		{
			yield return _earthquake.CRun(this);
		}

		private IEnumerator CastPowerWave()
		{
			yield return _powerWave.CRun(this);
		}

		private IEnumerator CastRescue()
		{
			yield return _rescue.CRun(this);
		}

		private IEnumerator DoGroggy()
		{
			_groggy.TryStart();
			while (_groggy.running)
			{
				yield return null;
			}
		}

		private IEnumerator SkipableIdle()
		{
			if (CanUsePotion())
			{
				yield return _skipableIdle.CRun(this);
				if (!base.dead && _skipableIdle.result == Characters.AI.Behaviours.Behaviour.Result.Fail)
				{
					yield return DrinkPotion();
				}
			}
			else
			{
				yield return _skipableIdle.CRun(this);
			}
		}

		private IEnumerator TrySpeacialMove()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (!base.dead)
			{
				if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CastWhirlwind();
					yield break;
				}
				character.CancelAction();
				yield return DoGroggy();
			}
		}

		public bool CanUseCastingSkill()
		{
			if (character.health.percent < 0.5)
			{
				return _castingSkill.CanUse();
			}
			return false;
		}

		public bool CanUseRescue()
		{
			if (_commander.alives.Count >= 2)
			{
				return _rescue.CanUse(character);
			}
			return false;
		}

		public bool IsTargetWithInEarthQuakeTrigger()
		{
			return FindClosestPlayerBody(_earthQuakeTrigger) != null;
		}
	}
}
