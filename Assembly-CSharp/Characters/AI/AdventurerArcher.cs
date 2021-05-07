using System.Collections;
using System.Collections.Generic;
using Characters.AI.Adventurer;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Adventurer.Archer;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class AdventurerArcher : AdventurerController
	{
		[Header("ArrowShot Attack")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ArrowShot))]
		private ArrowShot _arrowShot;

		[Header("Push Attack")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _pushAttack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _pushAttackShortCool;

		[Header("Arrow Rain")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(ArrowRain))]
		private ArrowRain _arrowRain;

		[Header("Backstep")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Archer.BackStep))]
		private Characters.AI.Behaviours.Adventurer.Archer.BackStep _backstep;

		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Archer.BackStep))]
		private Characters.AI.Behaviours.Adventurer.Archer.BackStep _backstepShortCool;

		[Header("Second Backstep")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Adventurer.Archer.BackStep))]
		private Characters.AI.Behaviours.Adventurer.Archer.BackStep _secondBackstep;

		[Header("BirdHunt")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(BirdHunt))]
		private BirdHunt _birdHunt;

		[Header("Casting")]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(CastingSkill))]
		private CastingSkill _castingSkill;

		[Header("Tool")]
		[Space]
		[SerializeField]
		private Collider2D _minimumDistanceCollider;

		[Header("Pattern Management")]
		[SerializeField]
		[Subcomponent(typeof(ArcherPatternSelector))]
		private ArcherPatternSelector _archerPatternSelector;

		private const float _minDistanceFromWallForBackstep = 2f;

		public ArcherPatternSelector archerPatternSelector => _archerPatternSelector;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _pushAttack, _arrowShot, _arrowRain, _birdHunt, _castingSkill };
			_sequenceSelector = new ArcherSequenceSelector(this);
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
				if (pattern is ArcherPattern.Attack)
				{
					yield return CastAttack();
				}
				else if (pattern is ArcherPattern.MeleeAttack)
				{
					yield return CastMeleeAttack();
				}
				else if (pattern is ArcherPattern.MeleeAttackShortCool)
				{
					yield return CastMeleeAttackShortCool();
				}
				else if (pattern is ArcherPattern.ArrowRain)
				{
					yield return TryArrowRain();
				}
				else if (pattern is ArcherPattern.BirdHunt)
				{
					yield return CastBirdHunt();
				}
				else if (pattern is ArcherPattern.Backstep)
				{
					yield return CastBackStep();
				}
				else if (pattern is ArcherPattern.BackstepShortCool)
				{
					yield return CastBackStepShortCool();
				}
				else if (pattern is ArcherPattern.SecondBackstep)
				{
					yield return CastSecondBackStep();
				}
				else if (pattern is ArcherPattern.SkipableIdle)
				{
					yield return SkipableIdle();
				}
				else if (pattern is ArcherPattern.Idle)
				{
					yield return Idle();
				}
				else if (pattern is ArcherPattern.DrinkPotion)
				{
					yield return DrinkPotion();
				}
				else if (pattern is ArcherPattern.Runaway)
				{
					yield return CRunaway();
				}
			}
		}

		private IEnumerator CastMeleeAttack()
		{
			yield return _pushAttack.CRun(this);
		}

		private IEnumerator CastMeleeAttackShortCool()
		{
			yield return _pushAttackShortCool.CRun(this);
		}

		private IEnumerator CastBackStep()
		{
			yield return _backstep.CRun(this);
		}

		private IEnumerator CastBackStepShortCool()
		{
			yield return _backstepShortCool.CRun(this);
		}

		private IEnumerator CastSecondBackStep()
		{
			yield return _secondBackstep.CRun(this);
		}

		private IEnumerator CastAttack()
		{
			yield return _arrowShot.CRun(this);
		}

		private IEnumerator CastBirdHunt()
		{
			yield return _birdHunt.CRun(this);
			if (MMMaths.RandomBool() && !base.dead)
			{
				yield return _birdHunt.CRun(this);
			}
		}

		private IEnumerator CastArrowRain()
		{
			yield return _arrowRain.CRun(this);
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

		private IEnumerator TryArrowRain()
		{
			character.ForceToLookAt(base.target.transform.position.x);
			yield return _castingSkill.CRun(this);
			if (!base.dead)
			{
				if (_castingSkill.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CastArrowRain();
					yield break;
				}
				character.CancelAction();
				yield return DoGroggy();
			}
		}

		public bool CanUseCastingSkill()
		{
			if (_castingSkill.CanUse())
			{
				return CheckSpecialMoveStartCondition();
			}
			return false;
		}

		public bool CanUsePushAttack()
		{
			return _pushAttack.CanUse();
		}

		public bool CanUseShortPushAttack()
		{
			return _pushAttackShortCool.CanUse();
		}

		public bool CanUseBackstep()
		{
			return _backstep.CanUse();
		}

		public bool CanUseShortBackstep()
		{
			return _backstepShortCool.CanUse();
		}

		public bool IsTargetWithInMinimumDistance()
		{
			return FindClosestPlayerBody(_minimumDistanceCollider);
		}

		public bool CanUseToCastingSkill()
		{
			return _castingSkill.CanUse();
		}

		private bool CheckSpecialMoveStartCondition()
		{
			return character.health.percent < 0.60000002384185791;
		}

		public bool CanUseExtraBackStep()
		{
			if (MMMaths.RandomBool())
			{
				return false;
			}
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			if (base.target.transform.position.x > character.transform.position.x)
			{
				if (Mathf.Abs(bounds.min.x - base.transform.position.x) < 2f)
				{
					return false;
				}
			}
			else if (Mathf.Abs(bounds.max.x - base.transform.position.x) < 2f)
			{
				return false;
			}
			return true;
		}
	}
}
