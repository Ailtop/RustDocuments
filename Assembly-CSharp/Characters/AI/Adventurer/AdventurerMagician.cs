using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Magician;
using Characters.AI.Behaviours.Attacks;
using Characters.Movements;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class AdventurerMagician : AdventurerController
	{
		[Header("FireballCombo")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(FireballCombo))]
		private FireballCombo _fireballCombo;

		[Header("PhoenixLanding")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(PhoenixLanding))]
		private PhoenixLanding _phoenixLanding;

		[SerializeField]
		[Subcomponent(typeof(PhoenixLanding))]
		private PhoenixLanding _phoenixLandingLongCool;

		[Header("WorldOnFire")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(WorldOnFire))]
		private WorldOnFire _worldOnFire;

		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[Header("KeepDistance")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Magician.KeepDistance))]
		private Characters.AI.Behaviours.Adventurer.Magician.KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Magician.KeepDistance))]
		private Characters.AI.Behaviours.Adventurer.Magician.KeepDistance _keepDistanceLongDistance;

		[Header("Pattern Management")]
		[Space]
		[Subcomponent(typeof(MagicianPatternSelector))]
		[SerializeField]
		private MagicianPatternSelector _magicianPatternSelector;

		public MagicianPatternSelector magicianPatternSelector => _magicianPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _fireballCombo, _phoenixLanding, _castingSkill, _worldOnFire, _keepDistance };
			character.health.onDie += delegate
			{
				character.movement.config.type = Movement.Config.Type.Walking;
			};
			_sequenceSelector = new MagicianSequenceSelector(this);
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
		}

		public override IEnumerator RunPattern(Pattern pattern)
		{
			if (!base.dead)
			{
				if (pattern is MagicianPattern.KeepDistance)
				{
					yield return CastKeepDistance();
				}
				else if (pattern is MagicianPattern.KeepDistanceLongDistance)
				{
					yield return CastKeepDistanceLongDistance();
				}
				else if (pattern is MagicianPattern.FireballCombo)
				{
					yield return CastFireballCombo();
				}
				else if (pattern is MagicianPattern.PhoenixLanding)
				{
					yield return CastPhoenixLanding();
				}
				else if (pattern is MagicianPattern.PhoenixLandingLongCool)
				{
					yield return CastPhoenixLandingLongCool();
				}
				else if (pattern is MagicianPattern.WorldOnFire)
				{
					yield return TryCastingSkill();
				}
				else if (pattern is MagicianPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is MagicianPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is MagicianPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is MagicianPattern.Runaway)
				{
					yield return CRunaway();
				}
			}
		}

		private IEnumerator TryCastingSkill()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (!base.dead)
			{
				if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CastWorldOnFire();
					yield break;
				}
				character.CancelAction();
				yield return DoGroggy();
			}
		}

		private IEnumerator CastFireballCombo()
		{
			yield return _fireballCombo.CRun(this);
		}

		private IEnumerator CastPhoenixLanding()
		{
			yield return _phoenixLanding.CRun(this);
		}

		private IEnumerator CastPhoenixLandingLongCool()
		{
			yield return _phoenixLandingLongCool.CRun(this);
		}

		private IEnumerator CastWorldOnFire()
		{
			yield return _worldOnFire.CRun(this);
		}

		private IEnumerator CastKeepDistance()
		{
			yield return _keepDistance.CRun(this);
		}

		private IEnumerator CastKeepDistanceLongDistance()
		{
			yield return _keepDistanceLongDistance.CRun(this);
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

		private IEnumerator DoGroggy()
		{
			_groggy.TryStart();
			while (_groggy.running)
			{
				yield return null;
			}
		}

		public bool CanUseWorldOnFire()
		{
			if (CheckSpecialMoveStartCondition())
			{
				return _castingSkill.CanUse();
			}
			return false;
		}

		public bool CanUsePhoneixLanding()
		{
			if (character.health.percent < 0.800000011920929)
			{
				return _phoenixLanding.CanUse();
			}
			return false;
		}

		public bool CanUsePhoneixLandingLongCool()
		{
			if (character.health.percent < 0.800000011920929)
			{
				return _phoenixLandingLongCool.CanUse();
			}
			return false;
		}

		private bool CheckSpecialMoveStartCondition()
		{
			return character.health.percent < 0.60000002384185791;
		}
	}
}
